using System;

namespace HundredHour.RealtimeVoice
{
    [Serializable]
    public sealed class BuildCharacterRequestPayload
    {
        public string voice_answer;
        public string photo_impression;
    }

    [Serializable]
    public sealed class BuildCharacterResponsePayload
    {
        public bool success;
        public string character_json; // レスポンスのcharacterオブジェクトをそのままJSON文字列として保持する
        public string error;
    }
}
