using System.Threading.Tasks;
using Kevsoft.WordCount.Messages;
using MassTransit;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Kevsoft.WordCount.Service.Consumers
{
    public class CountRequestConsumer : IConsumer<ICountRequest>
    {
        private readonly IReliableStateManager _stateManager;

        public CountRequestConsumer(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task Consume(ConsumeContext<ICountRequest> context)
        {
            var statsDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, long>>("statsDictionary").ConfigureAwait(false);

            using (var tx = _stateManager.CreateTransaction())
            {
                var result = await statsDictionary.TryGetValueAsync(tx, "Number of Words Processed").ConfigureAwait(false);

                if (result.HasValue)
                {
                    await context.RespondAsync<ICountResponse>(new { Count = result.Value }).ConfigureAwait(false);
                }
            }
        }
    }
}
