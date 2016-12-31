using System;
using System.Collections.Generic;
using System.Fabric;
using Kevsoft.MassTransit.ServiceFabric;
using Kevsoft.WordCount.Service.Consumers;
using MassTransit;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Kevsoft.WordCount.Service
{
    /// <summary>
    /// Sample Service Fabric persistent service for counting words.
    /// </summary>
    public class WordCountService : StatefulService
    {
        public const string ServiceEventSourceName = "WordCountService";

        /// <summary>
        /// Initializes a new instance of the <see cref="WordCountService"/> class. 
        /// </summary>
        public WordCountService(StatefulServiceContext context)
            : base(context)
        {
            ServiceEventSource.Current.ServiceInstanceConstructed(ServiceEventSourceName);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            ServiceEventSource.Current.CreateCommunicationListener(ServiceEventSourceName);

            return new[]
            {
                new ServiceReplicaListener(context => new MassTransitListener(CreateBus(StateManager)))
            };
        }

        private IBusControl CreateBus(IReliableStateManager stateManager)
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost/"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("WordCount.Service", c =>
                {
                    c.Consumer(() => new AddWordConsumer(stateManager));
                    c.Consumer(() => new CountRequestConsumer(stateManager));
                });
            });

            return busControl;
        }
    }
}