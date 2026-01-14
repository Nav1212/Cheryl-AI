# PhoneAgent Integration Tests

This project contains integration tests for Azure services used in the PhoneAgent application.

## Test Coverage

### Azure Speech Services Tests (`AzureSpeechIntegrationTests`)
1. **TextToSpeech_ShouldSynthesizeSpeech** - Tests Azure Text-to-Speech (TTS) service
2. **SpeechToText_ShouldRecognizeSpeechFromAudioFile** - Tests Azure Speech-to-Text (STT) service  
3. **GetAvailableVoices_ShouldReturnVoiceList** - Tests retrieving available TTS voices

### Azure OpenAI Tests (`AzureOpenAIIntegrationTests`)
1. **ChatCompletion_ShouldGenerateResponse** - Tests basic chat completion
2. **ChatCompletion_WithConversation_ShouldMaintainContext** - Tests conversation context retention
3. **StreamingChatCompletion_ShouldStreamResponse** - Tests streaming chat responses

## Prerequisites

Make sure your `.env` file (located at `C:\Users\NSarshar\Desktop\ENV`) contains:

```env
AZURE_SPEECH_SUBSCRIPTION_KEY=your_key_here
AZURE_SPEECH_REGION=your_region_here
AZURE_OPENAI_ENDPOINT=your_endpoint_here
AZURE_OPENAI_API_KEY=your_key_here
AZURE_OPENAI_DEPLOYMENT_NAME=your_deployment_name_here
```

## Running Tests

### Run all tests:
```powershell
cd PhoneAgent.IntegrationTests
dotnet test
```

### Run specific test class:
```powershell
dotnet test --filter "FullyQualifiedName~AzureSpeechIntegrationTests"
dotnet test --filter "FullyQualifiedName~AzureOpenAIIntegrationTests"
```

### Run specific test:
```powershell
dotnet test --filter "FullyQualifiedName~TextToSpeech_ShouldSynthesizeSpeech"
```

### Run with detailed output:
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Notes

- These are **integration tests** that make real API calls to Azure services
- Tests will consume Azure credits/quota
- Tests require valid Azure credentials in the environment file
- The STT test synthesizes audio first, then recognizes it to validate the full pipeline
