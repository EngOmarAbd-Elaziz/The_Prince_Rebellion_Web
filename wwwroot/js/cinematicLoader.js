// Shared cinematic loading screen markup & progress updates
(function () {
    var RING_R = 52;
    var CIRCUMFERENCE = 2 * Math.PI * RING_R;

    function emberFieldHtml() {
        var html = '<div class="loader-ember-field" aria-hidden="true">';
        for (var i = 0; i < 10; i++) {
            html += '<span class="loader-ember" style="--i:' + i + '"></span>';
        }
        html += '</div>';
        return html;
    }

    function ringBlockHtml(idPrefix, initialMsg) {
        var p = idPrefix || '';
        return (
            '<div class="loader-scanlines" aria-hidden="true"></div>' +
            emberFieldHtml() +
            '<div class="loader-artwork-bg"></div>' +
            '<div class="loader-fog-layer"></div>' +
            '<div class="loader-light-beam"></div>' +
            '<div class="loader-vignette-overlay"></div>' +
            '<div class="loader-left-content">' +
            '<div class="loader-logo-group">' +
            '<div class="logo-text-group">' +
            '<h1 class="loader-game-title">THE PRINCE\'S REBELLION</h1>' +
            '<div class="loader-game-subtitle">DARK FANTASY TURN-BASED RPG</div>' +
            '</div></div>' +
            '<div class="loader-status-heading">L O A D I N G<span class="loader-ellipsis" aria-hidden="true"></span></div>' +
            '<div class="circular-sword-container loader-ring-hero">' +
            '<svg class="loader-progress-svg" viewBox="0 0 120 120" aria-hidden="true">' +
            '<defs>' +
            '<linearGradient id="loaderRingGrad' + p + '" x1="0%" y1="0%" x2="100%" y2="100%">' +
            '<stop offset="0%" stop-color="#0284C7"/>' +
            '<stop offset="50%" stop-color="#38BDF8"/>' +
            '<stop offset="100%" stop-color="#FACC15"/>' +
            '</linearGradient>' +
            '</defs>' +
            '<circle class="loader-ring-track" cx="60" cy="60" r="' + RING_R + '"/>' +
            '<circle class="loader-ring-progress" id="' + p + 'ring-progress" cx="60" cy="60" r="' + RING_R + '" ' +
            'stroke="url(#loaderRingGrad' + p + ')" style="stroke-dasharray:' + CIRCUMFERENCE + ';stroke-dashoffset:' + CIRCUMFERENCE + '"/>' +
            '</svg>' +
            '<div class="outer-rune-ring loader-rune-ring--outer"></div>' +
            '<div class="inner-rune-ring loader-rune-ring--inner"></div>' +
            '<div class="loader-ring-glow"></div>' +
            '<span class="loader-orbit-dot loader-orbit-dot--a"></span>' +
            '<span class="loader-orbit-dot loader-orbit-dot--b"></span>' +
            '<span id="' + p + 'pct" class="circular-pct-text">0%</span>' +
            '</div>' +
            '<div class="loader-progress-section">' +
            '<div class="flourish-bar-wrapper">' +
            '<div class="energy-bar-track">' +
            '<div id="' + p + 'fill" class="energy-bar-fill" style="width:0%;">' +
            '<div class="energy-spark-head"></div></div></div></div>' +
            '<span id="' + p + 'msg" class="loader-msg-text">' + (initialMsg || 'Initializing Engine...') + '</span>' +
            '</div></div>' +
            '<div class="loader-tip-box-wrapper">' +
            '<div class="tip-content-text">' +
            '<div class="tip-label">TIP</div>' +
            '<div id="' + p + 'tip" class="tip-body-text">' +
            'Every choice shapes your destiny. Some paths are dangerous, some are unforgiving.' +
            '</div></div></div>' +
            '<div class="loader-quote-center">"In darkness, even a single choice can ignite a revolution."</div>'
        );
    }

    function setProgress(pct, prefix) {
        var p = prefix || '';
        pct = Math.max(0, Math.min(100, pct));
        var pctEl = document.getElementById(p + 'pct');
        var fillEl = document.getElementById(p + 'fill');
        var ringEl = document.getElementById(p + 'ring-progress');
        if (pctEl) {
            pctEl.textContent = pct + '%';
            pctEl.classList.remove('loader-pct-bump');
            void pctEl.offsetWidth;
            pctEl.classList.add('loader-pct-bump');
        }
        if (fillEl) fillEl.style.width = pct + '%';
        if (ringEl) {
            ringEl.style.strokeDashoffset = String(CIRCUMFERENCE * (1 - pct / 100));
        }
    }

    window.cinematicLoader = {
        buildMarkup: ringBlockHtml,
        setProgress: setProgress,
        circumference: CIRCUMFERENCE
    };
})();
