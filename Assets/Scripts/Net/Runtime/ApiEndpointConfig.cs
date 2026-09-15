using UnityEngine;

namespace HundredHour.Net
{
    /// <summary>PythonAPIクライアント全部が参照する接続先の一括切り替え設定。
    /// Resources/ApiEndpointConfig.asset の useCloud を1箇所トグルするだけで、
    /// ローカル(127.0.0.1)とさくらのクラウド公開サーバーを全クライアント一斉に切り替えられる。</summary>
    [CreateAssetMenu(menuName = "100Hour/Api Endpoint Config", fileName = "ApiEndpointConfig")]
    public sealed class ApiEndpointConfig : ScriptableObject
    {
        [Tooltip("オンにするとさくらのクラウド、オフにするとローカルのPythonAPIに接続します。")]
        [SerializeField] bool useCloud;
        [SerializeField] string localHost = "127.0.0.1:8001";
        [Tooltip("ドメイン+TLS(nginx+Let's Encrypt)経由。ポート省略で443番のHTTPS/WSSを使う。WebGLの混在コンテンツ制限対策。")]
        [SerializeField] string cloudHost = "api.biwacoder.com";
        [Tooltip("クラウド側のAPI_HMAC_SECRETと同じ値。空のままならローカル同様に署名なしで送る(サーバー側もシークレット未設定なら検証しない)。")]
        [SerializeField] string sharedSecret = "";

        // A WebGL build only ever runs on someone else's browser; 127.0.0.1 would point at their
        // machine, not the developer's, so there is no meaningful "local" choice to preserve there.
#if UNITY_WEBGL && !UNITY_EDITOR
        public bool UseCloud => true;
#else
        public bool UseCloud => useCloud;
#endif
        public string Host => UseCloud ? cloudHost : localHost;
        public string Http => (UseCloud ? "https://" : "http://") + Host;
        public string Ws => (UseCloud ? "wss://" : "ws://") + Host;
        // The real secret lives in Resources/ApiSecret.local.txt (git-ignored, so it never reaches the
        // public repository) and is still packed into builds because it sits under Resources. The
        // serialized field stays empty in version control; a missing file simply means unsigned requests.
        string loadedSecret; bool secretLoaded;
        public string SharedSecret
        {
            get
            {
                if (!string.IsNullOrEmpty(sharedSecret)) return sharedSecret;
                if (!secretLoaded) { secretLoaded = true; loadedSecret = Resources.Load<TextAsset>("ApiSecret.local")?.text.Trim() ?? ""; }
                return loadedSecret;
            }
        }

        static ApiEndpointConfig instance;
        public static ApiEndpointConfig Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = Resources.Load<ApiEndpointConfig>("ApiEndpointConfig");
                if (instance == null)
                {
                    instance = CreateInstance<ApiEndpointConfig>();
                    Debug.LogWarning("Resources/ApiEndpointConfig.asset が見つかりません。ローカル既定値(127.0.0.1:8001)を使用します。");
                }
                return instance;
            }
        }
    }
}
