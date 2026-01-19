# Simple Speech Test Interface

## Quick Start

1. **Run the application:**
   ```bash
   dotnet run
   ```

2. **Access the simple test page:**
   - Navigate to: `https://localhost:5001` or `https://localhost:5001/simple`
   - The root URL (`/`) will automatically redirect to `/simple`

3. **Test speech-to-text:**
   - Click the **"Start Recording"** button
   - Speak into your microphone
   - Click **"Stop Recording"** button
   - Your transcribed text will appear in the **"Transcription"** box

4. **Use the input text box:**
   - Below the transcription, there's a text box where you can type any input
   - This text box can be used for testing or entering manual responses

## Features

- **Simple, clean interface** with minimal distractions
- **Real-time microphone recording** using browser's audio API
- **Speech-to-text transcription** using Azure Speech Services (or dummy if not configured)
- **Text input box** for manual text entry
- **Error handling** with clear error messages

## Configuration

### Without Azure Speech (Testing Mode)

The app will work without Azure credentials - it will return placeholder text indicating Azure is not configured.

### With Azure Speech (Production)

1. Get your Azure Speech credentials from [Azure Portal](https://portal.azure.com)
2. Update `appsettings.Development.json`:

```json
{
  "AzureSpeech": {
    "SubscriptionKey": "YOUR_KEY_HERE",
    "Region": "eastus"
  }
}
```

Or set environment variables:
```bash
AZURESPEECH__SUBSCRIPTIONKEY=your_key_here
AZURESPEECH__REGION=eastus
```

## Troubleshooting

### Microphone Access Denied

- **Chrome/Edge:** Click the lock icon in the address bar → Site settings → Microphone → Allow
- **Firefox:** Click the shield/lock icon → Connection secure → More information → Permissions → Microphone → Allow
- **Must use HTTPS:** The browser requires HTTPS for microphone access (http://localhost won't work for production)

### "No speech detected" Error

- Make sure your microphone is working and selected as the default device
- Speak clearly and close to the microphone
- Check your system's microphone volume settings
- Try recording for at least 2-3 seconds before stopping

### Azure Speech Errors

- Verify your subscription key is correct in `appsettings.Development.json`
- Check that your Azure Speech service is in the correct region
- Ensure your Azure subscription has available quota

## Browser Compatibility

- **Chrome/Edge:** Full support
- **Firefox:** Full support
- **Safari:** Requires getUserMedia permissions

## File Structure

```
Pages/
  Simple.cshtml          - Main page route
  Index.cshtml           - Redirects to Simple.cshtml

Components/
  SimpleTest.razor       - Main Blazor component

wwwroot/js/
  audioRecorder.js       - Audio recording JavaScript
```

## Extending the Interface

The simple interface is designed to be minimal. To add features:

1. **Modify** `Components/SimpleTest.razor` - Add UI elements and logic
2. **Update** `wwwroot/js/audioRecorder.js` - Modify audio handling
3. **Inject services** - Add more services to the component for additional functionality

Example - Adding TTS playback:

```csharp
@inject ITextToSpeechService TextToSpeechService

private async Task ConvertToSpeech()
{
    if (!string.IsNullOrEmpty(inputText))
    {
        var audioBytes = await TextToSpeechService.SynthesizeAsync(inputText);
        var base64Audio = Convert.ToBase64String(audioBytes);
        await JSRuntime.InvokeVoidAsync("playAudio", base64Audio);
    }
}
```
