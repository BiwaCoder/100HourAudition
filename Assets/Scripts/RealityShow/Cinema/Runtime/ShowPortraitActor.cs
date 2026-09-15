using UnityEngine;
namespace HundredHour.RealityShow.Cinema
{
    // Replace the portrait child with an animated character without changing actor IDs or shots.
    public sealed class ShowPortraitActor : MonoBehaviour
    {
        public Camera audienceCamera;
        void LateUpdate(){if(audienceCamera!=null){var direction=transform.position-audienceCamera.transform.position;direction.y=0;if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction);}}
    }
}
