// AAA Professional Web Audio Synthesizer & Multi-Channel Audio Engine for The Prince's Rebellion

class AudioSynthEngine {
    constructor() {
        this.ctx = null;
        
        // Multi-Channel Gain Nodes
        this.masterGain = null;
        this.musicGainA = null;
        this.musicGainB = null;
        this.activeMusicChannel = 'A'; // 'A' or 'B'
        this.duckingGain = null;
        this.sfxGain = null;
        this.ambientGain = null;
        this.uiGain = null;
        this.voiceGain = null;

        // Independent Volume Levels (0.0 to 1.0)
        this.volumes = {
            master: 1.0,
            music: 0.6,
            sfx: 0.8,
            ambient: 0.5,
            ui: 0.7,
            voice: 0.9
        };

        this.isMuted = false;
        this.currentTrackName = '';
        this.currentMusicOscs = { A: [], B: [] };
        this.currentAmbientOscs = [];
        this.initialized = false;

        // Auto-resume on first user gesture
        const resumeAudio = () => {
            this.ensureContext();
            window.removeEventListener('click', resumeAudio);
            window.removeEventListener('keydown', resumeAudio);
            window.removeEventListener('pointerdown', resumeAudio);
        };
        window.addEventListener('click', resumeAudio);
        window.addEventListener('keydown', resumeAudio);
        window.addEventListener('pointerdown', resumeAudio);
    }

    init() {
        if (this.initialized) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            this.ctx = new AudioContext();
            
            // Master & Channel Gains
            this.masterGain = this.ctx.createGain();
            this.duckingGain = this.ctx.createGain();
            this.musicGainA = this.ctx.createGain();
            this.musicGainB = this.ctx.createGain();
            this.sfxGain = this.ctx.createGain();
            this.ambientGain = this.ctx.createGain();
            this.uiGain = this.ctx.createGain();
            this.voiceGain = this.ctx.createGain();

            // Set Initial Gains
            this.masterGain.gain.value = this.volumes.master;
            this.duckingGain.gain.value = 1.0;
            this.musicGainA.gain.value = this.volumes.music;
            this.musicGainB.gain.value = 0.0;
            this.sfxGain.gain.value = this.volumes.sfx;
            this.ambientGain.gain.value = this.volumes.ambient;
            this.uiGain.gain.value = this.volumes.ui;
            this.voiceGain.gain.value = this.volumes.voice;

            // Route Music through Ducking Gain into Master
            this.musicGainA.connect(this.duckingGain);
            this.musicGainB.connect(this.duckingGain);
            this.duckingGain.connect(this.masterGain);

            // Route SFX, Ambient, UI, Voice into Master
            this.sfxGain.connect(this.masterGain);
            this.ambientGain.connect(this.masterGain);
            this.uiGain.connect(this.masterGain);
            this.voiceGain.connect(this.masterGain);

            // Connect Master to Destination
            this.masterGain.connect(this.ctx.destination);

            this.initialized = true;
        } catch (e) {
            console.warn("Web Audio API init:", e);
        }
    }

    ensureContext() {
        try {
            if (!this.initialized) this.init();
            if (this.ctx && this.ctx.state === 'suspended') {
                this.ctx.resume().catch(() => {});
            }
        } catch (e) { }
    }

    // --- Independent Channel Volume Setters ---

    setMasterVolume(vol) {
        try {
            this.volumes.master = Math.max(0, Math.min(1, vol));
            if (this.masterGain) this.masterGain.gain.value = this.isMuted ? 0 : this.volumes.master;
            if (this.bgAudioPlayer) this.bgAudioPlayer.volume = this.isMuted ? 0 : (this.volumes.master * this.volumes.music);
        } catch (e) { }
    }

    setMusicVolume(vol) {
        try {
            this.volumes.music = Math.max(0, Math.min(1, vol));
            const activeGain = this.activeMusicChannel === 'A' ? this.musicGainA : this.musicGainB;
            if (activeGain) activeGain.gain.value = this.isMuted ? 0 : this.volumes.music;
            if (this.bgAudioPlayer) this.bgAudioPlayer.volume = this.isMuted ? 0 : (this.volumes.master * this.volumes.music);
        } catch (e) { }
    }

    setSfxVolume(vol) {
        try {
            this.volumes.sfx = Math.max(0, Math.min(1, vol));
            if (this.sfxGain) this.sfxGain.gain.value = this.isMuted ? 0 : this.volumes.sfx;
        } catch (e) { }
    }

    setAmbientVolume(vol) {
        try {
            this.volumes.ambient = Math.max(0, Math.min(1, vol));
            if (this.ambientGain) this.ambientGain.gain.value = this.isMuted ? 0 : this.volumes.ambient;
        } catch (e) { }
    }

    setUiVolume(vol) {
        try {
            this.volumes.ui = Math.max(0, Math.min(1, vol));
            if (this.uiGain) this.uiGain.gain.value = this.isMuted ? 0 : this.volumes.ui;
        } catch (e) { }
    }

    setVoiceVolume(vol) {
        try {
            this.volumes.voice = Math.max(0, Math.min(1, vol));
            if (this.voiceGain) this.voiceGain.gain.value = this.isMuted ? 0 : this.volumes.voice;
        } catch (e) { }
    }

    setAllVolumes(master, music, sfx, ambient, ui, voice) {
        this.setMasterVolume(master);
        this.setMusicVolume(music);
        this.setSfxVolume(sfx);
        this.setAmbientVolume(ambient);
        this.setUiVolume(ui);
        this.setVoiceVolume(voice);
    }

    toggleMute(muted) {
        try {
            this.isMuted = muted;
            if (this.masterGain) this.masterGain.gain.value = this.isMuted ? 0 : this.volumes.master;
            if (this.bgAudioPlayer) this.bgAudioPlayer.volume = this.isMuted ? 0 : (this.volumes.master * this.volumes.music);
        } catch (e) { }
    }

    // --- Audio Ducking (Music lowers 35% during dialogue text typing) ---
    startDucking() {
        try {
            if (!this.ctx || !this.duckingGain) return;
            const now = this.ctx.currentTime;
            this.duckingGain.gain.linearRampToValueAtTime(0.65, now + 0.3);
        } catch (e) { }
    }

    stopDucking() {
        try {
            if (!this.ctx || !this.duckingGain) return;
            const now = this.ctx.currentTime;
            this.duckingGain.gain.linearRampToValueAtTime(1.0, now + 0.8);
        } catch (e) { }
    }

    // --- Real Orchestral Dark Fantasy Music Tracks + Fallback Synthesizer Engine ---
    playTrack(trackName) {
        try {
            this.ensureContext();
            if (this.currentTrackName === trackName) return; // Already playing

            this.currentTrackName = trackName;

            // Local Downloaded Royalty-Free Tracks
            const audioTrackUrls = {
                'Home': 'audio/track1.mp3',
                'Loading': 'audio/track1.mp3',
                'Story': 'audio/track2.mp3',
                'Menu': 'audio/track1.mp3',
                'Exploration': 'audio/track2.mp3',
                'Battle': 'audio/track3.mp3',
                'Boss': 'audio/track3.mp3',
                'KingBattle': 'audio/track3.mp3',
                'Victory': 'audio/track1.mp3',
                'Ending': 'audio/track2.mp3',
                'Defeat': 'audio/track3.mp3'
            };

            const url = audioTrackUrls[trackName];

            // Initialize or stop existing HTML5 Audio player smoothly
            if (!this.bgAudioPlayer) {
                this.bgAudioPlayer = new Audio();
                this.bgAudioPlayer.loop = true;
                this.bgAudioPlayer.crossOrigin = "anonymous";
            }

            if (url) {
                try {
                    // Smooth transition
                    this.bgAudioPlayer.pause();
                    this.bgAudioPlayer.src = url;
                    this.bgAudioPlayer.volume = this.isMuted ? 0 : (this.volumes.master * this.volumes.music);
                    const playPromise = this.bgAudioPlayer.play();
                    if (playPromise !== undefined) {
                        playPromise.catch(err => {
                            console.warn("Real audio play deferred until user gesture or fallback synth engaged.");
                            this.playSynthFallbackTrack(trackName);
                        });
                    }
                    return;
                } catch (e) {
                    console.warn("Fallback to synth engine:", e);
                }
            }

            this.playSynthFallbackTrack(trackName);
        } catch (e) { }
    }

    playSynthFallbackTrack(trackName) {
        try {
            if (!this.ctx || this.ctx.state !== 'running') return;
            const nextChannel = this.activeMusicChannel === 'A' ? 'B' : 'A';
            const currentGain = this.activeMusicChannel === 'A' ? this.musicGainA : this.musicGainB;
            const nextGain = nextChannel === 'A' ? this.musicGainA : this.musicGainB;

            const now = this.ctx.currentTime;

            if (currentGain) {
                currentGain.gain.linearRampToValueAtTime(0.0001, now + 2.5);
            }

            const oldOscs = this.currentMusicOscs[this.activeMusicChannel];
            setTimeout(() => {
                oldOscs.forEach(({ osc, lfo }) => {
                    try { osc.stop(); lfo.stop(); } catch (e) { }
                });
                this.currentMusicOscs[this.activeMusicChannel] = [];
            }, 2600);

            let freqs = [110, 164.81, 220];
            let oscType = 'sine';

            if (trackName === 'Loading' || trackName === 'Home') {
                freqs = [73.42, 110.00, 146.83, 164.81];
                oscType = 'triangle';
            } else if (trackName === 'Story' || trackName === 'Menu') {
                freqs = [65.41, 98.00, 130.81, 196.00];
                oscType = 'triangle';
            } else if (trackName === 'Exploration') {
                freqs = [82.41, 123.47, 164.81, 196.00];
                oscType = 'sine';
            } else if (trackName === 'Battle') {
                freqs = [55.00, 82.41, 110.00, 130.81];
                oscType = 'sawtooth';
            } else if (trackName === 'Boss' || trackName === 'KingBattle') {
                freqs = [43.65, 65.41, 87.31, 130.81];
                oscType = 'sawtooth';
            } else if (trackName === 'Victory' || trackName === 'Ending') {
                freqs = [220.00, 277.18, 329.63, 440.00];
                oscType = 'triangle';
            } else if (trackName === 'Defeat') {
                freqs = [55.00, 65.41, 82.41];
                oscType = 'sawtooth';
            }

            this.currentMusicOscs[nextChannel] = [];
            if (nextGain) {
                nextGain.gain.setValueAtTime(0.0001, now);
                nextGain.gain.linearRampToValueAtTime(this.isMuted ? 0 : this.volumes.music, now + 2.5);
            }

            freqs.forEach((f) => {
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();

                osc.type = oscType;
                osc.frequency.setValueAtTime(f, now);

                const lfo = this.ctx.createOscillator();
                const lfoGain = this.ctx.createGain();
                lfo.frequency.value = 0.15;
                lfoGain.gain.value = 0.04;
                lfo.connect(lfoGain.gain);

                gain.gain.setValueAtTime(0.04, now);

                osc.connect(gain);
                gain.connect(nextGain);

                osc.start(now);
                lfo.start(now);

                this.currentMusicOscs[nextChannel].push({ osc, lfo, gain });
            });

            this.activeMusicChannel = nextChannel;
        } catch (e) { }
    }

    stopTrack() {
        try {
            if (!this.ctx) return;
            const now = this.ctx.currentTime;
            [this.musicGainA, this.musicGainB].forEach(g => {
                if (g) g.gain.linearRampToValueAtTime(0.0001, now + 1.5);
            });
            this.currentTrackName = '';
        } catch (e) { }
    }

    // --- Environmental Ambient Sound Generator ---
    playAmbientLoop(ambientType) {
        try {
            this.ensureContext();
            if (!this.ctx || this.ctx.state !== 'running') return;

            this.stopAmbientLoop();

            const now = this.ctx.currentTime;
            const bufferSize = this.ctx.sampleRate * 2.0;
            const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
            const data = buffer.getChannelData(0);
            for (let i = 0; i < bufferSize; i++) {
                data[i] = Math.random() * 2 - 1;
            }

            const noise = this.ctx.createBufferSource();
            noise.buffer = buffer;
            noise.loop = true;

            const filter = this.ctx.createBiquadFilter();
            filter.type = ambientType === 'Ocean' || ambientType === 'Rain' ? 'lowpass' : 'bandpass';
            filter.frequency.setValueAtTime(400, now);

            const gain = this.ctx.createGain();
            gain.gain.setValueAtTime(0.01, now);
            gain.gain.linearRampToValueAtTime(0.08, now + 3.0);

            noise.connect(filter);
            filter.connect(gain);
            gain.connect(this.ambientGain);

            noise.start(now);
            this.currentAmbientOscs = [{ noise, filter, gain }];
        } catch (e) { }
    }

    stopAmbientLoop() {
        try {
            if (!this.ctx || !this.currentAmbientOscs.length) return;
            const now = this.ctx.currentTime;
            this.currentAmbientOscs.forEach(({ noise, gain }) => {
                try {
                    gain.gain.linearRampToValueAtTime(0.0001, now + 1.5);
                    setTimeout(() => { try { noise.stop(); } catch (e) { } }, 1600);
                } catch (e) { }
            });
            this.currentAmbientOscs = [];
        } catch (e) { }
    }

    // --- UI & Game Sound Effects ---

    playClick() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(800, this.ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(400, this.ctx.currentTime + 0.05);

            gain.gain.setValueAtTime(0.15, this.ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, this.ctx.currentTime + 0.05);

            osc.connect(gain);
            gain.connect(this.uiGain);

            osc.start();
            osc.stop(this.ctx.currentTime + 0.05);
        } catch (e) { }
    }

    playHover() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(300, this.ctx.currentTime);
            osc.frequency.linearRampToValueAtTime(450, this.ctx.currentTime + 0.04);

            gain.gain.setValueAtTime(0.05, this.ctx.currentTime);
            gain.gain.linearRampToValueAtTime(0.01, this.ctx.currentTime + 0.04);

            osc.connect(gain);
            gain.connect(this.uiGain);

            osc.start();
            osc.stop(this.ctx.currentTime + 0.04);
        } catch (e) { }
    }

    playModalOpen() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(350, now);
            osc.frequency.exponentialRampToValueAtTime(700, now + 0.12);

            gain.gain.setValueAtTime(0.12, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.12);

            osc.connect(gain);
            gain.connect(this.uiGain);

            osc.start(now);
            osc.stop(now + 0.12);
        } catch (e) { }
    }

    playModalClose() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(650, now);
            osc.frequency.exponentialRampToValueAtTime(300, now + 0.1);

            gain.gain.setValueAtTime(0.12, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.1);

            osc.connect(gain);
            gain.connect(this.uiGain);

            osc.start(now);
            osc.stop(now + 0.1);
        } catch (e) { }
    }

    playTypingTick() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'triangle';
            osc.frequency.setValueAtTime(600 + Math.random() * 200, this.ctx.currentTime);

            gain.gain.setValueAtTime(0.03, this.ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.03);

            osc.connect(gain);
            gain.connect(this.voiceGain);

            osc.start();
            osc.stop(this.ctx.currentTime + 0.03);
        } catch (e) { }
    }

    playCoin() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const osc1 = this.ctx.createOscillator();
            const osc2 = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc1.type = 'sine';
            osc2.type = 'sine';
            osc1.frequency.setValueAtTime(987.77, now);
            osc2.frequency.setValueAtTime(1318.51, now + 0.08);

            gain.gain.setValueAtTime(0.2, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);

            osc1.connect(gain);
            osc2.connect(gain);
            gain.connect(this.sfxGain);

            osc1.start(now);
            osc1.stop(now + 0.08);
            osc2.start(now + 0.08);
            osc2.stop(now + 0.35);
        } catch (e) { }
    }

    playSwordSlash() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const bufferSize = this.ctx.sampleRate * 0.25;
            const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
            const data = buffer.getChannelData(0);
            for (let i = 0; i < bufferSize; i++) {
                data[i] = Math.random() * 2 - 1;
            }

            const noise = this.ctx.createBufferSource();
            noise.buffer = buffer;

            const filter = this.ctx.createBiquadFilter();
            filter.type = 'bandpass';
            filter.frequency.setValueAtTime(1200, now);
            filter.frequency.exponentialRampToValueAtTime(300, now + 0.25);
            filter.Q.value = 3;

            const gain = this.ctx.createGain();
            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.25);

            noise.connect(filter);
            filter.connect(gain);
            gain.connect(this.sfxGain);

            noise.start(now);
            noise.stop(now + 0.25);
        } catch (e) { }
    }

    playGunshot() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const bufferSize = this.ctx.sampleRate * 0.3;
            const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
            const data = buffer.getChannelData(0);
            for (let i = 0; i < bufferSize; i++) {
                data[i] = Math.random() * 2 - 1;
            }

            const noise = this.ctx.createBufferSource();
            noise.buffer = buffer;

            const filter = this.ctx.createBiquadFilter();
            filter.type = 'lowpass';
            filter.frequency.setValueAtTime(3000, now);
            filter.frequency.exponentialRampToValueAtTime(100, now + 0.3);

            const gain = this.ctx.createGain();
            gain.gain.setValueAtTime(0.5, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.3);

            noise.connect(filter);
            filter.connect(gain);
            gain.connect(this.sfxGain);

            noise.start(now);
            noise.stop(now + 0.3);
        } catch (e) { }
    }

    playShieldBlock() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'square';
            osc.frequency.setValueAtTime(150, now);
            osc.frequency.exponentialRampToValueAtTime(60, now + 0.2);

            gain.gain.setValueAtTime(0.25, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.2);

            osc.connect(gain);
            gain.connect(this.sfxGain);

            osc.start(now);
            osc.stop(now + 0.2);
        } catch (e) { }
    }

    playTreasureOpen() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const notes = [440, 554.37, 659.25, 880];
            notes.forEach((freq, idx) => {
                const now = this.ctx.currentTime + idx * 0.08;
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();
                osc.type = 'sine';
                osc.frequency.setValueAtTime(freq, now);

                gain.gain.setValueAtTime(0.15, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);

                osc.connect(gain);
                gain.connect(this.sfxGain);

                osc.start(now);
                osc.stop(now + 0.3);
            });
        } catch (e) { }
    }

    playVictoryFanfare() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const notes = [523.25, 659.25, 783.99, 1046.50];
            notes.forEach((freq, idx) => {
                const now = this.ctx.currentTime + idx * 0.15;
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(freq, now);

                gain.gain.setValueAtTime(0.25, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + (idx === 3 ? 1.0 : 0.4));

                osc.connect(gain);
                gain.connect(this.sfxGain);

                osc.start(now);
                osc.stop(now + (idx === 3 ? 1.0 : 0.4));
            });
        } catch (e) { }
    }

    playDefeatSound() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(200, now);
            osc.frequency.exponentialRampToValueAtTime(50, now + 0.8);

            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.8);

            osc.connect(gain);
            gain.connect(this.sfxGain);

            osc.start(now);
            osc.stop(now + 0.8);
        } catch (e) { }
    }

    playAchievementUnlock() {
        try {
            this.ensureContext();
            if (!this.ctx || this.isMuted || this.ctx.state !== 'running') return;

            const now = this.ctx.currentTime;
            const notes = [587.33, 739.99, 880, 1174.66];
            notes.forEach((freq, idx) => {
                const t = now + idx * 0.07;
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();
                osc.type = 'sine';
                osc.frequency.setValueAtTime(freq, t);

                gain.gain.setValueAtTime(0.18, t);
                gain.gain.exponentialRampToValueAtTime(0.001, t + 0.25);

                osc.connect(gain);
                gain.connect(this.sfxGain);

                osc.start(t);
                osc.stop(t + 0.25);
            });
        } catch (e) { }
    }
}

window.audioSynth = new AudioSynthEngine();
