# Sendbyte .NET SDK

A .NET SDK for the SendByte API.

> Status: early development. The SDK foundation is currently implemented. Email sending and other API features will follow in upcoming phases.

## Installation

```bash
dotnet add package Sendbyte --prerelease
```

## ASP.NET Core Usage

Register the SDK with dependency injection:

```csharp
using Sendbyte.DependencyInjection;

builder.Services.AddSendbyte(options =>
{
    options.ApiKey = builder.Configuration["Sendbyte:ApiKey"]!;
});
```

Inject `ISendbyteClient` into your services:

```csharp
using Sendbyte;

public sealed class NotificationService
{
    private readonly ISendbyteClient _sendbyte;

    public NotificationService(ISendbyteClient sendbyte)
    {
        _sendbyte = sendbyte;
    }
}
```

## Basic Usage

Email sending will be implemented in the next phase.

## Development

Restore, build, and test the solution:

```bash
dotnet restore
dotnet build
dotnet test
```

## License

MIT
