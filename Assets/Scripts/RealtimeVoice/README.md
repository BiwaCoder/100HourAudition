# RealtimeVoiceTest

`Assets/Scenes/GameScene/RealtimeVoiceTest.unity` を開いてPlayし、「音声でキャラクターを作る」を押します。PythonAPIを8001番で起動しておきます（`PythonAPI` で `.venv/bin/python run.py`）。マイクの利用許可が必要です。

## 進行

1. 相手が日本語でキャラクター作成とマイクチェックを案内します。
2. 案内の再生後に入力を受け付け、認識した発話を画面に表示します。「マイクOK・質問へ」で進みます。
3. 好きなもの・夢中になっていることを質問。回答を確認し、次へ進みます。
4. 性格・大切にしていることを質問。二つの回答を確認して生成します。
5. キャラクターJSONを表示。スクロール・コピー・再開始できます。

確認画面では「言い直す」でその回答だけ録り直せます。マイクチェックの発話は生成に含めません。相手の案内中、確認中、生成中は音声を送信しません。不要なパネル・ボタンは状態に応じて無効化します。

## 構成

- `VoiceInterviewFlow`: Unityや通信から独立した状態・確認済み回答。
- `RealtimeVoiceDemoController`: 接続、発話、文字起こし、確認、生成の進行。
- `RealtimeVoiceDemoView`: 日本語TMP UI、状態別表示、入力音量。
- `RealtimeVoiceSession`: WebSocket、入力ゲート、手動response.create、古いイベントの除外。
- `MicrophoneCapture` / `PcmStreamPlayer`: PCM入力・出力とサンプルレート変換。
- `BuildCharacterApiClient`: 既存PythonAPIへの二回答送信。中止時にリクエストをAbort。

RealtimeのVADは発話境界の検出だけに使用し、自動応答を無効化。進行はアプリが所有します。既存モデル・声設定はPythonAPIに残しています。質問文は `VoiceInterviewFlow`、抽出の説明は `PythonAPI/app/character_prompts.py` にあります。

シーンのUI構築は適用済みです。`RealtimeVoiceUxSetup.Apply()` は再実行不要。変更前シーンは `Backups/RealtimeVoice/BeforeUx.unity` に保存しています。

## 検証

`Checks~/VerifyFlow.cs` はEditorのCLI evalで実行する純粋な進行チェックです。`Checks~/VerifyUxPlay.cs` はPlay専用で、文字起こしイベントを模擬し、最後に実際の生成APIを1回呼びます。通常起動では実行されません。

Python側: `PythonAPI` で `.venv/bin/python -m unittest discover -s tests -v`。

結果・画面は `Docs/RealtimeVoiceUx`。実マイクでの日本語認識精度とスピーカーの回り込みは利用環境での確認が必要です。

## Terminal UI（2026-09-12）

黒地の低コントラスト・デジタルノイズと白文字、輪郭線のボタンに変更。既存の進行・音声API・JSON生成は維持しています。

- AIの実再生PCMを青い64帯域スペクトラムで表示。マイク受付中は送信対象のPCMを赤で表示します。
- 最初の有効な文字起こしを受信すると、その後のAI表示が紫へ変化。再開始・中止・エラーで青へ戻ります。
- マイクチェックではタイプライター、質問では短いノイズの復号、生成・完成では広がった復号帯で文字を表示。演出は最大約1.6秒を目安に追いつきます。字幕ストリームの追記では先頭から再生しません。
- 生成中のみ中央の小さな光が呼吸し、完成時に静止。声の波形とは分離しています。
- `TerminalTextReveal.FullText` が正しい全文です。描画中のTMP文字列には一時的な記号が含まれます。コピーにはFlowの完成JSONを使用し、演出中も正確です。
- `VoiceSignalBuffer`: 1024サンプルのスレッド安全な音声窓、Hann窓FFT。UI側30Hz解析、背景240×135を12Hz更新。音声スレッドでUI処理・FFT・配列確保は行いません。
- `VoiceTerminalPresentation` / `TerminalSpectrumGraphic` / `TerminalTextReveal`: 表示専用。外部接続を開始しません。Play停止で音声セッション・マイクを閉じ、生成テクスチャを破棄します。

見た目の適用は `RealtimeVoiceTerminalSetup.Apply()`（対象シーンをEditモードで開く）。既存のUIを再利用し、新規装飾は同名オブジェクトを更新します。変更前は `Backups/RealtimeVoice/BeforeTerminal.unity`。

外部通信なしの検証: CLI evalから `HundredHour.RealtimeVoice.Editor.RealtimeVoiceTerminalChecks.Run()`。EditではFFTと進行、PlayではさらにJSONコピー、青→紫→リセット、結果・確認・言い直し・エラーのUIを確認します。Playチェックは未接続で実行し、最後に待機へ戻します。

## 案内役AI-0

案内役は「生まれたばかりのAI案内役AI-0（エーアイゼロ）」を名乗る架空のAI。冒頭は「私はAI-0、エーアイゼロ。生まれたばかりのAIだ。」。人物設定は `PythonAPI/app/characters/astra.json`、音声ベースは `PythonAPI/app/api/realtime.py` の `VOICE = "cedar"`。低く深い成熟した男性の声、自然な間、控えめな抑揚をセッションと各発話で指示しています。

[OpenAIのRealtime音声仕様](https://developers.openai.com/api/docs/guides/realtime-conversations#voice-options)に沿って、新しい接続時に音声を設定します。自動リロードを使わずPythonAPIを起動している場合は、サーバー再起動後に会話を開始してください。今回、実APIによる音声の試聴は行っていません。
