# 噴水の会話中に残る四角いノイズ — 2026-09-15

ユーザー報告: 床に32px程度の四角い暗部と横筋。選択肢で選んだ噴水シーンの会話で発生したと思う、との補足。ブラウザ名・URLはまだ未確認。

今回の検証では報告された四角い模様を再現できず、原因未確定。製品のシェーダー・材質・影・カメラ設定は変更していない。

- 接続Editorは6000.3.19f1、AuditionTitle、Play停止。未コミット変更を保持。
- シーンを一時的に加算ロードして床のBoundsを確認。Continuous ground slab上端0、Courtyard paving上端約.015、Hall floor上端.06。単純な同一平面ではない。ただしこれだけで深度問題を全否定しない。
- 窓際の床を2角度から見る検証専用WebGLを作成。ANGLE Metal / Apple M4、960x600。invariant有りでは四角いノイズなし。invariantを外すと窓際の床に大きな灰色領域と横筋が再現。従来修正はこのケースに有効。
- 噴水の場所情報を受け、既存WebGLReleaseの診断用コピーからテスト専用のRouteTravelResume.jsonを読み込み、実際のChoose(1)→TravelToRoute→会話→相手の返答まで確認。960x600と拡大表示の双方で、今回の四角い模様は見られなかった。実ユーザーのセーブデータは使っていない。女性NPCとの初回会話で確認したが、報告文面の「ネイル」そのものの発話ではない。すべてのアニメーションフレームを確認したという意味ではない。
- 診断用コピーのみ、frameworkのFSを公開してテスト保存を読み込み、HTMLの検証ボタンから既存Resume/Chooseを呼んだ。製品版frameworkは未変更。診断コピーは配布しない。
- 一時Scene/MonoBehaviourはUnity APIで削除し、AuditionTitleへ戻した。ブラウザ検証タブは閉じ、viewportを元に戻した。

ローカル診断成果物（git管理外）: Builds/WebGLFloorProbe、Builds/WebGLRouteProbe、Temp/WebGLNoise2。次回は実際に症状が出たURLとブラウザを特定し、その版のStableVertexPosition.jsの適用と同じ会話構図を確認する。

## 追加の静的確認

- Source / WebGLRelease / WebGLTest のStableVertexPosition.jsはSHA256が一致: `78e9ea9e6aea8bf9e02ac97b915306eb4af0a9e8b5baeaed4c720b4d7d266d30`。JSの既存検証はPASS。
- ProjectSettingsは`PROJECT:HundredHour`、WebGL Threads OFF、Graphics Jobs OFF。現設定ではOffscreenCanvas/worker経由でラッパーを迂回することを積極的に示す証拠なし。公開版は未確認。
- 報告PNGの一部の暗部は厳密に32px幅（x=343..374）、別の暗部は64px幅（x=311..374）。スクリーン上のブロック状描画不整合を調査する材料になるが、これだけでGPUタイル/深度/影のどれかを確定できない。
- `Assets/Scripts/Environments/SeasideMansion/Shaders/WaterDroplet.shader:10` に `smoothstep(1,.3,r)` がある。GLSL ES 3.0 §8.3ではedge0>=edge1の結果は未定義。意図した「中心で不透明、縁で透明」は `1.0-smoothstep(0.3,1.0,r)` と書ける。ただし今回の暗い32pxブロックと関連するか未検証で、この式は今回は変更していない。
- 原本仕様: https://registry.khronos.org/OpenGL/specs/es/3.0/GLSL_ES_Specification_3.00.pdf （§4.6 invariant、§8.3 smoothstep）。invariantは同じ入力・演算等の条件を満たした出力の一致を制約するもので、異なる計算式を同じにする万能な修正ではない。fragmentの不定値や別メッシュの重なりも修正しない。
- 今回新規に判明した描画上の問題に対する製品コードの修正・本番ビルド・デプロイはなし。検証に伴う動的フォントBerkshireSwash.assetの差分があり、一括復元せず保持。最後のcleanupコマンドはAuditionTitle復帰・一時Scene/Script削除の成功を返した。その後のPipeline状態確認は到達不能だったため、次回Editorの現在状態を再確認する。
