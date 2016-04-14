// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

// ReSharper disable once CheckNamespace

namespace Microsoft.AzureCat.Samples.DeviceActorService.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.AzureCat.Samples.PayloadEntities;
    using Microsoft.ServiceFabric.Actors;

    public interface IDeviceActor : IActor
    {
        Task ProcessEventAsync(Payload payload);
        Task SetData(Device data);
        Task<Device> GetData();
    }
}