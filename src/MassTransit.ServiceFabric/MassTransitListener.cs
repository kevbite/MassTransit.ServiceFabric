using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Kevsoft.MassTransit.ServiceFabric
{
    public class MassTransitListener : ICommunicationListener
    {
        private readonly IBusControl _busControl;

        public MassTransitListener(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Initialize");
            await _busControl.StartAsync(cancellationToken).ConfigureAwait(false);
            
            Trace.WriteLine($"Connection to {this._busControl.Address}");

            return this._busControl.Address.AbsoluteUri;
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Close");

            await _busControl.StopAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public void Abort()
        {
            Trace.WriteLine("Abort");

            _busControl.Stop();
        }
    }
}