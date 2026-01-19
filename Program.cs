// Load environment variables from .env file
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var envFilePath = config["EnvironmentFilePath"];
if (!string.IsNullOrEmpty(envFilePath))
{
    var fullPath = Path.IsPathRooted(envFilePath) 
        ? envFilePath 
        : Path.Combine(Directory.GetCurrentDirectory(), envFilePath);
    
    if (File.Exists(fullPath))
    {
        foreach (var line in File.ReadAllLines(fullPath))
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
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Configure preprocessing options (if exists)
if (builder.Configuration.GetSection("ConversationPreprocessing").Exists())
{
    builder.Services.Configure<PhoneAgent.Configuration.PreprocessingOptions>(
        builder.Configuration.GetSection("ConversationPreprocessing"));

    // Register conversation preprocessor (if exists)
    builder.Services.AddSingleton<PhoneAgent.Interfaces.IConversationPreprocessor,
        PhoneAgent.Services.DefaultConversationPreprocessor>();
}

// Register phone agent services with dependency injection
builder.Services.AddSingleton<PhoneAgent.Interfaces.IPhoneService, PhoneAgent.Services.TestingPhoneService>();
builder.Services.AddSingleton<PhoneAgent.Interfaces.ISpeechToTextService, PhoneAgent.Services.AzureSpeechToTextService>();
builder.Services.AddSingleton<PhoneAgent.Interfaces.ITextToSpeechService, PhoneAgent.Services.AzureTextToSpeechService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
