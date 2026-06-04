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
var parser = new DissectParser("%{timestamp:DateTime[Europe/Berlin]} [%{log_level}] %{message}");

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

## Extensions

The following extensions to the Dissect grammar are supported.

### Typed values

The original Dissect grammar only supports string values. This extended implementation can return strongly typed values by adding a typed value modifier to a key.

The typed value modifier is introduced with a colon (`:`). An optional parameter can be specified inside brackets (`[` and `]`) immediately following the typed value. The modifier can appear anywhere after the key name.

The only restriction is that the right-padding modifier (`->`) must always be the final modifier.

The following patterns are all valid:

```
%{keyname:DATATYPE}
%{keyname/n:DATATYPE}
%{keyname:DATATYPE/n}
%{keyname/n:DATATYPE->}
%{keyname:DATATYPE/n->}
```

The following data types are currently supported:

| Type     | Parameter                                                                          | Remarks                                                                       |
| -------- | ---------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| string   | <None>                                                                             | Default type.                                                                 |
| int      | <None>                                                                             |                                                                               |
| float    | Culture name supported by the CultureInfo class.                                   | Useful for specifying whether decimal values use a period or comma separator. |
| double   | Culture name supported by the CultureInfo class.                                   | Useful for specifying whether decimal values use a period or comma separator. |
| decimal  | Culture name supported by the CultureInfo class.                                   | Useful for specifying whether decimal values use a period or comma separator. |
| datetime | An [IANA time zone](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones). | Values are returned as UTC `DateTime` instances.                              |
| time     | An [IANA time zone](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones). | Interpreted relative to the current day and returned as a UTC `DateTime`.     |

All parameters are optional. Type names are case-insensitive.

The `datetime` and `time` typed values also support the special time zone value `local`, which refers to the system's local time zone.

When a typed value is used together with the append modifier, all partial definitions of the same key should use the same data type:

```
var parser = new DissectParser("%{value:DateTime} hello world %{+value:DateTime}", separator: " ");
var result = parser.Parse("2026-05-01 hello world 19:26:12");
```

Typed values can also be used with reference keys. In this case, the type should be defined on the value reference (`&`) part of the pair:

```csharp
var parser = new DissectParser("%{*a} %{&a:DateTime}");
var result = parser.Parse("StartOfYear 2026-01-01");

Console.WriteLine(resultDict["StartOfYear"]);
```

## Append Modifier Separator

When using append modifiers such as `%{+key} %{+key}`, values can be joined using a separator.

A separator can be specified globally when creating the parser:

```csharp
var parser = new DissectParser(pattern, separator: "*");
```

Alternatively, it can be defined directly within the pattern by placing it in brackets immediately after the append modifier (`+`).

Different separators can be used within the same pattern. A separator defined on an append modifier remains active for subsequent append operations until another separator is specified.

### Examples

Given the input:

```
this is a test
```

and a default separator of `*`, the following results are produced:

| Pattern                     | Result           | Remarks                                                                              |
| --------------------------- | ---------------- | ------------------------------------------------------------------------------------ |
| %{+[,]a} %{+a}              | this,is a test   |                                                                                      |
| %{+a} %{+a}                 | this*is a test   | The default separator is used because no explicit separator is defined.              |
| %{+[...]a} %{+a}            | this...is a test |                                                                                      |
| %{+[]a} %{+a}               | thisis a test    |                                                                                      |
| %{+[+]a} %{+[-]a} %{a} %{a} | this+is-a-test   | The separator changes from `+` to `-` between the second and third append operation. |

## Source code

This library is published under the MIT-License (see license file for details). The source code can be found at [github](https://github.com/OSgAgA/Dissect.Extended.Net).