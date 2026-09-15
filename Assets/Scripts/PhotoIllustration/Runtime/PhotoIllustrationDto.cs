using System;

namespace HundredHour.PhotoIllustration
{
    [Serializable]
    public sealed class PhotoIllustrationResponsePayload
    {
        public bool success;
        public string impression;
        public string illustration_base64;
        public string error;
    }
}
