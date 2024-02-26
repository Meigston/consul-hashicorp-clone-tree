using Consul;

var consulClient = new ConsulClient(c =>
{
    c.Address = new Uri("http://proddocker01.qual.local:8500");
    // Substitua "seu-token-aqui" pelo seu token de acesso real
    c.Token = "FBAF54CC-E03D-4763-9F19-376114D3857B";
});

// Substitua "nomeInicial" pelo prefix desejado
Console.WriteLine("Digite o prefixo que deseja clonar: ");
var prefix = Console.ReadLine();
var keysEValues = await FindKeys(consulClient, prefix);

Console.WriteLine("Digite o NOVO prefixo onde deseja criar o clone: ");
var newPrefix = Console.ReadLine();

foreach (var par in keysEValues)
{
    Console.WriteLine($"Chave: {par.Key}, Valor: {par.Value}");
    // Substitua "novoPrefixo" pelo prefix onde você deseja criar o clone das chaves
    await CreateKey(consulClient, $"{newPrefix}/{par.Key}", par.Value);
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
                var value = System.Text.Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length);
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
        Value = System.Text.Encoding.UTF8.GetBytes(valor)
    };

    await client.KV.Put(kvPair);
}