using System;

namespace HundredHour.AIChat
{
    [Serializable]
    public sealed class ChatRequestPayload
    {
        public string message;
        public string system_message;
    }

    [Serializable]
    public sealed class ChatResponsePayload
    {
        public bool success;
        public string response;
        public string error;
    }
}
