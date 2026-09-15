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
