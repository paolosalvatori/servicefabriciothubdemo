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

    public class Alert : Device
    {
        /// <summary>
        /// Gets or sets the device value.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}