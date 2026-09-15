using System;
using UnityEngine;

namespace HundredHour.UI.Participants
{
    [Serializable]
    public sealed class ParticipantCardData
    {
        [Tooltip("人物写真。Texture Import SettingsでSprite (2D and UI)に設定してから指定。空ならシルエット。")]
        public Sprite portrait;
        public string displayName = "New participant";
        public string subtitle = "Age  ·  Occupation  ·  City";
        [Min(0)] public int number = 1;
        public long stars = 342000;
        [Range(0, 1)] public float support = .28f;
        public string status = "FRONTRUNNER";
        public Color accent = new Color(1f, .53f, .57f);
        public bool eliminated;
    }
}
