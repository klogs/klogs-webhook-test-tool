using System.Collections.Generic;

namespace Klogs.Webhook.TestHost
{
    public class TestWebhook
    {
        public string WebhookId { get; set; }

        public string Target { get; set; }

        public string HttpMethod { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string, string> HttpHeader { get; set; }

        public Dictionary<string, object> RequestData { get; set; }

        public string HashKey { get; set; }
    }
}
