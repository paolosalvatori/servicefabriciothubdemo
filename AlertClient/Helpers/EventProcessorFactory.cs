// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    public class EventProcessorFactory<T> : IEventProcessorFactory where T : class, IEventProcessor
    {
        #region IEventProcessorFactory Methods

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return this.instance ?? Activator.CreateInstance(typeof(T), this.configuration) as T;
        }

        #endregion

        #region Private Fields

        private readonly T instance;
        private readonly EventProcessorFactoryConfiguration configuration;

        #endregion

        #region Public Constructors

        public EventProcessorFactory()
        {
            this.configuration = null;
        }

        public EventProcessorFactory(EventProcessorFactoryConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public EventProcessorFactory(T instance)
        {
            this.instance = instance;
        }

        #endregion
    }
}