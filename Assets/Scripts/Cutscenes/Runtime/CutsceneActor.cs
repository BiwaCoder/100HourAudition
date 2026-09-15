using UnityEngine;
namespace HundredHour.Cutscenes
{
    public sealed class CutsceneActor : MonoBehaviour
    {
        public string actorId="A";
        public string displayName="Player";
        [Min(0)] public float aimHeight=.3f;
        [Min(.1f)] public float framingRadius=.8f;
        public Vector3 Aim => transform.position+Vector3.up*aimHeight;
    }
}
