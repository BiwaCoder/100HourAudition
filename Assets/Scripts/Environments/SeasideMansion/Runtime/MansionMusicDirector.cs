using System.Collections;
using UnityEngine;
namespace HundredHour.Environments
{
    // Crossfades in the mansion's ambient music. The clip loads from Resources so no Inspector
    // wiring is needed; PlayMain() is the only track -- the game no longer swaps music on a
    // time-leap.
    public sealed class MansionMusicDirector : MonoBehaviour
    {
        AudioSource a,b;
        // Resources.Load<AudioClip> on this multi-MB mp3 was a synchronous, blocking decode
        // right as the scene starts -- the "long load entering the game" complaint. LoadAsync
        // kicks the decode to a background thread; PlayMain just waits on the request instead
        // of the clip, so nothing here needs to change from the caller's side.
        ResourceRequest mainRequest;
        [SerializeField] float fadeSeconds=1.5f;
        [SerializeField,Range(0,1)] float volume=.55f;
        Coroutine routine,duckRoutine;float duck=1f;
        // Lowers the music under the voice finale and brings it back for the credits.
        public void Duck(float factor,float seconds=1.2f){if(duckRoutine!=null)StopCoroutine(duckRoutine);duckRoutine=StartCoroutine(DuckTo(Mathf.Clamp01(factor),seconds));}
        public void Restore(float seconds=1.5f)=>Duck(1f,seconds);
        IEnumerator DuckTo(float target,float seconds)
        {
            float from=duck;
            for(float t=0;t<seconds;t+=Time.unscaledDeltaTime){duck=Mathf.Lerp(from,target,Mathf.Clamp01(t/seconds));ApplyDuck();yield return null;}
            duck=target;ApplyDuck();duckRoutine=null;
        }
        void ApplyDuck(){foreach(var s in new[]{a,b})if(s&&s.isPlaying)s.volume=volume*duck;}
        void Awake()
        {
            mainRequest=Resources.LoadAsync<AudioClip>("Audio/Music/main");
            a=gameObject.AddComponent<AudioSource>();b=gameObject.AddComponent<AudioSource>();
            foreach(var s in new[]{a,b}){s.loop=true;s.playOnAwake=false;s.volume=0;s.spatialBlend=0;}
        }
        public void PlayMain()
        {
            if(routine!=null)StopCoroutine(routine);
            routine=StartCoroutine(PlayWhenLoaded(mainRequest));
        }
        IEnumerator PlayWhenLoaded(ResourceRequest request)
        {
            yield return request;
            var clip=request.asset as AudioClip;
            if(!clip||(a.isPlaying&&a.clip==clip)||(b.isPlaying&&b.clip==clip))yield break;
            yield return CrossfadeTo(clip);
        }
        IEnumerator CrossfadeTo(AudioClip clip)
        {
            if(!clip)yield break;
            AudioSource from=a.isPlaying?a:(b.isPlaying?b:null);
            AudioSource to=from==a?b:a;
            to.clip=clip;to.volume=0;to.Play();
            float t=0;float fromStart=from?from.volume:0;
            while(t<fadeSeconds)
            {
                t+=Time.deltaTime;float k=Mathf.Clamp01(t/fadeSeconds);
                to.volume=Mathf.Lerp(0,volume,k);
                if(from)from.volume=Mathf.Lerp(fromStart,0,k);
                yield return null;
            }
            to.volume=volume*duck;
            if(from){from.Stop();from.volume=0;}
        }
    }
}
