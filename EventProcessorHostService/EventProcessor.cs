// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.EventProcessorHostService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AzureCat.Samples.DeviceActorService.Interfaces;
    using Microsoft.AzureCat.Samples.PayloadEntities;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Newtonsoft.Json;

    public class EventProcessor : IEventProcessor
    {
        #region Private Constants

        private const string DeviceActorServiceUriCannotBeNull = "DeviceActorServiceUri setting cannot be null";

        #endregion

        #region Private Static Fields

        private static readonly Dictionary<long, IDeviceActor> actorProxyDictionary = new Dictionary<long, IDeviceActor>();

        #endregion

        #region Private Fields

        private readonly Uri serviceUri;

        #endregion

        #region Public Constructors

        public EventProcessor(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentNullException(DeviceActorServiceUriCannotBeNull);
            }
            this.serviceUri = new Uri(parameter);
        }

        #endregion

        #region IEventProcessor Methods

        public Task OpenAsync(PartitionContext context)
        {
            ServiceEventSource.Current.Message(
                $"Lease acquired: EventHub=[{context.EventHubPath}] ConsumerGroup=[{context.ConsumerGroupName}] PartitionId=[{context.Lease.PartitionId}]");
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                if (events == null)
                {
                    return;
                }
                IList<EventData> eventDataList = events as IList<EventData> ?? events.ToList();

                // Trace individual events
                foreach (Payload payload in eventDataList.Select(DeserializeEventData))
                {
                    // Invoke Actor
                    if (payload == null)
                    {
                        continue;
                    }

                    // Invoke Device Actor
                    IDeviceActor proxy = this.GetActorProxy(payload.DeviceId);
                    if (proxy != null)
                    {
                        await proxy.ProcessEventAsync(payload);
                    }
                }

                // Checkpoint
                await context.CheckpointAsync();
            }
            catch (LeaseLostException ex)
            {
                // Trace Exception as message
                ServiceEventSource.Current.Message(ex.Message);
            }
            catch (AggregateException ex)
            {
                // Trace Exception
                foreach (Exception exception in ex.InnerExceptions)
                {
                    ServiceEventSource.Current.Message(exception.Message);
                }
            }
            catch (Exception ex)
            {
                // Trace Exception
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            try
            {
                ServiceEventSource.Current.Message(
                    $"Lease lost: EventHub=[{context.EventHubPath}] ConsumerGroup=[{context.ConsumerGroupName}] PartitionId=[{context.Lease.PartitionId}]");
                if (reason == CloseReason.Shutdown)
                {
                    await context.CheckpointAsync();
                }
            }
            catch (Exception ex)
            {
                // Trace Exception
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        #endregion

        #region Private Static Methods

        private static Payload DeserializeEventData(EventData eventData)
        {
            return JsonConvert.DeserializeObject<Payload>(Encoding.UTF8.GetString(eventData.GetBytes()));
        }

        private IDeviceActor GetActorProxy(long deviceId)
        {
            lock (actorProxyDictionary)
            {
                if (actorProxyDictionary.ContainsKey(deviceId))
                {
                    return actorProxyDictionary[deviceId];
                }
                actorProxyDictionary[deviceId] = ActorProxy.Create<IDeviceActor>(new ActorId($"device{deviceId}"), this.serviceUri);
                return actorProxyDictionary[deviceId];
            }
        }

        #endregion
    }
}