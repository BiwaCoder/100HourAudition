using System;
namespace HundredHour.RealtimeVoice
{
    public enum VoiceInterviewState { Ready, Connecting, MicPrompt, MicListening, MicReview, Question1Prompt, Question1Listening, Question1Review, Question2Prompt, Question2Listening, Question2Review, Generating, Complete, Error }

    // Pure state machine: microphone test is never part of the two character answers.
    public sealed class VoiceInterviewFlow
    {
        public const string Question1="好きなものや、夢中になっていることを教えてくれ。";
        public const string Question2="君はどんな性格だと思う？ 大切にしていることも教えてくれ。";
        public VoiceInterviewState State {get;private set;}=VoiceInterviewState.Ready;
        public string Candidate {get;private set;}="";
        public string FirstAnswer {get;private set;}="";
        public string SecondAnswer {get;private set;}="";
        public string Error {get;private set;}="";
        public string Result {get;private set;}="";
        public bool Listening=>State==VoiceInterviewState.MicListening||State==VoiceInterviewState.Question1Listening||State==VoiceInterviewState.Question2Listening;
        public bool Reviewing=>State==VoiceInterviewState.MicReview||State==VoiceInterviewState.Question1Review||State==VoiceInterviewState.Question2Review;
        public bool Prompting=>State==VoiceInterviewState.MicPrompt||State==VoiceInterviewState.Question1Prompt||State==VoiceInterviewState.Question2Prompt;
        public int QuestionNumber=>State>=VoiceInterviewState.Question2Prompt&&State<=VoiceInterviewState.Question2Review?2:State>=VoiceInterviewState.Question1Prompt&&State<=VoiceInterviewState.Question1Review?1:0;
        public string Prompt=>State==VoiceInterviewState.MicPrompt?"私はAI-0、エーアイゼロ。生まれたばかりのAIだ。これから、君の新しいキャラクターを作ろう。まずはマイクチェックだ。何か話してくれ。":State==VoiceInterviewState.Question1Prompt?"声はちゃんと届いているよ。ここから二つ質問する。まず一つ目。"+Question1:State==VoiceInterviewState.Question2Prompt?"ありがとう。では二つ目。"+Question2:"";
        public string CurrentQuestion=>QuestionNumber==1?Question1:QuestionNumber==2?Question2:"「こんにちは」など、短く話してみてください。";
        public string BuildInput=>$"質問1: {Question1}\n回答1: {FirstAnswer}\n\n質問2: {Question2}\n回答2: {SecondAnswer}";
        public void Begin(){State=VoiceInterviewState.Connecting;Candidate=FirstAnswer=SecondAnswer=Error=Result="";}
        public bool Connected(){if(State!=VoiceInterviewState.Connecting)return false;State=VoiceInterviewState.MicPrompt;return true;}
        public bool PromptFinished()
        {
            if(!Prompting)return false;
            State=State==VoiceInterviewState.MicPrompt?VoiceInterviewState.MicListening:State==VoiceInterviewState.Question1Prompt?VoiceInterviewState.Question1Listening:VoiceInterviewState.Question2Listening;return true;
        }
        public bool ReceiveTranscript(string text)
        {
            if(!Listening||string.IsNullOrWhiteSpace(text))return false;
            Candidate=text.Trim();State=State==VoiceInterviewState.MicListening?VoiceInterviewState.MicReview:State==VoiceInterviewState.Question1Listening?VoiceInterviewState.Question1Review:VoiceInterviewState.Question2Review;return true;
        }
        public bool Confirm()
        {
            if(!Reviewing||string.IsNullOrWhiteSpace(Candidate))return false;
            if(State==VoiceInterviewState.MicReview)State=VoiceInterviewState.Question1Prompt;
            else if(State==VoiceInterviewState.Question1Review){FirstAnswer=Candidate;State=VoiceInterviewState.Question2Prompt;}
            else {SecondAnswer=Candidate;State=VoiceInterviewState.Generating;}
            Candidate="";return true;
        }
        public bool Retry()
        {
            if(!Reviewing)return false;
            State=State==VoiceInterviewState.MicReview?VoiceInterviewState.MicListening:State==VoiceInterviewState.Question1Review?VoiceInterviewState.Question1Listening:VoiceInterviewState.Question2Listening;Candidate="";return true;
        }
        public void Fail(string message){State=VoiceInterviewState.Error;Error=message;}
        public bool Complete(string json){if(State!=VoiceInterviewState.Generating||string.IsNullOrWhiteSpace(json))return false;Result=json;State=VoiceInterviewState.Complete;return true;}
        public void Reset(){State=VoiceInterviewState.Ready;Candidate=FirstAnswer=SecondAnswer=Result=Error="";}
    }
}
