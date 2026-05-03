# Tiki.Net.Parsers.Email

Email (EML/RFC822) parsing for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Parses RFC822 email messages (.eml files), extracting headers and body text. Uses a lightweight custom parser with **zero external dependencies**. Returns a `TikiMessage` result.

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Email
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();
var result = await tiki.ParseAsync("message.eml");

if (result is TikiMessage msg)
{
    Console.WriteLine($"From: {msg.FromName} <{msg.FromAddress}>");
    Console.WriteLine($"To: {string.Join(", ", msg.ToAddresses ?? [])}");
    Console.WriteLine($"CC: {string.Join(", ", msg.CcAddresses ?? [])}");
    Console.WriteLine($"Subject: {msg.Subject}");
    Console.WriteLine($"Date: {msg.DateSent}");
    Console.WriteLine($"Body: {msg.Content}");
}
```

## Extracted Properties

| Property | Type | Description |
|----------|------|-------------|
| Content | string | Email body (plain text preferred, falls back to stripped HTML) |
| FromAddress | string? | Sender email address |
| FromName | string? | Sender display name |
| ToAddresses | string[]? | Recipient email addresses |
| CcAddresses | string[]? | CC recipient addresses |
| BccAddresses | string[]? | BCC recipient addresses |
| Subject | string? | Email subject line |
| DateSent | DateTime? | Send date/time |
| ConversationId | string? | Message-ID header |

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
