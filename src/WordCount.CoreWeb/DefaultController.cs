using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kevsoft.WordCount.Messages;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WordCount.CoreWeb
{
    [Produces("application/json")]
    [Route("api/Default")]
    public class DefaultController : ControllerBase
    {
        private readonly IBus _bus;

        public DefaultController(IBus bus)
        {
            _bus = bus;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> Count()
        {
            var address = new Uri("rabbitmq://localhost/WordCount.Service");
            var requestTimeout = TimeSpan.FromSeconds(30);

            var client = new MessageRequestClient<ICountRequest, ICountResponse>(_bus, address, requestTimeout);

            var response = await client.Request(new CountRequest(), CancellationToken.None).ConfigureAwait(false);

            return Content($"<h1> Total: {response.Count} </h1>", "text/html", Encoding.UTF8);
        }

        [HttpPost("AddWord/{word}")]
        public async Task<IActionResult> AddWord(string word)
        {
            await _bus.Publish<IAddWord>(new { Word = word }).ConfigureAwait(false);

            return Content($"<h1>{word}</h1> added", "text/html", Encoding.UTF8);
        }
    }
}