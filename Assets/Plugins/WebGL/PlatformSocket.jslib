// Bridges HundredHour.Net.PlatformSocketWebGL (Assets/Scripts/Net/Runtime/PlatformSocketWebGL.cs)
// to the browser's native WebSocket. Text frames only -- every caller in this project sends and
// receives JSON, so binary framing was left out to keep this small.
var LibraryPlatformSocket = {
    $platformSocketState: {
        instances: {},
        lastId: 0,
        onOpen: null,
        onMessage: null,
        onError: null,
        onClose: null,
    },

    PlatformSocketSetCallbacks: function (onOpen, onMessage, onError, onClose) {
        platformSocketState.onOpen = onOpen;
        platformSocketState.onMessage = onMessage;
        platformSocketState.onError = onError;
        platformSocketState.onClose = onClose;
    },

    PlatformSocketAllocate: function (url) {
        var id = platformSocketState.lastId++;
        platformSocketState.instances[id] = { url: UTF8ToString(url), subprotocols: [], ws: null };
        return id;
    },

    PlatformSocketAddSubProtocol: function (id, subprotocol) {
        var instance = platformSocketState.instances[id];
        if (instance) instance.subprotocols.push(UTF8ToString(subprotocol));
    },

    PlatformSocketConnect: function (id) {
        var instance = platformSocketState.instances[id];
        if (!instance || instance.ws) return -1;

        var ws;
        try {
            ws = new WebSocket(instance.url, instance.subprotocols.length ? instance.subprotocols : undefined);
        } catch (err) {
            PlatformSocketEmitError(id, "WebSocketの作成に失敗しました: " + err);
            return -2;
        }
        instance.ws = ws;

        ws.onopen = function () {
            if (platformSocketState.onOpen) { Module.dynCall_vi(platformSocketState.onOpen, id); }
        };
        ws.onmessage = function (ev) {
            if (typeof ev.data !== "string") return; // binary frames are not used by this project
            PlatformSocketEmitMessage(id, ev.data);
        };
        ws.onerror = function () {
            PlatformSocketEmitError(id, "WebSocket error");
        };
        ws.onclose = function (ev) {
            if (platformSocketState.onClose) { Module.dynCall_vii(platformSocketState.onClose, id, ev.code || 1006); }
            delete instance.ws;
        };

        return 0;
    },

    PlatformSocketSendText: function (id, message) {
        var instance = platformSocketState.instances[id];
        if (!instance || !instance.ws || instance.ws.readyState !== 1) return -1;
        instance.ws.send(UTF8ToString(message));
        return 0;
    },

    PlatformSocketClose: function (id, code) {
        var instance = platformSocketState.instances[id];
        if (!instance || !instance.ws) return -1;
        try { instance.ws.close(code); } catch (err) { return -2; }
        return 0;
    },

    PlatformSocketGetState: function (id) {
        var instance = platformSocketState.instances[id];
        if (!instance || !instance.ws) return 3; // Closed
        return instance.ws.readyState;
    },

    PlatformSocketFree: function (id) {
        var instance = platformSocketState.instances[id];
        if (!instance) return;
        if (instance.ws && instance.ws.readyState < 2) { try { instance.ws.close(); } catch (err) {} }
        delete platformSocketState.instances[id];
    },

    // Helpers below allocate a NUL-terminated UTF8 buffer for a JS string and pass it to the
    // registered C# callback; not exported themselves ($-prefixed), just shared by the functions
    // above, which must call them through the `platformSocketState`-style bare names (no `$`).
    $PlatformSocketEmitMessage: function (id, text) {
        if (!platformSocketState.onMessage) return;
        var length = lengthBytesUTF8(text) + 1;
        var buffer = _malloc(length);
        stringToUTF8(text, buffer, length);
        try { Module.dynCall_vii(platformSocketState.onMessage, id, buffer); }
        finally { _free(buffer); }
    },

    $PlatformSocketEmitError: function (id, text) {
        if (!platformSocketState.onError) return;
        var length = lengthBytesUTF8(text) + 1;
        var buffer = _malloc(length);
        stringToUTF8(text, buffer, length);
        try { Module.dynCall_vii(platformSocketState.onError, id, buffer); }
        finally { _free(buffer); }
    },
};

autoAddDeps(LibraryPlatformSocket, '$platformSocketState');
autoAddDeps(LibraryPlatformSocket, '$PlatformSocketEmitMessage');
autoAddDeps(LibraryPlatformSocket, '$PlatformSocketEmitError');
mergeInto(LibraryManager.library, LibraryPlatformSocket);
