using System;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace HundredHour.Cutscenes.Editor
{
    public sealed class CutsceneWorkbench : EditorWindow
    {
        [SerializeField] CutsceneSequence selected;
        [SerializeField] string instruction="Aの肩越しにBを4秒、横に二人を5秒でゆっくりズームイン、最後に全体を背後から引いて暗転";
        [SerializeField] string endpoint="http://127.0.0.1:8001";
        CameraPlan draft;
        CancellationTokenSource cancellation;
        Label status,preview,actorsLabel;
        VisualElement inspector;
        Button generate,apply;
        ObjectField sequenceField;
        [MenuItem("Tools/100Hour/Cutscene Workbench")]
        public static void Open()=>GetWindow<CutsceneWorkbench>("Cutscene Workbench");
        [InitializeOnLoadMethod] static void InstallPlayCallback()
        {
            EditorApplication.playModeStateChanged-=OnPlayState;EditorApplication.playModeStateChanged+=OnPlayState;
        }
        static double readyAt;
        static void OnPlayState(PlayModeStateChange state)
        {
            EditorApplication.update-=StartPending;
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                readyAt=EditorApplication.timeSinceStartup+.5;
                EditorApplication.update+=StartPending;
            }
            else if(state==PlayModeStateChange.EnteredEditMode)
                SessionState.EraseString("100Hour.Cutscene.Pending");
        }
        static void StartPending()
        {
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<readyAt)return;
            var path=SessionState.GetString("100Hour.Cutscene.Pending","");
            if(string.IsNullOrEmpty(path)){EditorApplication.update-=StartPending;return;}
            var player=UnityEngine.Object.FindFirstObjectByType<CutscenePlayer>();
            if(player==null && EditorApplication.timeSinceStartup<readyAt+10)return;
            EditorApplication.update-=StartPending;
            SessionState.EraseString("100Hour.Cutscene.Pending");
            try
            {
                if(player==null)throw new InvalidOperationException("CutscenePlayerが見つかりません。");
                player.Play(AssetDatabase.LoadAssetAtPath<CutsceneSequence>(path));
            }
            catch(Exception e){Debug.LogException(e);}
        }
        public void CreateGUI()
        {
            if(selected==null)selected=FindPlayer()?.sequence;
            var root=rootVisualElement;root.Clear();root.style.paddingLeft=12;root.style.paddingRight=12;root.style.paddingTop=12;
            root.Add(new Label("カットシーン編集 / 自然言語カメラ") {style={fontSize=19,unityFontStyleAndWeight=FontStyle.Bold,marginBottom=10}});
            actorsLabel=new Label();root.Add(actorsLabel);UpdateActors();
            var file=sequenceField=new ObjectField("Sequence"){objectType=typeof(CutsceneSequence),allowSceneObjects=false,value=selected};file.RegisterValueChangedCallback(e=>{selected=e.newValue as CutsceneSequence;BindInspector();});root.Add(file);
            var address=new TextField("PythonAPI URL"){value=endpoint};address.RegisterValueChangedCallback(e=>endpoint=e.newValue);root.Add(address);
            var prompt=new TextField("演出指示"){multiline=true,value=instruction};prompt.style.minHeight=90;prompt.RegisterValueChangedCallback(e=>instruction=e.newValue);root.Add(prompt);
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;
            generate=new Button(Generate){text="自然言語から生成（API通信）"};row.Add(generate);row.Add(new Button(()=>cancellation?.Cancel()){text="生成中止"});root.Add(row);
            preview=new Label("生成後にショット一覧を確認して、Sequenceへ適用してください。");preview.style.whiteSpace=WhiteSpace.Normal;root.Add(preview);
            apply=new Button(ApplyDraft){text="生成結果をSequenceへ適用"};apply.SetEnabled(false);root.Add(apply);
            var playRow=new VisualElement();playRow.style.flexDirection=FlexDirection.Row;playRow.Add(new Button(Play){text="保存してPlay再生"});playRow.Add(new Button(()=>FindPlayer()?.Stop()){text="演出停止 / 操作に戻る"});root.Add(playRow);
            status=new Label("Escapeでも演出を中断できます。Shotsは展開して編集・並べ替えできます。");status.style.whiteSpace=WhiteSpace.Normal;root.Add(status);
            inspector=new ScrollView();inspector.style.flexGrow=1;root.Add(inspector);BindInspector();
        }
        CutscenePlayer FindPlayer()=>UnityEngine.Object.FindFirstObjectByType<CutscenePlayer>();
        void UpdateActors(){var p=FindPlayer();actorsLabel.text=p==null?"CutscenePlayerのある参考シーンを開いてください。":"登場人物: "+string.Join(" / ",p.actors.Where(a=>a!=null).Select(a=>a.actorId+" = "+a.displayName));}
        void BindInspector()
        {
            if(inspector==null)return;inspector.Unbind();inspector.Clear();if(selected==null)return;
            var so=new SerializedObject(selected);foreach(var name in new[]{"title","instruction","fadeIn","fadeOut","shots"})inspector.Add(new PropertyField(so.FindProperty(name)));inspector.Bind(so);
        }
        async void Generate()
        {
            if(cancellation!=null)return;
            try
            {
                var player=FindPlayer();if(player==null)throw new InvalidOperationException("参考シーンを開いてください。");
                cancellation=new CancellationTokenSource();generate.SetEnabled(false);apply.SetEnabled(false);status.text="プラン生成中…";
                draft=await new PythonCameraPlanSource(endpoint).Generate(instruction,player.GetActors().Keys.ToArray(),cancellation.Token);
                preview.text=draft.title+"\n"+string.Join("\n",draft.shots.Select((s,i)=>(i+1)+". "+s.label+" / "+s.kind+" / "+s.actorA+"→"+s.actorB+" / "+s.duration+"秒 / "+s.transition));apply.SetEnabled(true);status.text="生成・検証完了。適用すると選択中のSequenceのショットを置き換えます。";
            }
            catch(OperationCanceledException){if(status!=null)status.text="生成を中止しました。";}
            catch(Exception e){if(status!=null)status.text=e.Message;}
            finally{cancellation?.Dispose();cancellation=null;if(generate!=null)generate.SetEnabled(true);}
        }
        void ApplyDraft()
        {
            if(draft==null)return;
            if(selected==null){string path=EditorUtility.SaveFilePanelInProject("Save Cutscene","NewCutscene","asset","保存先を指定");if(string.IsNullOrEmpty(path))return;selected=CreateInstance<CutsceneSequence>();AssetDatabase.CreateAsset(selected,path);}
            Undo.RecordObject(selected,"Apply camera plan");selected.title=draft.title;selected.instruction=instruction;selected.shots=draft.shots.ToList();EditorUtility.SetDirty(selected);AssetDatabase.SaveAssets();sequenceField.SetValueWithoutNotify(selected);BindInspector();status.text="保存しました: "+AssetDatabase.GetAssetPath(selected);Selection.activeObject=selected;
        }
        void Play()
        {
            try
            {
                if(selected==null)throw new InvalidOperationException("Sequenceを選択してください。");var player=FindPlayer();if(player==null)throw new InvalidOperationException("CutscenePlayerが見つかりません。");CameraPlanValidation.Validate(selected.shots,player.GetActors().Keys.ToArray());AssetDatabase.SaveAssets();
                if(EditorApplication.isPlaying)player.Play(selected);else{SessionState.SetString("100Hour.Cutscene.Pending",AssetDatabase.GetAssetPath(selected));EditorApplication.isPlaying=true;}
            }
            catch(Exception e){status.text=e.Message;}
        }
        void OnDisable(){cancellation?.Cancel();}
    }
}
