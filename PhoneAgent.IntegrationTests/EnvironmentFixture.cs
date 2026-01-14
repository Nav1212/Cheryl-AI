using Microsoft.Extensions.Configuration;

namespace PhoneAgent.IntegrationTests;

public class EnvironmentFixture
{
    public IConfiguration Configuration { get; }

    public EnvironmentFixture()
    {
        // Load appsettings.json to get environment file path
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "appsettings.json");
        var tempConfig = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: true)
            .Build();
        
        var envFilePath = tempConfig["EnvironmentFilePath"];
        
        if (!string.IsNullOrEmpty(envFilePath) && File.Exists(envFilePath))
        {
            foreach (var line in File.ReadAllLines(envFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }

        Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
    }
}

[CollectionDefinition("AzureServices")]
public class AzureServicesCollection : ICollectionFixture<EnvironmentFixture>
{
}
