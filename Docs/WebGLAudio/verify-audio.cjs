// No audio device or API calls: exercise the browser bridge with a simulated audio clock.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const listeners = {};
const contexts = [];
class Context {
    constructor() { this.state = 'suspended'; this.currentTime = 0; this.sampleRate = 48000; contexts.push(this); }
    resume() { this.state = 'running'; return Promise.resolve(); }
    close() { this.state = 'closed'; return Promise.resolve(); }
    createAnalyser() { return { connect() {}, disconnect() {}, getFloatTimeDomainData(a) { a.fill(0.25); } }; }
    createBuffer(ch, count, rate) { const data = new Float32Array(count); return { duration: count / rate, getChannelData: () => data }; }
    createBufferSource() { return { connect() {}, disconnect() {}, start(t) { this.started = t; }, stop() { this.stopped = true; } }; }
    createMediaStreamSource() { return { connect() {}, disconnect() {} }; }
    createScriptProcessor() { return { connect() {}, disconnect() {} }; }
}
const heap = new ArrayBuffer(8192);
let permission;
const sandbox = { console, Float32Array, HEAPU8: new Uint8Array(heap), HEAPF32: new Float32Array(heap),
    window: { AudioContext: Context, addEventListener(e, fn) { listeners[e] = fn; }, removeEventListener(e) { delete listeners[e]; } },
    navigator: { mediaDevices: { getUserMedia: () => new Promise(resolve => permission = resolve) } },
    Module: { deinitializers: [] }, LibraryManager: { library: {} }, autoAddDeps() {}, mergeInto: Object.assign };
vm.createContext(sandbox);
for (const file of ['VoicePlayback', 'MicrophoneCapture']) vm.runInContext(fs.readFileSync(`Assets/Plugins/WebGL/${file}.jslib`, 'utf8'), sandbox);
for (const [key, value] of Object.entries(sandbox.LibraryManager.library)) sandbox[key.startsWith('$') ? key.slice(1) : key] = value;
sandbox._MicCaptureStop = sandbox.MicCaptureStop;
(async () => {
    sandbox.HHVoiceInit();
    assert.equal(contexts.length, 1);
    assert.equal(sandbox.HHVoiceIsRunning(), 0);
    assert.equal(typeof sandbox.Module.hundredHourUnlockVoiceAudio, 'function');
    const id = sandbox.HHVoiceCreate();
    // PCM16 signed decoding, suspended queue, and successive chunks without overlap.
    sandbox.HEAPU8.set([0, 128, 255, 127]);
    sandbox.HHVoiceEnqueue(id, 0, 4);
    sandbox.HHVoiceEnqueue(id, 0, 4);
    const player = sandbox.HHVoiceAudio.players[id];
    assert.equal(player.sources[0].source.buffer.getChannelData(0)[0], -1);
    assert.equal(player.sources[0].source.buffer.getChannelData(0)[1], 32767 / 32768);
    assert.equal(player.sources[1].start, player.sources[0].end);
    assert.equal(sandbox.HHVoiceQueuedSamples(id), 4);
    sandbox.HHVoiceReadSignal(id, 128);
    assert.equal(sandbox.HEAPF32[32], 0);
    listeners.click();
    assert.equal(contexts[0].state, 'running');
    assert.equal(sandbox.HHVoiceIsRunning(), 1);
    assert.equal(await sandbox.Module.hundredHourUnlockVoiceAudio(), 'running');
    assert.equal(sandbox.Module.hundredHourVoiceAudioDiagnostics().players[0].receivedChunks, 2);
    sandbox.HHVoiceReadSignal(id, 128);
    assert.equal(sandbox.HEAPF32[32], 0.25);
    contexts[0].currentTime = player.nextTime + 1;
    assert.equal(sandbox.HHVoiceQueuedSamples(id), 0);
    const sources = player.sources.map(x => x.source);
    sandbox.HHVoiceClear(id);
    assert(sources.every(x => x.stopped));
    assert.equal(player.sources.length, 0);
    // Permission arriving after cancellation must release the mic, never resurrect it.
    let stopped = false;
    const stream = { getTracks: () => [{ stop() { stopped = true; } }] };
    sandbox.MicCaptureStart();
    sandbox.MicCaptureStop();
    permission(stream);
    await new Promise(setImmediate);
    assert(stopped);
    assert.equal(sandbox.micCaptureState.context, null);
    sandbox.MicCaptureStart();
    permission(stream);
    await new Promise(setImmediate);
    assert.equal(sandbox.micCaptureState.context, contexts[0]);
    assert.equal(contexts.length, 1);
    sandbox.MicCaptureStop();
    assert.equal(contexts[0].state, 'running');
    sandbox.HHVoiceDestroy(id);
    assert.equal(Object.keys(sandbox.HHVoiceAudio.players).length, 0);
    sandbox.Module.deinitializers.forEach(fn => fn());
    assert.equal(Object.keys(listeners).length, 0);
    assert.equal(contexts[0].state, 'closed');
    console.log('PASS: PCM conversion, scheduling, gesture unlock, signal, clear/destroy, mic cancellation, shared context, unload.');
})().catch(error => { console.error(error); process.exitCode = 1; });
