# Using Console with Consul

This is a C# code example that interacts with Consul to export, import, and clone configurations.

## Requirements

- .NET Core SDK
- NuGet Package: Consul

## How to Use

1. Clone this repository.
2. Build the project using the command `dotnet build`.
3. Run the program using the command `dotnet run`.

## Features

The program offers the following options:

### Export Configurations

Exports configurations from Consul to a JSON file.

```csharp
Console.WriteLine("Enter the settings prefix to export: ");
var prefixToExport = Console.ReadLine();

Console.WriteLine("Enter the file path to save the settings. ex: c:/temp/my-export.json ");
var exportFilePath = Console.ReadLine();

await ExportConfigurations(consulClient, prefixToExport, exportFilePath);
```

### Import Configurations

Imports configurations from a JSON file to Consul.

```csharp
Console.WriteLine("Enter the file path of the settings to import.  ex: c:/temp/my-export.json");
var importFilePath = Console.ReadLine();

Console.WriteLine("Enter the new prefix for the imported settings: ");
var newPrefixForImport = Console.ReadLine();

await ImportConfigurations(consulClient, importFilePath, newPrefixForImport);
```

### Clone Structure

Clones the configuration structure from one prefix to another.

```csharp
Console.WriteLine("Enter the prefix you want to clone: ");
var prefix = Console.ReadLine();
var keysEValues = await FindKeys(consulClient, prefix);

Console.WriteLine("Enter the NEW prefix where you want to create the clone: ");
var newPrefix = Console.ReadLine();

foreach (var par in keysEValues)
{
    await CreateKey(consulClient, $"{newPrefix}/{par.Key}", par.Value);
}
```

## Notes

- Make sure you have the proper permissions to access Consul.
- Make sure you have a running instance of Consul before using this program.
- Replace the default Consul address and token values as needed in the `CreateConsulClient()` method.

---

Developed by [Meigston](https://github.com/Meigston).
