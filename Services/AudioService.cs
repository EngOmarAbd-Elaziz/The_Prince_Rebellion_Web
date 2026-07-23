using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AdventureGameWeb.Services
{
    public class AudioService
    {
        private readonly IJSRuntime js;

        public AudioService(IJSRuntime js)
        {
            this.js = js;
        }

        public async ValueTask PlayClickAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playClick"); } catch { }
        }

        public async ValueTask PlayHoverAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playHover"); } catch { }
        }

        public async ValueTask PlayModalOpenAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playModalOpen"); } catch { }
        }

        public async ValueTask PlayModalCloseAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playModalClose"); } catch { }
        }

        public async ValueTask PlayTypingTickAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playTypingTick"); } catch { }
        }

        public async ValueTask PlayCoinAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playCoin"); } catch { }
        }

        public async ValueTask PlaySwordSlashAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playSwordSlash"); } catch { }
        }

        public async ValueTask PlayGunshotAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playGunshot"); } catch { }
        }

        public async ValueTask PlayShieldBlockAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playShieldBlock"); } catch { }
        }

        public async ValueTask PlayTreasureOpenAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playTreasureOpen"); } catch { }
        }

        public async ValueTask PlayVictoryFanfareAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playVictoryFanfare"); } catch { }
        }

        public async ValueTask PlayDefeatSoundAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playDefeatSound"); } catch { }
        }

        public async ValueTask PlayAchievementUnlockAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.playAchievementUnlock"); } catch { }
        }

        public async ValueTask PlayTrackAsync(string trackName)
        {
            try { await js.InvokeVoidAsync("audioSynth.playTrack", trackName); } catch { }
        }

        public async ValueTask StopTrackAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.stopTrack"); } catch { }
        }

        public async ValueTask PlayAmbientLoopAsync(string ambientType)
        {
            try { await js.InvokeVoidAsync("audioSynth.playAmbientLoop", ambientType); } catch { }
        }

        public async ValueTask StopAmbientLoopAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.stopAmbientLoop"); } catch { }
        }

        public async ValueTask StartDuckingAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.startDucking"); } catch { }
        }

        public async ValueTask StopDuckingAsync()
        {
            try { await js.InvokeVoidAsync("audioSynth.stopDucking"); } catch { }
        }

        // Multi-Channel Volume Controls
        public async ValueTask SetMasterVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setMasterVolume", val); } catch { }
        }

        public async ValueTask SetMusicVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setMusicVolume", val); } catch { }
        }

        public async ValueTask SetSfxVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setSfxVolume", val); } catch { }
        }

        public async ValueTask SetAmbientVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setAmbientVolume", val); } catch { }
        }

        public async ValueTask SetUiVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setUiVolume", val); } catch { }
        }

        public async ValueTask SetVoiceVolumeAsync(double val)
        {
            try { await js.InvokeVoidAsync("audioSynth.setVoiceVolume", val); } catch { }
        }

        public async ValueTask SetAllVolumesAsync(double master, double music, double sfx, double ambient, double ui, double voice)
        {
            try { await js.InvokeVoidAsync("audioSynth.setAllVolumes", master, music, sfx, ambient, ui, voice); } catch { }
        }

        public async ValueTask ToggleMuteAsync(bool isMuted)
        {
            try { await js.InvokeVoidAsync("audioSynth.toggleMute", isMuted); } catch { }
        }
    }
}
