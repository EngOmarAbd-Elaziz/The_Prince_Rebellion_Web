using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AdventureGameWeb.Services
{
    public class AudioSettingsModel
    {
        public double Master { get; set; } = 1.0;
        public double Music { get; set; } = 0.6;
        public double Sfx { get; set; } = 0.8;
        public double Ambient { get; set; } = 0.5;
        public double Ui { get; set; } = 0.7;
        public double Voice { get; set; } = 0.9;
    }

    public class LocalStorageService
    {
        private readonly IJSRuntime js;
        private const string AUDIO_SETTINGS_KEY = "prince_rebellion_audio_settings";
        private const string PLAYER_NAME_KEY = "pr_player_name";

        public LocalStorageService(IJSRuntime js)
        {
            this.js = js;
        }

        public async ValueTask SetItemAsync(string key, string value)
        {
            try { await js.InvokeVoidAsync("storageHelper.setItem", key, value); } catch { }
        }

        public async ValueTask<string?> GetItemAsync(string key)
        {
            try { return await js.InvokeAsync<string?>("storageHelper.getItem", key); } catch { return null; }
        }

        public async ValueTask RemoveItemAsync(string key)
        {
            try { await js.InvokeVoidAsync("storageHelper.removeItem", key); } catch { }
        }

        public async ValueTask SavePlayerNameAsync(string name)
        {
            try { await SetItemAsync(PLAYER_NAME_KEY, name); } catch { }
        }

        public async ValueTask<string?> LoadPlayerNameAsync()
        {
            try { return await GetItemAsync(PLAYER_NAME_KEY); } catch { return null; }
        }

        public async ValueTask ToggleFullscreenAsync()
        {
            try { await js.InvokeVoidAsync("storageHelper.toggleFullscreen"); } catch { }
        }

        public async ValueTask SaveAudioSettingsAsync(AudioSettingsModel settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings);
                await SetItemAsync(AUDIO_SETTINGS_KEY, json);
            }
            catch { }
        }

        public async ValueTask<AudioSettingsModel> LoadAudioSettingsAsync()
        {
            try
            {
                var json = await GetItemAsync(AUDIO_SETTINGS_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonSerializer.Deserialize<AudioSettingsModel>(json);
                    if (loaded != null) return loaded;
                }
            }
            catch { }
            return new AudioSettingsModel();
        }
    }
}
