# Dissect.Extended.Net

An implementation of the Dissect parser with additional extensions.

## Introduction

A Dissect parser extracts structured data from a string by matching it against a predefined pattern. The parser compares the input string with the pattern and splits the input according to the Dissect rules.

For details about the original Dissect specification, see the official specification [here](https://github.com/elastic/dissect-specification/blob/master/README.md).

This .NET implementation is fully compatible with the original specification while adding support for typedvalues beyond strings. It also provides a way to define append separators directly within the pattern.

## Quickstart

Install the package from NuGet in your project.

Let's look at a simple example. Assume you have a log message like this:

```
2026-05-31 19:20:41 [Info] Successfully connected to server.
```

You can parse the message using the following Dissect pattern:

```
%{timestamp:DateTime[Europe/Berlin]} [%{log_level}] %{message}
```

This pattern parses the timestamp and interprets it using the IANA time zone `Europe/Berlin`.

The corresponding C# code:

```csharp
var parser = new Parser("%{timestamp:DateTime[Europe/Berlin]} [%{log_level}] %{message}");

if (!parser.IsValid) throw new Exception("invalid parse expression");

var result = parser.Parse("2026-05-31 19:20:41 [Info] Successfully connected to server.");

if (result.Success)
{
	foreach (var keyValue in result.ToDictionary())
	{
		Console.WriteLine($"key: {key}, value: {value}");
	}
}
```

This produces the following output:

```
key: timestamp, value: 31/05/2026 17:20:41
key: log_level, value: Info
key: message, value: Successfully connected to server.
```

Note that all timestamps are converted to UTC before being returned.

## Additional information

Additional information can be found at [github](https://github.com/OSgAgA/Dissect.Extended.Net).