globalThis.breakoutAudio = (() => {
    let ctx = null;
    let masterGain = null;
    const buffers = {};
    let bgSource = null;
    let bgGain = null;
    let pendingBg = null; // {id, volume} — saved when startBg called on suspended ctx
    let lastError = '';
    let backgroundSuspendRequested = false;

    function ensureCtx() {
        if (!ctx) {
            ctx = new AudioContext();
            masterGain = ctx.createGain();
            masterGain.connect(ctx.destination);
        }
    }

    function startBgNow(id, volume) {
        const buf = buffers[id];
        if (!buf) return;
        bgGain = ctx.createGain();
        bgGain.gain.value = volume;
        bgGain.connect(masterGain);
        bgSource = ctx.createBufferSource();
        bgSource.buffer = buf;
        bgSource.loop = true;
        bgSource.connect(bgGain);
        bgSource.start(0);
    }

    function resumeCtx() {
        ensureCtx();
        if (!ctx || ctx.state !== 'suspended') return;
        lastError = '';
        ctx.resume().then(() => {
            if (pendingBg && !bgSource) {
                startBgNow(pendingBg.id, pendingBg.volume);
            }
        }).catch((e) => {
            lastError = e?.message || String(e);
            console.error('[audio] resume failed:', e);
        });
    }

    function suspendForBackground() {
        if (!ctx || ctx.state !== 'running') {
            return;
        }

        backgroundSuspendRequested = true;
        lastError = '';
        ctx.suspend().catch((e) => {
            lastError = e?.message || String(e);
            console.error('[audio] suspend failed:', e);
        });
    }

    function resumeAfterBackground() {
        if (!backgroundSuspendRequested) {
            return;
        }

        backgroundSuspendRequested = false;
        resumeCtx();
    }

    // Resume AudioContext on first user gesture (browser autoplay policy)
    document.addEventListener('pointerdown', resumeCtx, { once: true });
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            suspendForBackground();
        } else {
            resumeAfterBackground();
        }
    });
    globalThis.addEventListener('pagehide', suspendForBackground);
    globalThis.addEventListener('pageshow', resumeAfterBackground);

    return {
        async preload(id, url) {
            try {
                lastError = '';
                ensureCtx();
                const resp = await fetch(url);
                if (!resp.ok) {
                    throw new Error(`HTTP ${resp.status} while loading ${url}`);
                }
                const buf = await resp.arrayBuffer();
                buffers[id] = await ctx.decodeAudioData(buf);
                return true;
            } catch (e) {
                lastError = e?.message || String(e);
                console.error('[audio] preload failed:', id, e);
                return false;
            }
        },

        play(id, volume, balance, loop) {
            const buf = buffers[id];
            if (!buf || !ctx || ctx.state === 'suspended') return;
            const src = ctx.createBufferSource();
            src.buffer = buf;
            src.loop = loop;
            const gainNode = ctx.createGain();
            gainNode.gain.value = volume;
            const panner = ctx.createStereoPanner();
            panner.pan.value = balance;
            src.connect(panner);
            panner.connect(gainNode);
            gainNode.connect(masterGain);
            src.start(0);
        },

        startBg(id, volume) {
            this.stopBg();
            pendingBg = { id, volume };
            ensureCtx();
            if (ctx.state === 'suspended') return;
            startBgNow(id, volume);
        },

        stopBg() {
            pendingBg = null;
            if (bgSource) { try { bgSource.stop(); } catch (e) { } bgSource = null; }
            if (bgGain) { bgGain.disconnect(); bgGain = null; }
        },

        setBgVolume(v) {
            if (pendingBg) pendingBg.volume = v;
            if (bgGain) bgGain.gain.value = v;
        },

        resumeCtx() {
            resumeCtx();
        },

        isBgPlaying() {
            return bgSource !== null;
        },

        getLastError() {
            return lastError;
        },

        getState() {
            return ctx ? ctx.state : 'none';
        }
    };
})();
