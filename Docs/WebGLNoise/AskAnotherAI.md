# 他のAIへの相談用

Unity 6000.3.19f1のBuilt-in Render Pipelineのゲームです。WebGLで、アニメーション／カメラ変化中に建物の屋根・床へ横筋が出る問題がありました。前回修正後も「庭（噴水）を選んだ後の会話」で、床に32pxまたは64px幅の暗い四角と横筋が出たという追加報告があります。画像も添付して相談してください。追加報告時のURL・ブラウザ名はまだ不明です。

## 前回の実験と結論

Apple M4のChromium系WebGL（ANGLE/Metal）で屋根の縞を再現。

1. 太陽光の影をOFF → 屋根の縞は残る。
2. shadowBias .025→.05、NormalBias .07→.4 → 解決しない。
3. Near Clip .1→.5、Far 15000維持 → 改善するが残る。
4. `HundredHour/Coastal Furnishing` をUnity Standardへ交換 → 消える。
5. 同じ元材質のまま全頂点シェーダーにGLSL invariant指定 → 消える。

この実験から、Forwardの複数ライティングパス間の頂点位置計算の微差による自己Z-fightingと判断しました。屋根メッシュ同士の物理的重複はそれ以前に修正済みでした。影を消すことやClip変更は最終修正には採用していません。

## 採用済みの修正コード

`Assets/WebGLTemplates/HundredHour/StableVertexPosition.js`:

```javascript
// Keep identical geometry at identical depth across Unity's lighting passes.
// ANGLE/Metal can otherwise optimize each vertex program differently, producing
// stripes on a single surface (especially while the camera moves).
(function () {
  "use strict";
  var installedCanvases = new WeakSet();
  var installedContexts = new WeakSet();

  window.HundredHourStableVertexPosition = {
    install: function (canvas) {
      if (installedCanvases.has(canvas)) return;
      installedCanvases.add(canvas);
      var getContext = canvas.getContext;
      canvas.getContext = function () {
        var context = getContext.apply(this, arguments);
        if (!context || typeof context.shaderSource !== "function" || installedContexts.has(context)) return context;
        installedContexts.add(context);
        var shaderSource = context.shaderSource;
        context.shaderSource = function (shader, source) {
          if (shader && typeof source === "string" &&
              this.getShaderParameter(shader, this.SHADER_TYPE) === this.VERTEX_SHADER &&
              !/^\s*#pragma\s+STDGL\s+invariant\s*\(\s*all\s*\)/m.test(source)) {
            // A preprocessor directive is valid before #extension declarations.
            // #version must remain first when present (GLSL ES 3.00 / WebGL 2).
            var directive = "#pragma STDGL invariant(all)\n";
            var version = /^[ \t]*#version[^\r\n]*(?:\r?\n|$)/m;
            source = version.test(source)
              ? source.replace(version, function (line) { return line.replace(/\r?\n?$/, "\n") + directive; })
              : directive + source;
          }
          return shaderSource.call(this, shader, source);
        };
        return context;
      };
    }
  };
}());

```

テンプレートindex.htmlで、Unityインスタンスを作る前に以下を実行:

```html
<script src="StableVertexPosition.js"></script>
<script>
  var canvas = document.querySelector("#unity-canvas");
  HundredHourStableVertexPosition.install(canvas);
  // この後に既存のcreateUnityInstance(canvas, config, ...)を実行
</script>
```

#versionを先頭に保つ、頂点シェーダーだけ変更、同じcanvas/contextを二重ラップしない。他canvasやグローバルWebGLプロトタイプは変更しません。Fragment shaderは手付かず。Shaderの元のHLSLや材質を直接書き換えているわけではありません。

## 確認できた範囲／できていない範囲

- 前回: ChromiumとSafariで修正ありの屋根を確認。製品ビルドでも上空ズーム・入口・Walkingで確認。
- 今回: 窓際床の固定2角度では修正ありで正常。修正なしに戻すと、床に灰色の大きな領域と横筋が再現。
- 今回: テスト用セーブから実際の庭選択→移動→噴水の初回会話と返答まで確認。通常サイズと拡大表示の双方で、報告の四角は再現できなかった。報告の「ネイル」に関する発話そのものや全アニメーションフレームは未検証。
- Source / WebGLRelease / WebGLTestのJSは同一。公開サーバー上の版とブラウザキャッシュは未検証。現ProjectSettingsはHundredHourテンプレート、WebGL Threads OFF、Graphics Jobs OFF。
- 追加の四角いノイズは未解決。上記の「影が原因ではない」は前回の屋根についての実験結果であり、今回の四角まで否定するものではありません。

## 次に切り分けたい点

- 実際に不具合が出たブラウザ・GPU・URL・canvas描画サイズ、およびその版でinvariantが実際に注入されているか。
- 同じ会話カメラで、床の材質とForwardAdd、影、カメラのdepthと重複メッシュを切り替えて、どのdraw passで四角が生じるか。
- `CoastalFurnishing.shader` はSurface Shader Standard/fullforwardshadows、worldPosベースの石・布・木のノイズと法線を使用。中庭床自体はStandard、屋根やホール床はCoastal Furnishing。
- `WaterDroplet.shader` の透明度が `smoothstep(1,.3,r)`。GLSLではedge0>=edge1は未定義で、候補の修正は `1.0-smoothstep(.3,1.0,r)`。ただし水しぶきと今回の暗いブロックの関係は未確認なので原因確定扱いしないでください。

仕様: https://registry.khronos.org/OpenGL/specs/es/3.0/GLSL_ES_Specification_3.00.pdf

既存のコード・シーン・未コミット作業を保持し、再現結果を根拠に提案してください。invariantだけですべて直ったとは扱わないでください。
