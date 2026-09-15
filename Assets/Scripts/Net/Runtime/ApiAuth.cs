using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;

namespace HundredHour.Net
{
    /// <summary>PythonAPI(app/security.py)と対になる、timestamp + nonce + HMAC-SHA256の簡易署名。
    /// RunaPhotoServerのEnsurePhotoApiKey.phpと同じ方式。ApiEndpointConfigのSharedSecretが
    /// 空の場合は何も付与しない(ローカル開発・シークレット未設定サーバーではノーオペレーション)。</summary>
    public static class ApiAuth
    {
        public static bool TryCreate(out string timestamp, out string nonce, out string signature)
        {
            string secret = ApiEndpointConfig.Instance.SharedSecret;
            if (string.IsNullOrEmpty(secret)) { timestamp = nonce = signature = null; return false; }

            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            nonce = Guid.NewGuid().ToString("N");
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + nonce));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            signature = sb.ToString();
            return true;
        }

        /// <summary>REST用: X-Auth-* ヘッダーを付与する。X-Client-Idは署名の有無に関わらず常に送る
        /// (秘密情報ではなく、PDCA用テレメトリで「同じ人の別セッション」を区別するためだけの匿名ID)。</summary>
        public static void ApplyHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("X-Client-Id", ClientId.Value);
            if (!TryCreate(out string ts, out string nonce, out string sig)) return;
            request.SetRequestHeader("X-Auth-Timestamp", ts);
            request.SetRequestHeader("X-Auth-Nonce", nonce);
            request.SetRequestHeader("X-Auth-Signature", sig);
        }

        /// <summary>WebSocket用: クエリ文字列の断片(&amp;client_id=...&amp;auth_ts=...&amp;auth_nonce=...&amp;auth_sig=...)。
        /// client_idは常に付与、署名部分はシークレット未設定なら空。</summary>
        public static string QuerySuffix()
        {
            string suffix = "&client_id=" + Uri.EscapeDataString(ClientId.Value);
            if (!TryCreate(out string ts, out string nonce, out string sig)) return suffix;
            return suffix + "&auth_ts=" + Uri.EscapeDataString(ts) + "&auth_nonce=" + Uri.EscapeDataString(nonce) + "&auth_sig=" + Uri.EscapeDataString(sig);
        }
    }
}
