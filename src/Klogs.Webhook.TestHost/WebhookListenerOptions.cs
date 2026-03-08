using System;

namespace Klogs.Webhook.TestHost
{
    internal class WebhookListenerOptions
    {
        public string Key { get; set; }

        public string Host { get; set; }

        public string Api { get; set; }

        public Uri ConvertApi2SocketUri()
        {
            return new Uri($"wss://{new Uri(Api).Authority}/ws?key={Key}");
        }
    }
}
