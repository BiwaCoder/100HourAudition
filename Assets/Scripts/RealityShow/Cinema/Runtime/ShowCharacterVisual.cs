using UnityEngine;
using UnityEngine.InputSystem;

namespace HundredHour.RealityShow.Cinema
{
    // Presentation only: the original collider, Rigidbody and actor remain in charge.
    public sealed class ShowCharacterVisual : MonoBehaviour
    {
        public enum VisualMode { Cube, Silhouette, Wire, SilhouetteAndWire, StreetToon, StorybookToon }
        public VisualMode mode = VisualMode.SilhouetteAndWire;
        public Renderer cube, silhouette, wire;
        public Transform model, torso, head, leftArm, rightArm, leftLeg, rightLeg;
        public UnityEngine.UI.Button switchButton;
        public TMPro.TMP_Text modeLabel;
        public UnityEngine.UI.Graphic[] originalPortraitGraphics;
        public bool animate = true;
        public bool acceptKeyboard = true;
        public ShowCharacterVisual linkedVisual;
        public ShowImportedCharacter[] importedModels;
        public ShowCharacterRig streetToon, storybookToon;
        Vector3 previousPosition;
        float stride, movement;
        VisualMode appliedMode = (VisualMode)(-1);

        void OnEnable()
        {
            previousPosition = transform.position;
            if (switchButton != null) switchButton.onClick.AddListener(CycleMode);
            ApplyMode();
        }
        void OnDisable()
        {
            if (switchButton != null) switchButton.onClick.RemoveListener(CycleMode);
        }
        public void CycleMode() => SetMode((VisualMode)(((int)mode + 1) % ((importedModels != null && importedModels.Length == 3) || (streetToon != null && storybookToon != null) ? 6 : 4)));
        public void SetMode(VisualMode value) { mode = value; ApplyMode(); }
        public void ApplyMode()
        {
            if (cube != null) cube.enabled = mode == VisualMode.Cube;
            if (silhouette != null) silhouette.enabled = mode == VisualMode.Silhouette || mode == VisualMode.SilhouetteAndWire;
            if (wire != null) wire.enabled = mode == VisualMode.Wire || mode == VisualMode.SilhouetteAndWire;
            if (model != null) model.gameObject.SetActive(mode != VisualMode.Cube && (int)mode < 4);
            if (streetToon != null) streetToon.gameObject.SetActive(mode == VisualMode.StreetToon);
            if (storybookToon != null) storybookToon.gameObject.SetActive(mode == VisualMode.StorybookToon);
            if (importedModels != null)
                for (int i = 0; i < importedModels.Length; i++)
                    if (importedModels[i] != null) importedModels[i].Show(mode, i);
            if (linkedVisual != null && linkedVisual.mode != mode) linkedVisual.SetMode(mode);
            if (originalPortraitGraphics != null)
                foreach (var graphic in originalPortraitGraphics)
                    if (graphic != null) graphic.enabled = mode == VisualMode.Cube;
            string[] labels = { "キューブ", "シルエット", "光のワイヤー", "光の面＋線", "ストリート・セル", "冒険アニメ" };
            if (modeLabel != null) modeLabel.text = "スタイル：" + labels[(int)mode] + "  [V]";
            appliedMode = mode;
        }
        void Update()
        {
            if (acceptKeyboard && Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame) CycleMode();
            if (appliedMode != mode) ApplyMode();
        }
        void LateUpdate()
        {
            Vector3 delta = Vector3.ProjectOnPlane(transform.position - previousPosition, Vector3.up);
            previousPosition = transform.position;
            if (importedModels != null && importedModels.Length == 3)
            {
                int index = mode == VisualMode.StreetToon ? 1 : mode == VisualMode.StorybookToon ? 2 : 0;
                if (animate && mode != VisualMode.Cube) importedModels[index].Tick(delta);
                return;
            }
            var rig=mode==VisualMode.StreetToon?streetToon:mode==VisualMode.StorybookToon?storybookToon:null;
            var activeModel=rig!=null?rig.transform:model;
            if (activeModel == null || !activeModel.gameObject.activeSelf || !animate) return;
            float dt = Mathf.Max(Time.deltaTime, .0001f);
            float speed = delta.magnitude / dt;
            // Ignore teleports. Rotate the visual only; physics and camera targets never turn.
            if (speed > .08f && speed < 15)
                activeModel.rotation = Quaternion.Slerp(activeModel.rotation, Quaternion.LookRotation(delta), 1 - Mathf.Exp(-10 * dt));
            movement = Mathf.Lerp(movement, speed < 15 ? Mathf.Clamp01(speed / 3) : 0, 1 - Mathf.Exp(-9 * dt));
            stride += Mathf.Min(delta.magnitude, .2f) * 8;
            float swing = Mathf.Sin(stride) * 24 * movement;
            float breath = Mathf.Sin(Time.time * 1.7f);
            if(rig!=null){rig.Pose(swing,breath);return;}
            torso.localRotation = Quaternion.Euler(breath * .8f, 0, breath * .5f);
            head.localRotation = Quaternion.Euler(breath * .6f, breath * 1.5f, 0);
            leftArm.localRotation = Quaternion.Euler(-swing * .7f, 0, 2 + breath);
            rightArm.localRotation = Quaternion.Euler(swing * .7f, 0, -2 - breath);
            leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
        }
    }
}
