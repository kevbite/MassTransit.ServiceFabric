using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Kevsoft.WordCount.Messages;
using MassTransit;

namespace Kevsoft.WordCount.WebService.Controllers
{
    [RoutePrefix("api")]
    public class DefaultController : ApiController
    {
        private readonly IBus _bus;

        public DefaultController(IBus bus)
        {
            _bus = bus;
        }

        [HttpGet]
        [Route("Count")]
        public async Task<HttpResponseMessage> Count()
        {
            var address = new Uri("rabbitmq://localhost/WordCount.Service");
            var requestTimeout = TimeSpan.FromSeconds(30);

            var client = new MessageRequestClient<ICountRequest, ICountResponse>(_bus, address, requestTimeout);

            var response = await client.Request(new CountRequest(), CancellationToken.None).ConfigureAwait(false);

            return new HttpResponseMessage()
            {
                Content = new StringContent($"<h1> Total: {response.Count} </h1>", Encoding.UTF8, "text/html")
            };
        }

        [HttpPost]
        [Route("AddWord/{word}")]
        public async Task<HttpResponseMessage> AddWord(string word)
        {
            await _bus.Publish<IAddWord>(new { Word = word }).ConfigureAwait(false);

            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    $"<h1>{word}</h1> added",
                    Encoding.UTF8,
                    "text/html")
            };
        }
    }
}