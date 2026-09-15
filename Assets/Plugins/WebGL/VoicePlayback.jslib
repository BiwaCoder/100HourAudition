// Shared by PCM playback and microphone capture. Independent of Unity's private WEBAudio.
var LibraryVoicePlayback = {
    $HHVoiceAudio: {
        context: null,
        players: {},
        nextId: 1,
        init: function () {
            if (HHVoiceAudio.context) return HHVoiceAudio.context;
            var Context = window.AudioContext || window.webkitAudioContext;
            var context = HHVoiceAudio.context = new Context();
            var unlock = function () {
                if (context.state !== 'running' && context.state !== 'closed') {
                    return context.resume().then(function () { return context.state; }).catch(function (error) { console.warn('Voice audio resume failed:', error); return context.state; });
                }
                return Promise.resolve(context.state);
            };
            var events = ['pointerdown', 'touchend', 'keydown', 'click'];
            events.forEach(function (event) { window.addEventListener(event, unlock, true); });
            // Explicit diagnostics, without exposing Unity's private implementation.
            Module['hundredHourVoiceAudioState'] = function () { return context.state; };
            Module['hundredHourUnlockVoiceAudio'] = unlock;
            Module['hundredHourVoiceAudioDiagnostics'] = function () {
                return { state: context.state, time: context.currentTime, players: Object.keys(HHVoiceAudio.players).map(function (id) {
                    var p = HHVoiceAudio.players[id];
                    return { id: id, receivedChunks: p.receivedChunks, queuedSources: p.sources.length };
                }) };
            };
            Module.deinitializers.push(function () {
                events.forEach(function (event) { window.removeEventListener(event, unlock, true); });
                if (context.state !== 'closed') context.close().catch(function () {});
                HHVoiceAudio.context = null;
                HHVoiceAudio.players = {};
            });
            return context;
        },
        clear: function (player) {
            player.sources.forEach(function (entry) {
                entry.source.onended = null;
                entry.source.stop();
                entry.source.disconnect();
            });
            player.sources = [];
            player.nextTime = 0;
        }
    },
    HHVoiceIsRunning: function () { return HHVoiceAudio.context && HHVoiceAudio.context.state === 'running' ? 1 : 0; },
    HHVoiceUnlock: function () { HHVoiceAudio.init(); if (Module['hundredHourUnlockVoiceAudio']) Module['hundredHourUnlockVoiceAudio'](); },
    HHVoiceInit: function () { HHVoiceAudio.init(); },
    HHVoiceCreate: function () {
        var context = HHVoiceAudio.init();
        var analyser = context.createAnalyser();
        analyser.fftSize = 1024;
        analyser.connect(context.destination);
        var id = HHVoiceAudio.nextId++;
        HHVoiceAudio.players[id] = { analyser: analyser, signal: new Float32Array(1024), sources: [], receivedChunks: 0, nextTime: 0 };
        return id;
    },
    HHVoiceEnqueue: function (id, pointer, byteLength) {
        var player = HHVoiceAudio.players[id];
        if (!player || byteLength < 2) return;
        player.receivedChunks++;
        var context = HHVoiceAudio.context;
        var count = byteLength >> 1;
        var buffer = context.createBuffer(1, count, 24000);
        var samples = buffer.getChannelData(0);
        for (var i = 0; i < count; i++) {
            var value = HEAPU8[pointer + i * 2] | (HEAPU8[pointer + i * 2 + 1] << 8);
            samples[i] = (value >= 32768 ? value - 65536 : value) / 32768;
        }
        var source = context.createBufferSource();
        source.buffer = buffer;
        source.connect(player.analyser);
        var start = Math.max(context.currentTime + 0.025, player.nextTime);
        var entry = { source: source, start: start, end: start + buffer.duration };
        source.onended = function () {
            source.disconnect();
            var index = player.sources.indexOf(entry);
            if (index >= 0) player.sources.splice(index, 1);
        };
        player.sources.push(entry);
        player.nextTime = entry.end;
        source.start(start);
    },
    HHVoiceQueuedSamples: function (id) {
        var player = HHVoiceAudio.players[id];
        if (!player) return 0;
        var now = HHVoiceAudio.context.currentTime;
        var duration = 0;
        player.sources.forEach(function (entry) { duration += Math.max(0, entry.end - Math.max(now, entry.start)); });
        return Math.ceil(Math.max(0, duration * 24000 - 0.000001));
    },
    HHVoiceReadSignal: function (id, pointer) {
        var player = HHVoiceAudio.players[id];
        if (!player) return 24000;
        if (HHVoiceAudio.context.state === 'running' && player.sources.length) player.analyser.getFloatTimeDomainData(player.signal);
        else player.signal.fill(0);
        HEAPF32.set(player.signal, pointer >> 2);
        return HHVoiceAudio.context.sampleRate;
    },
    HHVoiceClear: function (id) {
        var player = HHVoiceAudio.players[id];
        if (player) HHVoiceAudio.clear(player);
    },
    HHVoiceDestroy: function (id) {
        var player = HHVoiceAudio.players[id];
        if (!player) return;
        HHVoiceAudio.clear(player);
        player.analyser.disconnect();
        delete HHVoiceAudio.players[id];
    }
};
autoAddDeps(LibraryVoicePlayback, '$HHVoiceAudio');
mergeInto(LibraryManager.library, LibraryVoicePlayback);
