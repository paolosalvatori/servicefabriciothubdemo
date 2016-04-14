// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directices



#endregion

namespace Microsoft.AzureCat.Samples.DeviceManagementWebService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AzureCat.Samples.DeviceActorService.Interfaces;
    using Microsoft.AzureCat.Samples.PayloadEntities;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;

    public class DeviceController : ApiController
    {
        #region Private Static Fields

        private static readonly Dictionary<long, IDeviceActor> actorProxyDictionary = new Dictionary<long, IDeviceActor>();

        #endregion

        #region Private Static Methods

        private IDeviceActor GetActorProxy(long deviceId)
        {
            lock (actorProxyDictionary)
            {
                if (actorProxyDictionary.ContainsKey(deviceId))
                {
                    return actorProxyDictionary[deviceId];
                }
                actorProxyDictionary[deviceId] = ActorProxy.Create<IDeviceActor>(
                    new ActorId($"device{deviceId}"),
                    new Uri(OwinCommunicationListener.DeviceActorServiceUri));
                return actorProxyDictionary[deviceId];
            }
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public async Task<Device> GetDevice(long id)
        {
            try
            {
                IDeviceActor proxy = this.GetActorProxy(id);
                if (proxy != null)
                {
                    return await proxy.GetData();
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions?.Count > 0)
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        ServiceEventSource.Current.Message(exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
            return null;
        }

        [HttpPost]
        [Route("api/devices/get")]
        public async Task<IEnumerable<Device>> GetDevices(IEnumerable<long> ids)
        {
            try
            {
                IList<long> enumerable = ids as IList<long> ?? ids.ToList();
                if (ids == null || !enumerable.Any())
                {
                    return null;
                }
                List<Device> deviceList = new List<Device>();
                foreach (long id in enumerable)
                {
                    IDeviceActor proxy = this.GetActorProxy(id);
                    if (proxy != null)
                    {
                        deviceList.Add(await proxy.GetData());
                    }
                }
                return deviceList;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions?.Count > 0)
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        ServiceEventSource.Current.Message(exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
            return null;
        }

        [HttpPost]
        public async Task SetDevice(Device device)
        {
            try
            {
                IDeviceActor proxy = this.GetActorProxy(device.DeviceId);
                if (proxy != null)
                {
                    await proxy.SetData(device);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions?.Count > 0)
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        ServiceEventSource.Current.Message(exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/devices/set")]
        public async Task SetDevices(IEnumerable<Device> devices)
        {
            try
            {
                IList<Device> enumerable = devices as IList<Device> ?? devices.ToList();
                if (devices == null || !enumerable.Any())
                {
                    return;
                }
                foreach (Device device in enumerable)
                {
                    IDeviceActor proxy = this.GetActorProxy(device.DeviceId);
                    if (proxy != null)
                    {
                        await proxy.SetData(device);
                    }
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions?.Count > 0)
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        ServiceEventSource.Current.Message(exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        #endregion
    }
}