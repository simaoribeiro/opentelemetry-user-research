using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<HelloController> _logger;
        public static readonly ActivitySource MyActivitySource = new ("server-service");

        public HelloController(ILogger<HelloController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetHello")]
        public string Get()
        {
            var activity = Activity.Current;
            var baggageItem = activity.GetBaggageItem("project.id");
            activity?.SetTag("foo", "bar");
            activity?.SetTag("http.route", "Ola");
            activity?.SetTag("baggage", baggageItem);
            activity.DisplayName = "Ola";

            Thread.Sleep(1000);

            using (var childActivity = MyActivitySource.StartActivity("ChildHello"))
            {
                try
                {

                    Thread.Sleep(1000);
                    throw new System.Exception();
                }
                catch (Exception ex)
                {
                    ActivityTagsCollection collection = new ActivityTagsCollection();
                    collection.Add("exception", ex);
                    childActivity.AddEvent(new ActivityEvent("exception", default, collection));
                    childActivity.SetCustomProperty("exception2", collection);
                    childActivity.SetTag("error", true);
                }
            }

            return "Hello World";
        }
    }
}