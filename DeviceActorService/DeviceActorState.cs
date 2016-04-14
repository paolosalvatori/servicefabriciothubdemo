// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.DeviceActorService
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.AzureCat.Samples.PayloadEntities;

    [Serializable]
    [DataContract]
    public class DeviceActorState
    {
        [DataMember]
        public Queue<Payload> Queue { get; set; }

        [DataMember]
        public Device Data { get; set; }
    }
}