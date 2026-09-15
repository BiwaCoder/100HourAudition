# WebGLの面に出る縞・カメラ移動中のちらつき（2026-09-14）

## 原因の切り分け

SeasideMansionAuditionの複製をWebGLにビルドし、固定カメラで報告画像と同じ屋根の横縞を再現した。OpenGLES3 / ANGLE Metal Renderer Apple M4で検証。

- 太陽光のBias .025 / Normal Bias .07 → .05 / .4: 解消せず。
- 太陽光の影を完全に無効化: 解消せず。shadow acneではない。
- Near Clip .1 → .5、Far Clip 15000維持: 改善するが一部が残る。
- 屋根のCoastal Furnishing材質をStandardに切替: 消える。
- 元の材質・影・カメラ設定で頂点シェーダーにGLSLのinvariant指定: 消える。

以上から、同じ面を光源ごとの複数パスで描くときの頂点計算の不一致による自己Z-fightingと判断。カメラ移動に応じて深度比較の成否が変わり、ノイズとして見える。屋根スラブ同士は既に10m幅に修正済みで、重複の再修正は不要だった。

## 修正

`Assets/WebGLTemplates/HundredHour/StableVertexPosition.js` をUnity起動前に読み込む。ゲームのcanvasが取得したWebGLコンテキストだけを対象に、頂点シェーダーへ `#pragma STDGL invariant(all)` を付与する。GLSLの同一演算の出力をパス間で一致させる指定で、材質の質感・追加ライト・影は保持する。

- #versionを先頭に保ち、#extensionより前に置けるプリプロセッサ指定を使用。
- フラグメントシェーダーは変更しない。
- 二重インストールと同一コンテキストの重複ラップを防ぐ。
- WebGLテンプレート経由で全シーンに適用。CinemachineやシーンのClip値は変更しない。
- 他canvasやブラウザ全体のWebGLプロトタイプは変更しない。
- 元々あった音声開始ゲートと未コミットの音声修正は保持。

## 検証

`node Docs/WebGLNoise/verify-vertex-invariance.cjs` : PASS。
頂点だけへの適用、フラグメントの保持、#version/#extensionの順序、CRLF、WebGL1形式、二重初期化、繰返しgetContext、無効shaderの委譲を検証。

検証用の一時SceneとMonoBehaviourはUnity APIで削除し、EditorはAuditionTitle / Play停止へ戻した。検証用生成物はgit管理外のBuilds/WebGLNoiseProbeとTemp/WebGLNoiseに保持。配布ビルドには検証ボタンを含まない。

追加確認: Chromium系のCodex内ブラウザ（ANGLE/Metal Apple M4）とSafari（OpenGLES3 / Apple GPU）の両方で、製品用のcanvas限定ラッパーを読み込んだ検証ビルドが起動し、屋根の横縞が消えることを確認。Chromium側のシェーダー関連Console error/warnはなし。

仕様上の根拠: [Khronos GLSL ES 3.00仕様 §4.6](https://registry.khronos.org/OpenGL/specs/es/3.0/GLSL_ES_Specification_3.00.pdf) は、別々にコンパイルされた頂点シェーダーの同一式でも出力が微妙に異なり、マルチパスで位置合わせの問題が起きることと、invariantによる一致の条件を説明している。invariant指定は頂点処理の最適化を制約するため、GPU性能への影響は未計測。


最終配布ビルド: Builds/WebGLRelease、同一成果物をBuilds/WebGLTestへ反映。成功（80,258,282 bytes、エラー0、警告17）。警告は既存のPipeline設定・シーン間参照・非推奨API・未使用フィールドとSkyStudioの整数演算に関するもの。詳細はbuild-result.json。修正JSのソース/配布版SHA256一致、index.htmlの初期化呼び出しも確認。

完成ビルドのChromium実行でタイトル→本編の上空ズーム（報告と同じ字幕のカット）の屋根が縞なしで描画され、入口カットからWalkingへ遷移することを確認。写真シーンの表示とメニュー復帰も確認。Consoleに描画エラーなし、保存同期APIの非推奨警告のみ。追加画像そのものの窓際カットは独立には再現していない。公開サーバーへのデプロイは未実施。
