# klogs-webhook-test-tool

A CLI tool that lets you receive [Klogs](https://klogs.io) webhook events on your **local development machine** — no public URL or tunneling setup required.

## How It Works

```
[Klogs Webhook Service] ──WebSocket──▶ [webhook tool] ──HTTP──▶ [localhost app]
```

1. You get a **webhook test key** from the Klogs Dashboard.
2. You run this tool with that key and your local application's URL.
3. The tool opens a persistent WebSocket connection to the Klogs webhook relay service.
4. When a webhook event is triggered, the relay sends it over the WebSocket to this tool.
5. The tool reconstructs and forwards the original HTTP request (method, headers, body) to your local application.

## Prerequisites

- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) or later (only needed if not using a self-contained build)
- A Klogs account and a webhook test key

## Getting Started

### Build from Source

```bash
git clone https://github.com/klogs/klogs-webhook-test-tool.git
cd klogs-webhook-test-tool
dotnet build src/Klogs.Webhook.TestHost/Klogs.Webhook.TestHost.csproj
```

The compiled executable will be `webhook` (or `webhook.exe` on Windows).

### Publish as a Self-Contained Single Executable

```bash
dotnet publish src/Klogs.Webhook.TestHost/Klogs.Webhook.TestHost.csproj -c Release -r win-x64 --self-contained
```

> Replace `win-x64` with your target runtime identifier — e.g., `linux-x64` or `osx-x64`.

## Usage

```
webhook --key <KEY> --host <LOCAL_URL> [--api <API_URL>]
```

### Options

| Option  | Alias | Description                                                   | Required | Default                    |
|---------|-------|---------------------------------------------------------------|----------|----------------------------|
| `--key` | `-k`  | Webhook test key from the Klogs Dashboard                     | Yes      | —                          |
| `--host`| `-h`  | Local application URL (e.g. `https://localhost:44325`)        | Yes      | —                          |
| `--api` | `-a`  | Klogs webhook relay API address                               | No       | `https://hook.klogs.dev`   |

### Webhook Test Key

Log in to the Klogs Dashboard to generate a test key:

| Environment | Dashboard URL                        |
|-------------|--------------------------------------|
| Sandbox     | https://webhookui.klogs.dev          |
| Production  | https://webhookui.klogs.io           |

## Examples

**Sandbox (default):**

```bash
webhook --key YOUR_TEST_KEY --host https://localhost:44325
```

**Production API:**

```bash
webhook --key YOUR_TEST_KEY --host https://localhost:44325 --api https://hook.klogs.io
```

**Using short aliases:**

```bash
webhook -k YOUR_TEST_KEY -h https://localhost:44325
```

## What Gets Forwarded

When a webhook event arrives, the tool reconstructs the original HTTP request targeting your local application:

| Attribute      | Behaviour                                                                                   |
|----------------|---------------------------------------------------------------------------------------------|
| **Method**     | Preserved as-is (`POST`, `PUT`, `PATCH`, etc.)                                              |
| **Headers**    | All custom headers from the original request are forwarded                                   |
| **Body**       | Forwarded as `application/json` or `application/x-www-form-urlencoded` based on content type|

The HTTP status code and response body returned by your local application are logged to the console.

## Stopping the Tool

Press `Ctrl+C` to gracefully stop the tool.

## License

See [LICENSE](LICENSE) for details.
