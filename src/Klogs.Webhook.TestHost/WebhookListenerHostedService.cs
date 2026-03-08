using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Klogs.Webhook.TestHost
{
    internal class WebhookListenerHostedService : IHostedService
    {
        private readonly ILogger<WebhookListenerHostedService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WebsocketClient _client;

        public WebhookListenerHostedService(
            ILogger<WebhookListenerHostedService> logger,
            IHttpClientFactory httpClientFactory,
            WebsocketClient websocketClient)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _client = websocketClient;
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private Timer _timer;

        //private static readonly ManualResetEvent exit = new ManualResetEvent(false);

        private const string HART_BEAT = "Hartbeat";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(state => 
            {
                _client.Send(HART_BEAT);

            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            _logger.LogInformation("Webhook listening...");

            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);

            _client.ReconnectionHappened.Subscribe(info =>
            {
                _logger.LogInformation("Reconnected to webhook.");
            });

            _client.MessageReceived.Subscribe(async msg =>
            {
                if (msg.Text == HART_BEAT)
                {
                    return;
                }
                
                _logger.LogInformation("Webhook message received: {Message}", msg.Text);

                using (var http = _httpClientFactory.CreateClient())
                {
                    try
                    {
                        var webhook = JsonSerializer.Deserialize<TestWebhook>(msg.Text, JsonOptions);

                        var req = new HttpRequestMessage
                        {
                            Content = CreateHttpContent(webhook),
                            Method = HttpMethod.Parse(webhook.HttpMethod),
                            RequestUri = new Uri(webhook.Target)
                        };

                        if (webhook.HttpHeader != null)
                        {
                            foreach (var header in webhook.HttpHeader)
                            {
                                req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                        }

                        var response = await http.SendAsync(req);

                        var responseContent = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Webhook send complete. {HttpStatusCode} {Content}", response.StatusCode, responseContent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while sending webhook request");
                    }
                }
            });
            
            _client.Start();

            Task.Run(() =>
            {
                _client.Send(HART_BEAT);
            });

            _logger.LogInformation("For exit press Ctrl+C.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Webhook stopped!");

            return Task.CompletedTask;
        }

        private static readonly CompositeFormat _format = CompositeFormat.Parse("{0}");

        private static HttpContent CreateHttpContent(TestWebhook webhook)
        {
            if (webhook.RequestData == null)
            {
                return null;
            }

            if (webhook.ContentType == MediaTypeNames.Application.Json)
            {
                return new StringContent(
                    content: JsonSerializer.Serialize(webhook.RequestData, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    }),
                    encoding: Encoding.UTF8,
                    mediaType: MediaTypeNames.Application.Json
                );
            }

            return new FormUrlEncodedContent(
                webhook.RequestData.ToDictionary(x => x.Key, x => string.Format(CultureInfo.InvariantCulture, _format, x.Value))
            );
        }
    }
}
