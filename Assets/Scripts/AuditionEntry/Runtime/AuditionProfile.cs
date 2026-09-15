using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using HundredHour.RealityShow;
namespace HundredHour.AuditionEntry
{
    public static class AuditionProfile
    {
        public static string DirectoryPath => Path.Combine(Application.persistentDataPath,"AuditionEntry");
        public static string Gender=>PlayerPrefs.GetString("Audition.PlayerGender","female");
        public static bool IsMale=>Gender=="male";
        public static void SelectGender(bool male){PlayerPrefs.SetString("Audition.PlayerGender",male?"male":"female");PlayerPrefs.Save();}
        public static string DefaultName=>IsMale?"悠真":"ひまり";
        public static string JsonPath => Path.Combine(DirectoryPath,IsMale?"participant-male.json":"participant.json");
        public static string PortraitPath => Path.Combine(DirectoryPath,IsMale?"portrait-male.png":"portrait.png");
        public static JObject Read()
        {
            try { return File.Exists(JsonPath)?JObject.Parse(File.ReadAllText(JsonPath)):null; }
            catch(Exception e){Debug.LogWarning("Participant could not be loaded: "+e.Message);return null;}
        }
        public static JObject Character(JObject root)=>root?["characters"]?.First as JObject;
        public static string Name => Clean((string)Character(Read())?["name"]);
        public static string Clean(string value)=>(value??"").Replace("<","〈").Replace(">","〉");
        public static void SaveJson(string json)
        {
            var root=JObject.Parse(json);
            if(Character(root)==null || string.IsNullOrWhiteSpace((string)Character(root)["name"]))throw new FormatException("人物の名前がありません / Missing character name");
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(JsonPath+".tmp",json);Commit(JsonPath);
        }
        public static void SavePortrait(Texture2D texture)
        {
            Directory.CreateDirectory(DirectoryPath);File.WriteAllBytes(PortraitPath+".tmp",texture.EncodeToPNG());Commit(PortraitPath);
        }
        static void Commit(string path){if(File.Exists(path))File.Replace(path+".tmp",path,null);else File.Move(path+".tmp",path);}
        [Serializable] sealed class CastData {public CastPerson[] cast;}
        [Serializable] sealed class CastPerson {public string id,name,personality,image;public string[] topics;}
        static readonly System.Collections.Generic.Dictionary<string,Sprite> portraits=new System.Collections.Generic.Dictionary<string,Sprite>();
        public static Sprite MalePortrait(string image)
        {
            if(portraits.TryGetValue(image,out var sprite)&&sprite)return sprite;
            var texture=Resources.Load<Texture2D>("AuditionCast/"+image);if(!texture)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));portraits[image]=sprite;return sprite;
        }
        static void ApplyMaleCast(ShowContent content)
        {
            var data=Resources.Load<TextAsset>("AuditionCast/MaleCast");
            if(!data)throw new InvalidOperationException("Male cast data missing");
            foreach(var c in JsonUtility.FromJson<CastData>(data.text).cast)
            {
                var p=content.Person(c.id);p.displayName=c.name;p.personality=c.personality;p.topics=c.topics;
                p.role=c.id=="himari"?"YOU / 28":c.id=="yuto"?"ROMANTIC LEAD / 28":"RIVAL / ADULT";
                p.portrait=MalePortrait(c.image);
            }
            content.customParticipant=true;
        }
        // 生成された人物JSONは、必ず自然文の性格説明に組み立て直してから使う。JSONそのままをセリフに出さない。
        static string ExtractPersonality(JObject c)
        {
            try
            {
                var parts=new System.Collections.Generic.List<string>();
                string text=c["personality"]?.Type==JTokenType.String?Clean((string)c["personality"]):"";
                string jobs=c["jobs"]?.Type==JTokenType.String?Clean((string)c["jobs"]):"";
                if(!string.IsNullOrWhiteSpace(jobs))parts.Add($"職業は{jobs}。");
                if(!string.IsNullOrWhiteSpace(text))parts.Add(text);
                var traitsArray=(c["traits"] as JArray)?.Values<string>().Where(x=>!string.IsNullOrWhiteSpace(x)).Take(3).Select(Clean).ToArray();
                if(traitsArray!=null&&traitsArray.Length>0)parts.Add("性格の特徴："+string.Join("・",traitsArray)+"。");
                string result=string.Join("",parts);
                return string.IsNullOrWhiteSpace(result)?"素直で、まっすぐな人間です。":result;
            }
            catch(Exception){return "素直で、まっすぐな人間です。";}
        }
        public static void Apply(ShowContent content)
        {
            if(IsMale)ApplyMaleCast(content);
            content.playerGender=Gender;
            RomanticLeadProfiles.Apply(content,IsMale);
            // English play shows every NPC name romanized (the leads already are; rivals come from the Japanese data).
            if(HundredHour.Localization.GameLanguage.Current==HundredHour.Localization.GameLocale.English)
                foreach(var member in content.cast)
                    member.displayName=member.displayName switch{"星野蓮"=>"Ren Hoshino","雨宮陽翔"=>"Haruto Amamiya","橘蒼真"=>"Soma Tachibana","雨宮ののか"=>"Nonoka Amamiya","神埼沙織"=>"Saori Kanzaki","沙織"=>"Saori Kanzaki",_=>member.displayName};
            var p=content.Person("himari");var c=Character(Read());
            // The stock participants keep Japanese names in the data; English play shows them romanized.
            if(c==null&&HundredHour.Localization.GameLanguage.Current==HundredHour.Localization.GameLocale.English)
            {
                p.displayName=IsMale?"Yuma":"Himari";
                p.topics=IsMale?new[]{"story writing","walks by the sea","games"}:new[]{"pressed flowers","fairy tales","small creatures"};
                p.personality=IsMale?"Curious and intuitive; loves building stories and games, and speaks plainly.":"A gentle dreamer who loves fairy tales and small creatures; honest and a little shy.";
            }
            if(c!=null)
            {
                content.customParticipant=true;
                p.displayName=Clean((string)c["name"]);p.role="YOU";
                p.personality=ExtractPersonality(c);
                var topics=(c["hobbies"] as JArray)?.Values<string>().Where(x=>!string.IsNullOrWhiteSpace(x)).Select(Clean).ToArray();
                if(topics!=null&&topics.Length>0)p.topics=topics;
            }
            if(File.Exists(PortraitPath))
            {
                var t=new Texture2D(2,2);if(t.LoadImage(File.ReadAllBytes(PortraitPath)))p.portrait=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f));else UnityEngine.Object.Destroy(t);
            }
        }
    }
}
