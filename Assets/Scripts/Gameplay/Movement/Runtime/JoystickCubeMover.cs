using UnityEngine;
using UnityEngine.InputSystem;
namespace HundredHour.JoystickDemo
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class JoystickCubeMover : MonoBehaviour
    {
        [SerializeField] FixedJoystick joystick;
        [SerializeField] Camera referenceCamera;
        [SerializeField, Min(0)] float speed = 6;
        [SerializeField, Min(0)] float jumpHeight = 1.5f;
        Rigidbody body;
        bool grounded;
        bool jumpRequested;
        public bool IsGrounded => grounded;

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpRequested = true;
        }

        void OnCollisionStay(Collision collision)
        {
            // Walls and steep surfaces cannot provide another jump.
            if (body.linearVelocity.y > .1f) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= .6f)
                {
                    grounded = true;
                    break;
                }
            }
        }

        void OnCollisionEnter(Collision collision) => OnCollisionStay(collision);
        void Awake() => body = GetComponent<Rigidbody>();
        void FixedUpdate()
        {
            Vector3 direction = Vector3.zero;
            if (joystick != null && joystick.isActiveAndEnabled && referenceCamera != null)
            {
                var forward = Vector3.ProjectOnPlane(referenceCamera.transform.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(referenceCamera.transform.right, Vector3.up).normalized;
                direction = Vector3.ClampMagnitude(forward * joystick.Vertical + right * joystick.Horizontal, 1);
            }
            float verticalSpeed = body.linearVelocity.y;
            if (jumpRequested && grounded)
                verticalSpeed = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            body.linearVelocity = new Vector3(direction.x * speed, verticalSpeed, direction.z * speed);
            jumpRequested = false;
            // Physics contacts repopulate this after each simulation step.
            grounded = false;
        }
        void OnDisable()
        {
            jumpRequested = false;
            grounded = false;
            if(body != null) body.linearVelocity = new Vector3(0,body.linearVelocity.y,0);
        }
    }
}
