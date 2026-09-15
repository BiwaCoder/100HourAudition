using System;
using System.Collections.Generic;
using UnityEngine;
namespace HundredHour.Cutscenes
{
    public enum ShotKind { Single, TwoShot, OverShoulder, ReverseShoulder, RearWide, GroupWide, Orbit, DollyIn, DollyOut }
    public enum ShotTransition { Cut, Blend, Fade }
    [Serializable] public sealed class CameraShot
    {
        public string label="New shot";
        public ShotKind kind;
        public string actorA="A", actorB="B";
        [Min(.2f)] public float duration=4;
        public ShotTransition transition=ShotTransition.Blend;
        [Range(0,3)] public float transitionSeconds=.6f;
        [Range(1,150)] public float distance=5;
        [Range(-180,180)] public float yaw;
        [Range(.1f,60)] public float elevation=1.2f;
        [Range(15,90)] public float startFov=40, endFov=40;
        [Range(-180,180)] public float orbitDegrees=60;
        [Tooltip("Camera-space lateral travel, positive moves from right to left.")]
        [Range(-20,20)] public float lateralTravel;
    }
    [Serializable] public sealed class CameraPlan { public string title; public CameraShot[] shots; }
    [CreateAssetMenu(menuName="100Hour/Cutscene Sequence")]
    public sealed class CutsceneSequence : ScriptableObject
    {
        public string title="Nature conversation";
        [TextArea(3,8)] public string instruction;
        [Range(0,3)] public float fadeIn= .7f, fadeOut=.7f;
        public List<CameraShot> shots=new List<CameraShot>();
    }
    public static class CameraPlanValidation
    {
        public static void Validate(IList<CameraShot> shots, ICollection<string> actors)
        {
            if(shots==null || shots.Count==0 || shots.Count>64)throw new ArgumentException("ショットは1〜64件必要です。");
            float total=0;
            foreach(var s in shots)
            {
                if(s==null || !Enum.IsDefined(typeof(ShotKind),s.kind) || !Enum.IsDefined(typeof(ShotTransition),s.transition))throw new ArgumentException("未対応のショット種類です。");
                if(!actors.Contains(s.actorA))throw new ArgumentException("登場人物IDが見つかりません: "+s.actorA);
                if((s.kind==ShotKind.TwoShot || s.kind==ShotKind.OverShoulder || s.kind==ShotKind.ReverseShoulder) && (s.actorA==s.actorB || !actors.Contains(s.actorB)))throw new ArgumentException("2人の異なる人物IDが必要です。");
                Range(s.lateralTravel,-20,20,"lateralTravel");Range(s.duration,.2f,60,"duration");Range(s.transitionSeconds,0,3,"transitionSeconds");Range(s.distance,1,150,"distance");Range(s.elevation,.1f,60,"elevation");Range(s.yaw,-180,180,"yaw");Range(s.orbitDegrees,-180,180,"orbitDegrees");Range(s.startFov,15,90,"startFov");Range(s.endFov,15,90,"endFov");total+=s.duration+s.transitionSeconds*2;
            }
            if(total>600)throw new ArgumentException("演出は10分以内にしてください。");
        }
        static void Range(float v,float min,float max,string name){if(float.IsNaN(v)||float.IsInfinity(v)||v<min||v>max)throw new ArgumentException(name+" の値が範囲外です。");}
    }
}
