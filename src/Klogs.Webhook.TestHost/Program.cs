using Klogs.Webhook.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Websocket.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(@"
  _    _                 
 | |  | |                
 | | _| | ___   __ _ ___ 
 | |/ / |/ _ \ / _` / __|
 |   <| | (_) | (_| \__ \
 |_|\_\_|\___/ \__, |___/
                __/ |    
               |___/     
");

        Console.WriteLine("KLOGS YAZILIM VE BİLİŞİM TEKNOLOJİLERİ LTD. ŞTİ.");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[WEBHOOK TEST TOOL]");
        Console.WriteLine();

        Console.ResetColor();

        var rootCommand = new RootCommand("Klogs Webhook Test Application")
        {
            new Option<string>("--key", "-k")
            {
                Description = "Webhook test key from https://webhookui.klogs.io (Production) | https://webhookui.klogs.dev (Sandbox)",
                Required = true
            },

            new Option<string>("--host", "-h")
            {
                Description =  "Application local url for example: https://localhost:44325",
                Required = true
            },

            new Option<string>("--api", "-a")
            {
                Description = "Webhook API address.",
                Required = false,
                DefaultValueFactory = pr => "https://hook.klogs.dev"
            }
        };

        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count > 0)
        {
            foreach (var error in parseResult.Errors)
            {
                Console.Error.WriteLine(error.Message);
            }

            return;
        }

        rootCommand.SetAction(async (result, ct) =>
        {
            var key = result.GetValue<string>("--key");
            var host = result.GetValue<string>("--host");
            var api = result.GetValue<string>("--api");

            var builder = Host.CreateDefaultBuilder(args)
                              .ConfigureServices((context, services) =>
                              {
                                  services.Configure<WebhookListenerOptions>(opt =>
                                  {
                                      opt.Key = key;
                                      opt.Host = host;
                                      opt.Api = api;
                                  });

                                  services.AddSerilog(logger =>
                                  {
                                      logger.WriteTo.Console(Serilog.Events.LogEventLevel.Information);

                                      logger.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
                                      logger.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
                                  });

                                  services.AddHttpClient();

                                  services.AddSingleton<WebsocketClient>(sp =>
                                  {
                                      var opt = sp.GetService<IOptions<WebhookListenerOptions>>().Value;

                                      return new WebsocketClient(opt.ConvertApi2SocketUri());
                                  });

                                  services.AddHostedService<WebhookListenerHostedService>();
                              });

            var app = builder.Build();

            try
            {
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                await File.WriteAllTextAsync("startup_error.log", ex.ToString());
            }
        });

        await parseResult.InvokeAsync();
    }
}