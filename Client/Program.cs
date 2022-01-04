using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace OTELClient // Note: actual namespace depends on the project name.
{
    public class Program
    {
        static string serviceName = "client-service";
        static string serviceVersion = "1.0.0";
        public static readonly ActivitySource MyActivitySource = new(serviceName);

        public static void Main(string[] args)
        {
            

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(o =>
                {
                    o.BatchExportProcessorOptions = new OpenTelemetry.BatchExportProcessorOptions<System.Diagnostics.Activity>()
                    {
                        MaxExportBatchSize = 64
                    };
                })
                .AddSource(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                .SetSampler(new AlwaysOnSampler())
                .Build();

            HttpClient client = new HttpClient();
            string helloWorld = string.Empty;

            using var myActivity = MyActivitySource.StartActivity("Client");


            for (int i = 0; i < 2; i++)
            {
                using (var childActivity = MyActivitySource.StartActivity($"ChildClient-{i}"))
                {
                    childActivity.AddBaggage("project.id", "123");
                    helloWorld = client.GetStringAsync("https://localhost:44309/Hello").GetAwaiter().GetResult();
                }
            }

            Console.WriteLine(helloWorld);
            Console.ReadLine();
        }
    }
}