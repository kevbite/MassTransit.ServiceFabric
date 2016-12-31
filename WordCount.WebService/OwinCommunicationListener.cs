using System;
using System.Diagnostics;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Kevsoft.WordCount.WebService
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly ServiceContext _serviceContext;

        /// <summary>
        /// OWIN server handle.
        /// </summary>
        private IDisposable _serverHandle;

        private readonly IOwinAppBuilder _startup;
        private string _publishAddress;
        private string _listeningAddress;
        private readonly string _appRoot;
        
        public OwinCommunicationListener(string appRoot, IOwinAppBuilder startup, ServiceContext serviceContext)
        {
            _startup = startup;
            _appRoot = appRoot;
            _serviceContext = serviceContext;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Initialize");

            var serviceEndpoint = this._serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var port = serviceEndpoint.Port;

            var statefulServiceContext = _serviceContext as StatefulServiceContext;
            if (statefulServiceContext != null )
            {
                var statefulInitParams = statefulServiceContext;

                _listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}/{2}/{3}/",
                    port,
                    statefulInitParams.PartitionId,
                    statefulInitParams.ReplicaId,
                    Guid.NewGuid());
            }
            else
            {
                var statelessServiceContext = _serviceContext as StatelessServiceContext;
                if (statelessServiceContext != null)
                {
                    _listeningAddress = string.Format(
                        CultureInfo.InvariantCulture,
                        "http://+:{0}/{1}",
                        port,
                        string.IsNullOrWhiteSpace(_appRoot)
                            ? ""
                            : _appRoot.TrimEnd('/') + '/');
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            _publishAddress = _listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            Trace.WriteLine($"Opening on {_publishAddress}");

            try
            {
                Trace.WriteLine($"Starting web server on {_listeningAddress}");

                _serverHandle = WebApp.Start(_listeningAddress, appBuilder => _startup.Configuration(appBuilder));

                return Task.FromResult(_publishAddress);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);

                StopWebServer();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Close");

            StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            Trace.WriteLine("Abort");

            StopWebServer();
        }

        private void StopWebServer()
        {
            if (_serverHandle != null)
            {
                try
                {
                    _serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}