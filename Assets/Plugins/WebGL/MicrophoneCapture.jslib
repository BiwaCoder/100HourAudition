// Bridges HundredHour.RealtimeVoice.MicrophoneCaptureWebGL (Assets/Scripts/RealtimeVoice/Runtime/
// MicrophoneCaptureWebGL.cs) to getUserMedia + Web Audio, since UnityEngine.Microphone does not
// exist in WebGL builds. Singleton (this project never runs two mic captures at once).
// ScriptProcessorNode is deprecated in favor of AudioWorkletNode but remains supported in every
// current browser and needs no separate module file to ship with the build -- picked for that
// simplicity under a hackathon deadline; revisit if browsers ever drop it.
var LibraryMicrophoneCapture = {
    $micCaptureState: {
        generation: 0,
        starting: false,
        onStarted: null,
        onChunk: null,
        onError: null,
        context: null,
        stream: null,
        source: null,
        processor: null,
    },

    MicCaptureSetCallbacks: function (onStarted, onChunk, onError) {
        micCaptureState.onStarted = onStarted;
        micCaptureState.onChunk = onChunk;
        micCaptureState.onError = onError;
    },

    MicCaptureStart: function () {
        if (micCaptureState.context || micCaptureState.starting) return; // already capturing

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            MicCaptureEmitError("このブラウザはマイク入力に対応していません。");
            return;
        }

        var generation = ++micCaptureState.generation;
        micCaptureState.starting = true;
        navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
            if (generation !== micCaptureState.generation) {
                stream.getTracks().forEach(function (track) { track.stop(); });
                return;
            }
            micCaptureState.stream = stream;
            var context = HHVoiceAudio.init();
            micCaptureState.context = context;
            // Shared context was armed on the title screen; retry if browser suspended it.
            if (context.state !== 'running') context.resume().catch(function (error) {
                console.warn('Microphone audio resume failed:', error);
            });
            var source = context.createMediaStreamSource(stream);
            // 4096 samples/callback is the largest ScriptProcessorNode buffer size; keeps the
            // JS->WASM call rate low without adding much latency at typical 44.1/48kHz inputs.
            var processor = context.createScriptProcessor(4096, 1, 1);

            processor.onaudioprocess = function (event) {
                event.outputBuffer.getChannelData(0).fill(0); // Never monitor the microphone.
                var input = event.inputBuffer.getChannelData(0);
                var bytes = input.length * 4;
                var ptr = _malloc(bytes);
                HEAPF32.set(input, ptr >> 2);
                try {
                    if (micCaptureState.onChunk) Module.dynCall_vii(micCaptureState.onChunk, ptr, input.length);
                } finally {
                    _free(ptr);
                }
            };

            source.connect(processor);
            // Keep callbacks active; the output buffer is explicitly silent.
            processor.connect(context.destination);

            micCaptureState.context = context;
            micCaptureState.stream = stream;
            micCaptureState.source = source;
            micCaptureState.processor = processor;

            micCaptureState.starting = false;
            if (micCaptureState.onStarted) Module.dynCall_vi(micCaptureState.onStarted, context.sampleRate);
        }).catch(function (err) {
            if (generation !== micCaptureState.generation) return;
            _MicCaptureStop();
            MicCaptureEmitError("マイクの使用が許可されませんでした: " + err);
        });
    },

    MicCaptureStop: function () {
        var state = micCaptureState;
        state.generation++;
        state.starting = false;
        if (state.processor) { state.processor.disconnect(); state.processor.onaudioprocess = null; }
        if (state.source) state.source.disconnect();
        if (state.stream) state.stream.getTracks().forEach(function (track) { track.stop(); });
        // The shared context stays alive for PCM playback and subsequent sessions.
        state.context = state.stream = state.source = state.processor = null;
    },

    MicCaptureIsCapturing: function () {
        return micCaptureState.context ? 1 : 0;
    },

    // Not exported itself ($-prefixed); called as a bare identifier by the functions above.
    $MicCaptureEmitError: function (text) {
        if (!micCaptureState.onError) return;
        var length = lengthBytesUTF8(text) + 1;
        var buffer = _malloc(length);
        stringToUTF8(text, buffer, length);
        try { Module.dynCall_vi(micCaptureState.onError, buffer); }
        finally { _free(buffer); }
    },
};

autoAddDeps(LibraryMicrophoneCapture, '$HHVoiceAudio');
autoAddDeps(LibraryMicrophoneCapture, '$micCaptureState');
autoAddDeps(LibraryMicrophoneCapture, '$MicCaptureEmitError');
mergeInto(LibraryManager.library, LibraryMicrophoneCapture);
