using System;
using UnityEditor;
using UnityEngine;
namespace HundredHour.RealtimeVoice.Editor
{
    public static class RealtimeVoiceTerminalChecks
    {
        static void Check(bool value,string message) { if(!value) throw new Exception(message); }
        public static string Run()
        {
            var buffer=new VoiceSignalBuffer();var bands=new float[64];
            int previous=-1;
            foreach(int hz in new[]{220,880,3200})
            {
                var bytes=Tone(hz,.2f);
                buffer.WritePcm16(bytes);float rms=buffer.ReadBands(bands);
                int peak=0;for(int i=1;i<bands.Length;i++)if(bands[i]>bands[peak])peak=i;
                Check(rms>.1f,"PCM signal energy missing");Check(peak>previous,"Frequency bands not ordered");previous=peak;
            }
            buffer.Clear();Check(buffer.ReadBands(bands)==0,"Silence must have zero energy");
            foreach(float band in bands)Check(band==0,"Stale spectrum after clear");
            var flow=new VoiceInterviewFlow();flow.Begin();Check(flow.Connected(),"Connect");flow.PromptFinished();flow.ReceiveTranscript("こんにちは");flow.Confirm();flow.PromptFinished();flow.ReceiveTranscript("海と音楽");flow.Confirm();flow.PromptFinished();flow.ReceiveTranscript("好奇心");flow.Confirm();
            Check(!flow.BuildInput.Contains("こんにちは"),"Mic test leaked into character input");
            Check(flow.Complete("{\"name\":\"波音\"}"),"Completion failed");
            if(Application.isPlaying)
            {
                var view=UnityEngine.Object.FindFirstObjectByType<RealtimeVoiceDemoView>();
                Check(!view.terminal.session.IsConnected,"Offline checks require disconnected session");
                string clipboard=GUIUtility.systemCopyBuffer;
                try
                {
                    view.Present(flow);Check(view.resultPanel.activeSelf&&view.copyButton.gameObject.activeSelf,"Result controls missing");
                    view.copyButton.onClick.Invoke();Check(GUIUtility.systemCopyBuffer==flow.Result,"Reveal corrupted copied JSON");
                    flow.Reset();view.Present(flow);Check(!view.terminal.ReceivedVoice,"Color history must reset");
                    flow.Begin();flow.Connected();view.Present(flow);Check(view.terminal.SystemColor==VoiceTerminalPresentation.Blue,"Initial system not blue");
                    flow.PromptFinished();flow.ReceiveTranscript("声");view.Present(flow);Check(view.terminal.SystemColor==VoiceTerminalPresentation.Purple,"Received voice did not turn system purple");
                    Check(view.confirmButton.gameObject.activeSelf&&view.retryButton.gameObject.activeSelf,"Review controls missing");
                    flow.Retry();view.Present(flow);Check(view.microphonePanel.activeSelf&&!view.confirmButton.gameObject.activeSelf,"Retry state invalid");
                    flow.Fail("接続を確認してください。");view.Present(flow);Check(view.startButton.gameObject.activeSelf&&!view.microphonePanel.activeSelf,"Error state invalid");
                }
                finally{GUIUtility.systemCopyBuffer=clipboard;view.terminal.session.Disconnect();flow.Reset();view.Present(flow);}
            }
            return "PASS: FFT frequency/energy/silence, interview isolation"+(Application.isPlaying?", JSON copy during reveal, blue/purple/reset, review/retry/error visibility":"");
        }
        public static byte[] Tone(int hz,float seconds)
        {
            int count=(int)(24000*seconds);var bytes=new byte[count*2];
            for(int i=0;i<count;i++){short sample=(short)(Math.Sin(2*Math.PI*hz*i/24000)*12000);bytes[i*2]=(byte)sample;bytes[i*2+1]=(byte)(sample>>8);}return bytes;
        }
    }
}
