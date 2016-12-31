using System.Threading.Tasks;
using Kevsoft.WordCount.Messages;
using MassTransit;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Kevsoft.WordCount.Service.Consumers
{
    public class AddWordConsumer : IConsumer<IAddWord>
    {
        private readonly IReliableStateManager _stateManager;

        public AddWordConsumer(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task Consume(ConsumeContext<IAddWord> context)
        {
            var wordCountDictionary =
                await _stateManager.GetOrAddAsync<IReliableDictionary<string, long>>("wordCountDictionary")
                    .ConfigureAwait(false);

            var statsDictionary =
                await _stateManager.GetOrAddAsync<IReliableDictionary<string, long>>("statsDictionary")
                    .ConfigureAwait(false);

            using (var tx = _stateManager.CreateTransaction())
            {
                var word = context.Message.Word;

                await wordCountDictionary.AddOrUpdateAsync(
                    tx,
                    word,
                    1,
                    (key, oldValue) => oldValue + 1).ConfigureAwait(false);

                await statsDictionary.AddOrUpdateAsync(
                    tx,
                    "Number of Words Processed",
                    1,
                    (key, oldValue) => oldValue + 1).ConfigureAwait(false);

                await tx.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}