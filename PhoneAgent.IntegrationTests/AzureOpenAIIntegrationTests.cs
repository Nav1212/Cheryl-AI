using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using Xunit;
using Xunit.Abstractions;

namespace PhoneAgent.IntegrationTests;

[Collection("AzureOpenAI")]
public class AzureOpenAIIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _configuration;

    public AzureOpenAIIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        
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

        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
    }

    [Fact]
    public async Task ChatCompletion_ShouldGenerateResponse()
    {
        // Arrange
        var endpoint =new Uri("https://mh-dev-whisper.openai.azure.com/");
        var apiKey = _configuration["AZURE_OPENAI_API_KEY"];
        var deploymentName = "gpt-5.2";

        Assert.False(string.IsNullOrEmpty(apiKey), "AZURE_OPENAI_API_KEY is not set");
        Assert.False(string.IsNullOrEmpty(deploymentName), "AZURE_OPENAI_DEPLOYMENT_NAME is not set");

        AzureOpenAIClient client = new( endpoint, new AzureKeyCredential(apiKey));

        ChatClient chatClient = client.GetChatClient(deploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant."),
            new UserChatMessage("Hello! Can you tell me what 2 + 2 equals?")
        };

        // Act
        _output.WriteLine("Sending chat completion request...");
        _output.WriteLine($"User: {messages[1].Content[0].Text}");
        
        var response = await chatClient.CompleteChatAsync(messages);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        Assert.True(response.Value.Content.Count > 0, "Response should have content");
        
        var responseText = response.Value.Content[0].Text;
        Assert.False(string.IsNullOrEmpty(responseText), "Response text should not be empty");
        
        _output.WriteLine($"✓ Azure OpenAI responded: {responseText}");
        _output.WriteLine($"  Model: {response.Value.Model}");
        _output.WriteLine($"  Finish Reason: {response.Value.FinishReason}");
    }

    [Fact]
    public async Task ChatCompletion_WithConversation_ShouldMaintainContext()
    {
        // Arrange
        var endpoint = new Uri("https://mh-dev-whisper.openai.azure.com/");
        var apiKey = _configuration["AZURE_OPENAI_API_KEY"];
        var deploymentName = "gpt-5.2";

        Assert.False(string.IsNullOrEmpty(apiKey), "AZURE_OPENAI_API_KEY is not set");

        var client = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        var chatClient = client.GetChatClient(deploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant."),
            new UserChatMessage("My favorite color is blue.")
        };

        // Act - First message
        _output.WriteLine("First message: 'My favorite color is blue.'");
        var response1 = await chatClient.CompleteChatAsync(messages);
        messages.Add(new AssistantChatMessage(response1.Value.Content[0].Text));
        _output.WriteLine($"Assistant: {response1.Value.Content[0].Text}");

        // Act - Second message (test context retention)
        messages.Add(new UserChatMessage("What is my favorite color?"));
        _output.WriteLine("Second message: 'What is my favorite color?'");
        var response2 = await chatClient.CompleteChatAsync(messages);

        // Assert
        var finalResponse = response2.Value.Content[0].Text.ToLower();
        _output.WriteLine($"Assistant: {response2.Value.Content[0].Text}");
        
        Assert.Contains("blue", finalResponse);
        _output.WriteLine("✓ Azure OpenAI maintained conversation context correctly");
    }

    [Fact]
    public void StreamingChatCompletion_ShouldStreamResponse()
    {
        // Arrange
        var endpoint = new Uri("https://mh-dev-whisper.openai.azure.com/");
        var apiKey = _configuration["AZURE_OPENAI_API_KEY"];
        var deploymentName = "gpt-5.2";

        Assert.False(string.IsNullOrEmpty(apiKey), "AZURE_OPENAI_API_KEY is not set");

        var client = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
        var chatClient = client.GetChatClient(deploymentName);

        // Act
        _output.WriteLine("Starting streaming chat completion...");
        var fullResponse = "";
        
        CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(
            [
                new SystemChatMessage("You are a helpful AI assistant."),
                new UserChatMessage("Count from 1 to 5.")
            ]);

        foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
        {
            foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
            {
                fullResponse += contentPart.Text;
            }
        }

        // Assert
        Assert.False(string.IsNullOrEmpty(fullResponse), "Streamed response should not be empty");
        _output.WriteLine($"✓ Streamed response: {fullResponse}");
    }
}
