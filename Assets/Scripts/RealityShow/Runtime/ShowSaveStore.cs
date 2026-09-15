using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace HundredHour.RealityShow
{
    public static class ShowSaveStore
    {
        public static string Path=>System.IO.Path.Combine(Application.persistentDataPath,"RealityShowRun-v1.json");
        public static void Save(ShowState state,string path=null)
        {
            path=path??Path;var json=JsonUtility.ToJson(state,true);File.WriteAllText(path+".tmp",json);
            if(File.Exists(path)){File.Copy(path,path+".bak",true);File.Delete(path);}File.Move(path+".tmp",path);
        }
        public static bool TryLoad(ShowContent content,out ShowState state,out string error,string path=null)
        {
            state=null;error=null;path=path??Path;if(!File.Exists(path))return false;
            try
            {
                var s=JsonUtility.FromJson<ShowState>(File.ReadAllText(path));
                if(s==null||s.version!=1||!Enum.IsDefined(typeof(ShowPhase),s.phase)||!Enum.IsDefined(typeof(ShowDifficulty),s.difficulty)||s.chapter<0||s.chapter>2||s.turn<0||s.turn>4||s.agi<0||s.agi>8||s.focus<0||s.focus>3||s.loop<0||s.loop>100000)throw new FormatException("セーブの状態が不正です");
                if(s.deck==null||s.draw==null||s.hand==null||s.played==null||s.discard==null||s.offers==null||s.actions==null||s.contestants==null||s.memories==null||s.memoryCards==null||s.log==null)throw new FormatException("セーブの項目が不足しています");
                if(s.deck.Concat(s.draw).Concat(s.hand).Concat(s.discard).Concat(s.played).Concat(s.offers).Any(id=>content.Card(id)==null&&!s.memoryCards.Any(m=>m.id==id&&s.memories.Any(f=>f.id==m.memoryId)))||s.contestants.Count!=4||s.contestants.Select(x=>x.id).Distinct().Count()!=4||s.contestants.Any(x=>content.Person(x.id)==null)||s.Player==null||content.Person(s.target)==null)throw new FormatException("セーブのカードまたは人物が見つかりません");
                state=s;return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
    }
}
