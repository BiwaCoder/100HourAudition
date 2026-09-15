using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
namespace HundredHour.Environments
{
    // Camera bookmarks share NatureStage's output camera and image effects.
    public sealed class MansionSceneTour : MonoBehaviour
    {
        public Camera outputCamera;
        public CinemachineBrain brain;
        public GameObject followCamera;
        public GameObject joystickCanvas;
        public Rigidbody player;
        public Behaviour mover;
        public Transform[] viewpoints;
        public int CurrentView { get; private set; }
        void Start() => SelectView(0);
        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (k.digit1Key.wasPressedThisFrame) SelectView(0);
            if (k.digit2Key.wasPressedThisFrame) SelectView(1);
            if (k.digit3Key.wasPressedThisFrame) SelectView(2);
            if (k.digit4Key.wasPressedThisFrame) SelectView(3);
        }
        public void SelectView(int index)
        {
            if (index < 0 || index > 3) return;
            CurrentView = index;
            bool walk = index == 3;
            mover.enabled = false;
            if (!player.isKinematic) player.linearVelocity = Vector3.zero;
            player.isKinematic = !walk;
            mover.enabled = walk;
            joystickCanvas.SetActive(walk);
            followCamera.SetActive(walk);
            brain.enabled = walk;
            if (!walk && index < viewpoints.Length)
            {
                outputCamera.transform.SetPositionAndRotation(viewpoints[index].position, viewpoints[index].rotation);
                outputCamera.fieldOfView = index == 0 ? 48 : 68;
            }
        }
    }
}
