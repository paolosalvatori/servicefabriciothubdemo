// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using Microsoft.AzureCat.Samples.PayloadEntities;

    public class EventProcessorFactoryConfiguration
    {
        #region Public Properties

        public Action<Alert> TrackEvent { get; set; }

        public Action<string> WriteToLog { get; set; }

        #endregion
    }
}