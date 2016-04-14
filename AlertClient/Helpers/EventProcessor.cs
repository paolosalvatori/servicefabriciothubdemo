// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AzureCat.Samples.PayloadEntities;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;

    public class EventProcessor : IEventProcessor
    {
        #region Private Fields

        private EventProcessorFactoryConfiguration configuration;

        #endregion

        #region Public Constructors

        public EventProcessor(EventProcessorFactoryConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            this.configuration = configuration;
        }

        #endregion

        #region Private Static Methods

        private static Alert DeserializeEventData(EventData eventData)
        {
            return JsonConvert.DeserializeObject<Alert>(Encoding.UTF8.GetString(eventData.GetBytes()));
        }

        #endregion

        #region IEventProcessor Methods

        public Task OpenAsync(PartitionContext context)
        {
            try
            {
                // Trace Open Partition
                this.configuration.WriteToLog(
                    $"[EventProcessor].[OpenAsync]:: EventHub=[{context.EventHubPath}] ConsumerGroup=[{context.ConsumerGroupName}] PartitionId=[{context.Lease.PartitionId}]");
            }
            catch (Exception ex)
            {
                // Trace Exception
                this.configuration.WriteToLog($"[EventProcessor].[OpenAsync]:: Exception=[{ex.Message}]");
            }
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                EventData[] eventDatas = events as EventData[] ?? events.ToArray();
                if (events == null || !eventDatas.Any())
                {
                    return;
                }
                IList<EventData> eventDataList = events as IList<EventData> ?? eventDatas.ToList();

                // Trace Process Events
                this.configuration.WriteToLog(
                    $"[EventProcessor].[ProcessEventsAsync]:: EventHub=[{context.EventHubPath}] ConsumerGroup=[{context.ConsumerGroupName}] PartitionId=[{context.Lease.PartitionId}] EventCount=[{eventDataList.Count}]");

                // Trace individual events
                foreach (Alert alert in eventDataList.Select(DeserializeEventData).Where(alert => alert != null))
                {
                    // Trace Payload
                    this.configuration.WriteToLog(
                        $"[Alert] DeviceId=[{alert.DeviceId:000}] " +
                        $"Name=[{alert.Name}] " +
                        $"Value=[{alert.Value:000}] " +
                        $"Timestamp=[{alert.Timestamp}]");

                    // Track event
                    this.configuration.TrackEvent(alert);
                }

                // Checkpoint
                await context.CheckpointAsync();
            }
            catch (AggregateException ex)
            {
                // Trace Exception
                foreach (Exception exception in ex.InnerExceptions)
                {
                    this.configuration.WriteToLog(exception.Message);
                }
            }
            catch (Exception ex)
            {
                // Trace Exception
                this.configuration.WriteToLog($"[EventProcessor].[ProcessEventsAsync]:: Exception=[{ex.Message}]");
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            try
            {
                // Trace Open Partition
                this.configuration.WriteToLog(
                    $"[EventProcessor].[CloseAsync]:: EventHub=[{context.EventHubPath}] ConsumerGroup=[{context.ConsumerGroupName}] PartitionId=[{context.Lease.PartitionId}] Reason=[{reason}]");

                if (reason == CloseReason.Shutdown)
                {
                    await context.CheckpointAsync();
                }
            }
            catch (LeaseLostException)
            {
            }
            catch (Exception ex)
            {
                // Trace Exception
                this.configuration.WriteToLog($"[EventProcessor].[CloseAsync]:: Exception=[{ex.Message}]");
            }
        }

        #endregion
    }
}