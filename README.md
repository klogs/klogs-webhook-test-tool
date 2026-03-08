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

- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) or later (only needed when building from source)
- A Klogs account and a webhook test key

## Download Pre-Built Binaries

Ready-to-use self-contained executables (no .NET installation required) are available on the [releases page](https://github.com/klogs/klogs-webhook-test-tool/releases/tag/latest):

| Platform        | File to download                  |
|-----------------|-----------------------------------|
| Windows x64     | `webhook_win-x64.zip`             |
| Linux x64       | `webhook_linux-x64.zip`           |
| macOS x64       | `webhook_osx-x64.zip`             |
| macOS ARM64     | `webhook_osx-arm64.zip`           |

Extract the archive and run the `webhook` executable directly — no installation needed.

## Getting Started

### Build from Source

```bash
git clone https://github.com/klogs/klogs-webhook-test-tool.git
cd klogs-webhook-test-tool
dotnet build src/Klogs.Webhook.TestHost/Klogs.Webhook.TestHost.csproj
```

The compiled executable will be `webhook` (or `webhook.exe` on Windows).

### Build Self-Contained Executables for All Platforms

The repository includes build scripts that produce self-contained, single-file executables for all supported platforms at once. Output is written to the `publish/` directory.

**Windows — run `build.bat`:**

```bat
build.bat
```

**Linux / macOS — run `build.sh`:**

```bash
chmod +x build.sh
./build.sh
```

Both scripts target the following runtimes:

| Platform    | Runtime Identifier |
|-------------|--------------------|
| Windows x64 | `win-x64`          |
| Linux x64   | `linux-x64`        |
| macOS x64   | `osx-x64`          |
| macOS ARM64 | `osx-arm64`        |

## Usage

```
webhook --key <KEY> --host <LOCAL_URL> [--api <API_URL>]
```

### Options

| Option     | Alias | Description                                                   | Required | Default                    |
|------------|-------|---------------------------------------------------------------|----------|----------------------------|
| `--key`    | `-k`  | Webhook test key from the Klogs Dashboard                     | Yes      | —                          |
| `--host`   | `-h`  | Local application URL (e.g. `https://localhost:44325`)        | Yes      | —                          |
| `--api`    | `-a`  | Klogs webhook relay API address                               | No       | `https://hook.klogs.dev`   |

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
