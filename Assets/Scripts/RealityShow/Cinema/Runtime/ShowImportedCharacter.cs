using UnityEngine;

namespace HundredHour.RealityShow.Cinema
{
    public sealed class ShowImportedCharacter : MonoBehaviour
    {
        public Renderer body, contours;
        public Material painted, ink;
        public Animation motion;
        public AnimationClip idleAnimation, walkAnimation;
        public string idleClip, walkClip;
        bool walking;

        void Awake()
        {
            if (motion == null) return;
            if (idleAnimation != null) motion.AddClip(idleAnimation, idleClip);
            if (walkAnimation != null) motion.AddClip(walkAnimation, walkClip);
        }

        public void Show(ShowCharacterVisual.VisualMode mode, int variant)
        {
            int m = (int)mode;
            bool visible = variant == 0 ? m > 0 && m < 4 : m == variant + 3;
            gameObject.SetActive(visible);
            if (!visible) return;
            body.enabled = m != 2;
            body.sharedMaterial = m == 1 ? ink : painted;
            if (contours != null) contours.enabled = m == 2 || m == 3;
            walking = false;
            if (Application.isPlaying && motion != null) motion.Play(idleClip);
        }

        public void Tick(Vector3 delta)
        {
            float dt = Mathf.Max(Time.deltaTime, .0001f);
            float speed = delta.magnitude / dt;
            bool moving = speed > .08f && speed < 15;
            if (moving)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 1 - Mathf.Exp(-10 * dt));
            if (motion == null) return;
            if (moving != walking)
            {
                motion.CrossFade(moving ? walkClip : idleClip, .18f);
                walking = moving;
            }
            if (moving) motion[walkClip].speed = Mathf.Clamp(speed / 2, .6f, 2.5f);
        }
    }
}
