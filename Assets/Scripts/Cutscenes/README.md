# 自然言語カットシーン

## 最初に使う

1. `Assets/Scenes/GameScene/NatureCutsceneLab.unity` を開く。NatureStageをコピーした編集用シーンです。
2. Playして右上の **PLAY CUTSCENE** を押すと7ショットを再生。Escapeで中断できます。
3. **Tools > 100Hour > Cutscene Workbench** を開く。初期Sequenceは `Assets/Cutscenes/Sequences/NatureConversation.asset`。
4. 日本語で演出指示を入力し「自然言語から生成」を押す。PythonAPI `http://127.0.0.1:8001` が必要です。OpenAIへ通信します。
5. 生成一覧を確認して「生成結果をSequenceへ適用」。既存Sequenceを選択している場合はショット一覧を置き換えます（Undo対応）。未選択なら新規保存します。
6. 下のShotsを展開して種類、秒数、人物、ズーム、切り替えを編集・並べ替え。「保存してPlay再生」で確認できます。

生成と適用は別操作です。APIが失敗しても既存プランは変更されません。通信なしでもサンプルや保存済みSequenceを編集・再生できます。

実際の日本語生成例は `Assets/Cutscenes/Sequences/NaturalLanguageExample.asset` に保存しています。

## 人物の割り当て

シーンの人物に `CutsceneActor` を付け、重複しないactorIdを設定します。サンプルは A=コーラル、B=青、C=紫。CutsceneDirectorのActors配列へ登録してください。Aim HeightはTransform中心から狙う高さ、Framing Radiusは全身を収めるための余白です。モデルに差し替えてもIDを保てばプランを流用できます。

Stage Forwardは人物の正面側を定義するワールド方向（初期+Z）。RearWideはこの反対側から撮ります。

## ショット

| Kind | 内容 |
|---|---|
| Single | 指定人物を撮る |
| TwoShot | A/Bを横に並べて収める構図 |
| OverShoulder | Aの肩越しにB |
| ReverseShoulder | Bの肩越しにA |
| RearWide | 登録された全員を背後から引きで撮る |
| GroupWide | 登録された全員を正面から引きで撮る |
| Orbit | 対象の周囲を回り込む |
| DollyIn / DollyOut | カメラそのものが寄る／引く |

Start Fov→End Fovを変えると、位置移動とは独立したレンズズームです。例えば40→25でズームイン、25→50でズームアウト。参考プロジェクト同様、FOVを直接線形補間せず焦点距離をSmoothStepで補間します。

TwoShot/GroupWide/RearWideでは、全員が入るよう距離を最低限補正します。ズーム終端の狭い画角でも入るように計算するため、開始時は広めになります。人物を横へ移動・整列する機能ではなく、現在の配置に対する撮影構図です。

## 切り替えと操作復帰

- Cut：瞬時に切り替え。
- Blend：カメラの位置・向き・焦点距離を補間。
- Fade：黒へフェードアウト→次の構図へ切り替え→フェードイン。
- Durationはショット動作の秒数。Transition SecondsはBlendでは移動時間、Fadeでは片道の時間で、Durationに加算されます。
- 最初のショットはTransitionの代わりにSequenceのFade Inを使用します。
- SequenceのFade Inは冒頭、Fade Outは最後の暗転とゲーム画面への復帰に使用。

再生中だけ専用CinemachineCameraと黒帯・フェードCanvasを生成します。Suspend During PlaybackのBehaviourを停止し、Hide During PlaybackのUIを隠し、登録人物のRigidbodyを一時的にkinematicにします。終了・Stop・コンポーネント無効化で元の有効状態とカメラ設定を戻します。通常操作は停止状態から再開します。

## 設計・移植元

- CutsceneSequence / CameraPlanValidation：保存用データと検証。
- CameraShotSolver：構図・複数人物の収まり・焦点距離補間。
- CutscenePlayer：再生と通常カメラ／操作の復帰。
- ICameraPlanSource / PythonCameraPlanSource：自然言語からプラン取得。既存AIChat DTOとPythonAPIを再利用。
- Editor/CutsceneWorkbench：UI Toolkitの編集ウィンドウ。

参考元は3D_Movie_CharMoveのCineDirector（PromptBuilder、CameraShotSolver、PlanPlayer）とCameraOrbit/CameraMenuButtons.ZoomRoutine。カメラ用に責務を整理して移植し、人物アニメーション・表情・ゲーム進行への依存を除きました。今回は順序付きSequenceによる編集で、Unity Timelineのトラックではありません。

本機能はカメラ演出用です。俳優の歩行演技・口パク・表情・音声は含みません。地面にカメラが埋まらない補正はありますが、木・建物の遮蔽物を自動回避する機能はありません。肩越し構図はモデルの大きさと立ち位置によって調整してください。元のNatureStageはこの作業で変更していません。

## 確認結果

7ショット再生、ズーム40→25／25→50、途中のフェード値、正常終了・停止・無効化時の操作復帰、生成プランの再生を確認済み。二人と全員が画面に収まり、肩越しの対象までの視線が手前の人物で遮られないことも確認しました。
