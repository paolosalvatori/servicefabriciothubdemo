// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.PayloadEntities
{
    using System;
    using Newtonsoft.Json;

    public class Payload
    {
        /// <summary>
        /// Gets or sets the device id.
        /// </summary>
        [JsonProperty(PropertyName = "deviceId", Order = 1)]
        public long DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 2)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the device value.
        /// </summary>
        [JsonProperty(PropertyName = "value", Order = 3)]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 4)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp", Order = 5)]
        public DateTime Timestamp { get; set; }
    }
}