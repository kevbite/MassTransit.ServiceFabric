using System;
using System.Collections.Generic;
using System.Fabric;
using Kevsoft.MassTransit.ServiceFabric;
using MassTransit;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Kevsoft.WordCount.WebService
{
    /// <summary>
    /// Service that handles front-end web requests and acts as a proxy to the back-end data for the UI web page.
    /// It is a stateless service that hosts a Web API application on OWIN.
    /// </summary>
    public class WordCountWebService : StatelessService
    {
        public WordCountWebService(StatelessServiceContext context)
            : base(context)
        {
        }

        private static IBusControl CreateBus()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost/"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            return busControl;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var bus = CreateBus();

            return new []
            {
                new ServiceInstanceListener(context => new OwinCommunicationListener("wordcount", new Startup(bus), context), "owin"),
                new ServiceInstanceListener(_ => new MassTransitListener(bus), "masstransit")
            };
        }
    }
}   