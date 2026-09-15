# RealtimeVoice UX 検証（2026-09-12）

- FlowChecks.txt: 14項目PASS。マイクチェックの除外、二つの回答、言い直し、遅延イベント、リセットなど。
- PlayChecks.txt: 14項目PASS。状態別UI、文字起こしイベント表示、確認ボタン、実APIでのJSON生成とスクロール。
- LiveOpening.txt: 実Realtime接続で日本語の冒頭案内と音声キューの再生完了を確認。入力受付直前に切断し、ユーザーのマイク音声は送信していません。
- Python unittest: 3項目PASS。日本語プロンプト、手動VAD設定、二回答の抽出。
- 01〜08のPNG: 開始、案内、確認、質問、生成中、結果の画面。

文字起こしのUI検証には模擬イベントを使用しています。実際の人の声による全工程の通し確認は未実施です。

実装参照: [OpenAI Realtime VAD](https://developers.openai.com/api/docs/guides/realtime-vad)。create_response=false / interrupt_response=false で境界検出を残し、アプリが応答開始を制御します。

追加確認: Unity Editor全体のaudioMasterMuteが有効だったため解除。ユーザーから動作良好の確認を受けました。検証終了後はPlay停止、Realtime通信停止。ミュート解除はEditor設定であり、シーンの変更ではありません。
