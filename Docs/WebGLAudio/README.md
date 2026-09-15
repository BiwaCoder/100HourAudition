# WebGL会話音声修正 (2026-09-14)

## 原因
- Unity 6000.3.19f1の生成frameworkではWEBAudioは内部変数で、Module.WEBAudioは公開されない。以前のHTMLのresume処理は何も実行していなかった。
- Unity自身のAudioContextにはmousedown/touchstartでの再開処理がある。
- PcmStreamPlayerはOnAudioFilterReadで受信PCMを供給していた。WebGLにはこの再生経路がなく、無音クリップだけが再生されていた。

## 修正
- WebGLビルドのみVoicePlayback.jslibを使用。24kHz/mono/PCM16をAudioBufferへコピーし、AudioBufferSourceNodeを音声クロックで連続予約する。
- ネイティブ/Editorでは既存のOnAudioFilterRead経路を維持。
- BeforeSceneLoadで共有AudioContextとcapture-phaseのpointerdown/touchend/keydown/clickを登録。タイトル画面の操作で有効化し、マイクと再生で共有する。Unity内部のWEBAudioには依存しない。
- 再生待ちは実際の予約音声の残量、波形はAnalyserNodeの出力を使用。Clear/シーン終了で予約ソースを停止し、UnloadでイベントとContextを解放。
- マイク停止では共有Contextを閉じない。許可要求のキャンセル後に到着したMediaStreamは即停止。マイクの出力バッファは明示的に無音化。
- HundredHourテンプレートから無効なModule.WEBAudio参照を撤去。

## 検証
`node Docs/WebGLAudio/verify-audio.cjs` は音声デバイス/外部APIを使わないモック検証。
PCM16の正負値、チャンク連続予約、suspendedからのクリック再開、波形、待ち残量、停止/破棄、許可待ちキャンセル、Context共有とUnloadの後片付けを確認。
Unityコンパイル成功。WebGLビルド成功（エラー0、警告9）。ビルド結果はbuild-result.txt参照。
ローカル配信時は生成ファイルのURLに時刻を付け、Unityのデータキャッシュをno-storeにする。通常のホストでのキャッシュ動作は変更しない。
ブラウザ検証: 新しいoriginでタイトル表示とクリック時のUnity AudioContext再開ログを確認。元のoriginでは更新前キャッシュとの混在が疑われるWASM起動エラーがあり、上記のローカルキャッシュ回避を追加。

## 手元での確認
http://127.0.0.1:8890/ をキャッシュを更新して再読み込みし、ロード完了後にタイトルをクリックして音声キャラメイクへ進む。
コンソールでは以下で会話音声Contextの状態を確認できる。

```js
hundredHourUnityInstance.Module.hundredHourVoiceAudioState()
```

クリック後はrunningが期待値。初期のautoplay警告が履歴に残っていても失敗とは限らない。
実マイク/外部APIを使った会話音声の試聴は別途必要。

キャッシュ回避後、元の http://127.0.0.1:8890/ でもタイトル画面の正常起動を確認。実会話の試聴は未実施。


## 2026-09-14 公開後の無音報告への対応

ユーザーのスクリーンショットにはUnityのAudioContext再開成功ログがある。警告の存在だけでは失敗と断定できない。
タイトルはAuditionTitleNarratorで実際にAPI音声案内を行っている（タイトルが元々無音という見立ては撤回）。

確認した事実:
- 公開frameworkとローカルWebGLReleaseのframeworkはSHA256一致。データ/WASM全体の比較は公開ダウンロードのタイムアウトで未完了。
- プロジェクト設定の署名を使った公開/ローカルAPIへの接続は、ともにlive.readyとlive.audioを受信。署名なしの公開接続は403になるが、設定済み署名では成功。
- 修正前のローカル配布ビルドを計測すると、操作前に両Contextがsuspendedのまま144チャンク程度まで受信・予約。クリック後は両Contextがrunningになった。報告された「クリックしても無音が続く」はこの環境では再現できず、利用者環境の直接原因は未断定。
- ApiEndpointConfig.csにはユーザーによるWebGL常時クラウド接続の未コミット変更がある。保持しており、ローカルWebGLもapi.biwacoder.comへ接続する。Editorは現在useCloud=falseでローカルAPI。

修正:
- ロード後に音声開始ボタンを表示し、会話用Contextがrunningになったと確認してから解除。
- タイトル案内の接続開始をAudioReadyまで待つ。音声開始前に案内を消費しない。
- 案内の準備タイムアウト/不意の切断を通知。接続初期化時の例外も捕捉。
- Module.hundredHourVoiceAudioDiagnostics()で状態、音声クロック、プレイヤーごとの受信チャンク数/予約ソース数を確認可能（認証情報や会話内容は含まない）。

検証:
- Unityコンパイル成功、WebGLReleaseビルド成功、エラー0・警告9。
- verify-audio.cjs PASS。
- ブラウザ実ビルド＋公開API: ボタン操作前はstate=suspended、受信0、WebSocket未接続。
- ボタン操作後はstate=running、live.ready=1、live.audio=136、receivedChunks=136、maxRms=0.18044297191085296。ブラウザのAnalyserNodeに非ゼロの音声出力を確認。Console error/warnなし。
- マイク録音は未実施。物理スピーカーから聞こえるかはユーザーの端末で確認が必要。
- 計測用audio-probe.htmlは検証後削除。必要ならmake-browser-probe.pyで生成できるが、公開物には含めない。
- Builds/WebGLReleaseが再アップロード用。Builds/WebGLTestにも同じ成果物をコピーした。公開サーバーへの再デプロイは未実施。
