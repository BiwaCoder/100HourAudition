var preview = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var host = new UnityEngine.GameObject("ChoiceFrameCheck", typeof(UnityEngine.RectTransform));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(host, preview);
try
{
    var rt = (UnityEngine.RectTransform)host.transform;
    rt.sizeDelta = new UnityEngine.Vector2(600, 46);
    rt.anchoredPosition = new UnityEngine.Vector2(12, 34);
    rt.localScale = new UnityEngine.Vector3(1.2f, 1.3f, 1);
    var frame = host.AddComponent<HundredHour.UI.Choices.ChoiceSelectionFrame>();
    var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
    var type = frame.GetType();
    type.GetMethod("OnEnable", flags).Invoke(frame, null);
    var child = host.transform.GetChild(0);
    if (child.childCount != 12) throw new System.Exception("Expected 12 frame pieces");
    foreach (var image in host.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        if (image.raycastTarget) throw new System.Exception("Frame blocks pointer input");
    frame.SetSelected(false);
    if (child.gameObject.activeSelf) throw new System.Exception("Selection hide failed");
    frame.SetSelected(true);
    type.GetMethod("Update", flags).Invoke(frame, null);
    if (child.localScale.x < .98f || child.localScale.x > 1.02f) throw new System.Exception("Pulse out of range");
    if (rt.anchoredPosition != new UnityEngine.Vector2(12, 34) || rt.localScale != new UnityEngine.Vector3(1.2f, 1.3f, 1)) throw new System.Exception("Host was modified");
    type.GetMethod("OnDisable", flags).Invoke(frame, null);
    if (child.gameObject.activeSelf) throw new System.Exception("Disable leaves frame visible");
    type.GetMethod("OnEnable", flags).Invoke(frame, null);
    if (host.transform.childCount != 1 || !child.gameObject.activeSelf) throw new System.Exception("Reenable duplicated/lost frame");
    type.GetMethod("OnDestroy", flags).Invoke(frame, null);
    if (host.transform.childCount != 0) throw new System.Exception("Detached frame leaked");
    return "PASS: 12-piece geometry, no raycast blocking, selection visibility, pulse bounds, untouched host transform, disable/reenable, removal cleanup";
}
finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview); }
