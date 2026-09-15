# 並行作業中の分離調査

## 公開環境の特定（ユーザー追加情報）

- 発生ブラウザ: Chrome。URL: https://play.biwacoder.com/
- 2026-09-15 13:31 JSTにHTTPで確認。公開indexはStableVertexPosition.jsをロードし、createUnityInstanceより前にinstall(canvas)を呼ぶ。
- 公開JSのSHA256は`78e9ea9e6aea8bf9e02ac97b915306eb4af0a9e8b5baeaed4c720b4d7d266d30`。現在のソースと一致。サーバーに前回対策がないという仮説は支持されない。ただし、ユーザーの既存タブが読み込んだ版は未確認。
- 公開dataのLast-Modifiedは2026-09-15 04:29:49 GMT（13:29:49 JST）、Content-Lengthは75,105,854。直近に更新された公開物なので、以前の診断コピーと同一ビルドとは扱わない。indexとJSのLast-Modifiedは前日10:10:42 GMT。
- 公開HTMLのキャッシュ回避用クエリ追加はlocalhost限定。これは公開版で古いファイルを使った証拠ではないが、既存タブの版と最新配信を区別する必要がある。
- 本番への書き込み・デプロイ・保存データ削除は実施していない。

## 結果

噴水会話の四角いノイズは未解決。今回の既存ビルドのコピーでは再現せず、製品側の修正はしていない。Unity Editorのシーン切替、Play操作、Pipeline操作、ビルド、Assets・ProjectSettings・Packagesへの書き込みは行っていない。

## 検証方法と限界

- `Builds/WebGLRelease`を`Temp/webgl-noise-isolated-zqrczaxa/player`へコピーし、別オリジン`127.0.0.1:8897`で実行。最新の並行作業のコードをビルドしたものではない。
- コピーのframeworkだけFSを公開し、以前の診断ビルドのテスト用セーブからResume→Choose(1)で噴水へ進めた。実ユーザーの保存データは使っていない。
- コピーのJSで頂点・フラグメントのshaderSourceを収集（188件）。収集位置は既存invariant注入の手前なので、収集テキストにpragmaがないことは未適用の証拠にならない。
- コピーのJSからBLEND有効かつRGBのSRC/DSTがONE/ONEのdrawArrays/drawElementsを一時停止する比較を実施。停止件数64,416を確認し、通常描画へ戻した。この条件はForwardAdd専用ではなく、instanced draw等をすべて捕捉するものでもない。
- 両方の状態で報告画像の四角は見えなかった。ノイズが出ている状態から消えた実験ではないので、追加ライトを原因と確定する根拠にはしない。

## 新しい調査候補

収集されたCoastal Furnishingの基本パスの頂点プログラムでは、ObjectToWorldを`UnityInstancing_PerDraw0`の`unity_Builtins0Array`から参照する。追加ライト用と考えられるプログラムでは、通常uniformの`hlslcc_mtx4x4unity_ObjectToWorld`を参照する。双方とも位置をObjectToWorld→MatrixVPで変換するが、行列の格納・参照方法が異なる。

これはコンパイルされたプログラムの観測であり、問題の床のdrawで実際にどのプログラムが使われたかはまだ特定していない。中庭の床はStandardでもあるため、Coastal Furnishingだけを直せばよいとも言えない。

Unityの説明でも、Built-inのForwardは追加パスで基本パスと同様にインスタンシングできない制約がある。GLSLのinvariantは入力・演算等の条件付き保証なので、前回のpragmaだけで異なる計算経路の一致を無条件に保証できるとは扱わない。

- Unity: https://docs.unity.cn/6000.1/Documentation/Manual/gpu-instancing-per-instance-properties.html
- GLSL ES仕様 §4.6: https://registry.khronos.org/OpenGL/specs/es/3.0/GLSL_ES_Specification_3.00.pdf

次の比較候補は、症状が出るビルド・カメラで床材質のGPU Instancingを一時的に無効化し、invariantを維持して比較すること。これには別のUnity検証環境または共有Editorを使える時間が必要。未検証のため今回は製品のinstancing設定を変更していない。

## 再開に必要な情報

実際に症状が出たブラウザ名・URLと、可能なら再現する会話直前の状態。URLとブラウザはユーザーへ確認中。公開版のJS適用、キャッシュ、GPU、canvas解像度を合わせてから、追加パス・instancing・影・深度を一つずつ比較する。

前回の修正コードと比較結果は`AskAnotherAI.md`にまとまっている。前回は頂点シェーダーへ`#pragma STDGL invariant(all)`を挿入して屋根の縞を解消した。今回の四角いノイズまで直ったという意味ではない。
