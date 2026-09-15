using System;
using UnityEngine;

namespace HundredHour.Net
{
    /// <summary>個人情報を含まない、インストールごとのランダムID。PDCA用のテレメトリで
    /// 「同じ人の別セッション」を区別するためだけに使う(実名・アカウントとは無関係)。</summary>
    public static class ClientId
    {
        const string PrefKey = "HundredHour.ClientId";
        static string cached;

        public static string Value
        {
            get
            {
                if (!string.IsNullOrEmpty(cached)) return cached;
                cached = PlayerPrefs.GetString(PrefKey, "");
                if (string.IsNullOrEmpty(cached))
                {
                    cached = Guid.NewGuid().ToString("N");
                    PlayerPrefs.SetString(PrefKey, cached);
                    PlayerPrefs.Save();
                }
                return cached;
            }
        }
    }
}
