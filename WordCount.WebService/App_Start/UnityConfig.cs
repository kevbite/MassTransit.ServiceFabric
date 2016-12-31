using System.Web.Http;
using Kevsoft.WordCount.WebService.Controllers;
using MassTransit;
using Microsoft.Practices.Unity;
using Unity.WebApi;

namespace Kevsoft.WordCount.WebService
{
    /// <summary>
    /// Configures dependency injection for Controllers using a Unity container. 
    /// </summary>
    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config, IBus bus)
        {
            UnityContainer container = new UnityContainer();

            // The default controller needs a bus to perform operations.
            // Using the DI container, we can inject it as a dependency.
            container.RegisterType<DefaultController>(
                new TransientLifetimeManager(),
                new InjectionConstructor(bus));

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}