using Consul;
using System.Text;
using System.Text.Json;

//ConsulClient consulClient = CreateConsulClient();
while (true)
{
    var consulClient = CreateConsulClient();
    await MenuOption(consulClient);
}


static async Task MenuOption(ConsulClient consulClient)
{
    Console.WriteLine($"Choose an option:{Environment.NewLine} 1) Export {Environment.NewLine} 2) Import {Environment.NewLine} 3) Clone {Environment.NewLine} 4) Clone and Migrate key to new prefix");
    var opcao = Console.ReadLine();

    switch (opcao)
    {
        case "1":
            Console.WriteLine("Enter the settings prefix to export: ");
            var prefixToExport = Console.ReadLine();

            Console.WriteLine("Enter the file path to save the settings. ex: c:/temp/my-export.json ");
            var exportFilePath = Console.ReadLine();

            await ExportConfigurations(consulClient, prefixToExport, exportFilePath);

            Console.Clear();
            Console.WriteLine($"Settings exported successfully. {Environment.NewLine}");
            break;
        case "2":
            Console.WriteLine("Enter the file path of the settings to import.  ex: c:/temp/my-export.json");
            var importFilePath = Console.ReadLine();

            Console.WriteLine("Enter the new prefix for the imported settings: ");
            var newPrefixForImport = Console.ReadLine();

            await ImportConfigurations(consulClient, importFilePath, newPrefixForImport);

            Console.Clear();
            Console.WriteLine($"Settings imported successfully. {Environment.NewLine}");
            break;
        case "3":
            await CloneStructure(consulClient);

            Console.Clear();
            Console.WriteLine($"Settings cloned successfully. {Environment.NewLine}");
            break;
        case "4":
            await CloneStructure(consulClient, true);

            Console.Clear();
            Console.WriteLine($"Settings cloned and migrated successfully. {Environment.NewLine}");
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}


static ConsulClient CreateConsulClient()
{
    return new ConsulClient(c =>
    {
        Console.WriteLine("Put your Consul URL");
        c.Address = new Uri(Console.ReadLine() ?? "http://localhost:8500");

        Console.WriteLine("Put your Consul TOKEN");
        c.Token = Console.ReadLine();
    });
}

static async Task CloneStructure(ConsulClient consulClient, bool migrate = false)
{
    Console.WriteLine("Enter the prefix you want to clone: ");
    var prefix = Console.ReadLine();
    var keysEValues = await FindKeys(consulClient, prefix);

    Console.WriteLine("Enter the NEW prefix where you want to create the clone: ");
    var newPrefix = Console.ReadLine();

    foreach (var par in keysEValues)
    {
        if (migrate)
        {
            var updatedValue = par.Value.Replace($"{{{{ key '{prefix}", $"{{{{ key '{newPrefix}");
            Console.WriteLine($"Key: {par.Key}, Value: {updatedValue}");
            await CreateKey(consulClient, $"{newPrefix}/{par.Key}", updatedValue);
        }
        else
        {
            Console.WriteLine($"Key: {par.Key}, Value: {par.Value}");
            await CreateKey(consulClient, $"{newPrefix}/{par.Key}", par.Value);
        }
    }
}

static async Task ExportConfigurations(ConsulClient consulClient, string prefix, string filePath)
{
    var keysEValues = await FindKeys(consulClient, prefix);
    var consulFormatList = new List<Dictionary<string, string>>();

    foreach (var pair in keysEValues)
    {
        consulFormatList.Add(new Dictionary<string, string>
        {
            { "Key", pair.Key },
            { "Value", Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value)) }
        });
    }

    var json = JsonSerializer.Serialize(consulFormatList, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(filePath, json);
}


static async Task ImportConfigurations(ConsulClient consulClient, string filePath, string newPrefix)
{
    var json = await File.ReadAllTextAsync(filePath);
    var consulFormatList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);

    if (consulFormatList != null)
    {
        foreach (var item in consulFormatList)
        {
            var key = item.ContainsKey("Key") ? item["Key"] : null;
            var value = item.ContainsKey("Value") ? item["Value"] : null;

            if (key != null && value != null)
            {
                Console.WriteLine($"Key: {key}, Value: {value}");
                await CreateKey(consulClient, $"{newPrefix}/{key}", Encoding.UTF8.GetString(Convert.FromBase64String(value)));
            }
        }
    }
}


static async Task<Dictionary<string, string>> FindKeys(ConsulClient client, string prefix)
{
    var keysEValues = new Dictionary<string, string>();

    var queryResult = await client.KV.List(prefix);
    if (queryResult.Response != null)
    {
        foreach (var kv in queryResult.Response)
        {
            var key = kv.Key.Substring(prefix.Length).TrimStart('/');
            if (kv.Value != null)
            {
                var value = Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length);
                keysEValues[key] = value;
            }
        }
    }

    return keysEValues;
}

static async Task CreateKey(ConsulClient client, string key, string valor)
{
    var kvPair = new KVPair(key)
    {
        Value = Encoding.UTF8.GetBytes(valor)
    };

    await client.KV.Put(kvPair);
}