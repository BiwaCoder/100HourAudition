"""Make a temporary, local-only diagnostic page beside the release. No microphone capture."""
from pathlib import Path
root = Path('Builds/WebGLRelease')
script = '''<script>
const audioProbe={analysers:[],sockets:[],maxRms:0};
const NativeContext=window.AudioContext;
window.AudioContext=class extends NativeContext {
 createAnalyser(){const analyser=super.createAnalyser();audioProbe.analysers.push(analyser);return analyser;}
};
const NativeSocket=window.WebSocket;
window.WebSocket=class extends NativeSocket {
 constructor(...args){super(...args);let state={host:new URL(args[0]).host,path:new URL(args[0]).pathname,messages:{},close:null};audioProbe.sockets.push(state);this.addEventListener('message',ev=>{try{const type=JSON.parse(ev.data).type;state.messages[type]=(state.messages[type]||0)+1;}catch{}});this.addEventListener('close',ev=>state.close=ev.code);}
};
window.addEventListener('DOMContentLoaded',()=>{const output=document.createElement('pre');output.id='audio-probe';document.body.appendChild(output);setInterval(()=>{
let rms=0;audioProbe.analysers.forEach(a=>{let data=new Float32Array(a.fftSize);a.getFloatTimeDomainData(data);let sum=0;for(let n of data)sum+=n*n;rms=Math.max(rms,Math.sqrt(sum/data.length));});audioProbe.maxRms=Math.max(audioProbe.maxRms,rms);
output.textContent=JSON.stringify({voice:window.hundredHourUnityInstance?.Module?.hundredHourVoiceAudioDiagnostics?.(),sockets:audioProbe.sockets,rms:rms,maxRms:audioProbe.maxRms});
},250);});
</script>'''
(root / 'audio-probe.html').write_text((root / 'index.html').read_text().replace('<head>', '<head>'+script))
