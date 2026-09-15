using UnityEngine;
namespace HundredHour.RealityShow.Cinema
{
    public sealed class ShowCharacterRig : MonoBehaviour
    {
        public Transform torso, head, leftArm, rightArm, leftLeg, rightLeg;
        public void Pose(float swing, float breath)
        {
            torso.localRotation=Quaternion.Euler(breath*.8f,0,breath*.5f);
            head.localRotation=Quaternion.Euler(breath*.6f,breath*1.5f,0);
            leftArm.localRotation=Quaternion.Euler(-swing*.7f,0,2+breath);
            rightArm.localRotation=Quaternion.Euler(swing*.7f,0,-2-breath);
            leftLeg.localRotation=Quaternion.Euler(swing,0,0);
            rightLeg.localRotation=Quaternion.Euler(-swing,0,0);
        }
    }
}
