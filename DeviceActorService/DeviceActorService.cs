// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.DeviceActorService
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class DeviceActorService : ActorService
    {
        #region Public Constructor

        public DeviceActorService(
            StatefulServiceContext context,
            ActorTypeInformation typeInfo,
            Func<ActorBase> actorFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            : base(context, typeInfo, actorFactory, stateProvider, settings)
        {
            // Read settings from the DeviceActorServiceConfig section in the Settings.xml file
            ICodePackageActivationContext activationContext = this.Context.CodePackageActivationContext;
            ConfigurationPackage config = activationContext.GetConfigurationPackageObject(ConfigurationPackage);
            ConfigurationSection section = config.Settings.Sections[ConfigurationSection];

            // Read the ServiceBusConnectionString setting from the Settings.xml file
            ConfigurationProperty parameter = section.Parameters[ServiceBusConnectionStringParameter];
            if (!string.IsNullOrWhiteSpace(parameter?.Value))
            {
                this.ServiceBusConnectionString = parameter.Value;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(ParameterCannotBeNullFormat, ServiceBusConnectionStringParameter),
                    ServiceBusConnectionStringParameter);
            }

            // Read the EventHubName setting from the Settings.xml file
            parameter = section.Parameters[EventHubNameParameter];
            if (!string.IsNullOrWhiteSpace(parameter?.Value))
            {
                this.EventHubName = parameter.Value;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(ParameterCannotBeNullFormat, EventHubNameParameter),
                    EventHubNameParameter);
            }

            // Read the QueueLength setting from the Settings.xml file
            parameter = section.Parameters[QueueLengthParameter];
            if (!string.IsNullOrWhiteSpace(parameter?.Value))
            {
                this.QueueLength = DefaultQueueLength;
                int queueLength;
                if (int.TryParse(parameter.Value, out queueLength))
                {
                    this.QueueLength = queueLength;
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format(ParameterCannotBeNullFormat, QueueLengthParameter),
                    QueueLengthParameter);
            }
        }

        #endregion

        #region Private Constants

        //************************************
        // Parameters
        //************************************
        private const string ConfigurationPackage = "Config";
        private const string ConfigurationSection = "DeviceActorServiceConfig";
        private const string ServiceBusConnectionStringParameter = "ServiceBusConnectionString";
        private const string EventHubNameParameter = "EventHubName";
        private const string QueueLengthParameter = "QueueLength";

        //************************************
        // Formats
        //************************************
        private const string ParameterCannotBeNullFormat = "The parameter [{0}] is not defined in the Setting.xml configuration file.";

        //************************************
        // Constants
        //************************************
        private const int DefaultQueueLength = 100;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the service bus connection string
        /// </summary>
        public string ServiceBusConnectionString { get; private set; }

        /// <summary>
        /// Gets or sets the event hub name
        /// </summary>
        public string EventHubName { get; private set; }

        /// <summary>
        /// Gets or sets the queue length
        /// </summary>
        public int QueueLength { get; private set; }

        #endregion
    }
}