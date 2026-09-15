using System;
using System.Collections.Generic;
namespace HundredHour.RealityShow
{
    public enum ShowPhase { Title, Opening, FirstChoice, FirstLoss, Awakening, Route, Conversation, Reward, Ceremony, Eliminated, Victory, GameOver, FirstReview, Archetype }
    public enum ShowDifficulty { Easy, Normal, Hard }
    [Serializable] public sealed class ShowMemory
    {
        public string id, owner, category, detail, source;
        public int loop, used;
    }
    [Serializable] public sealed class MemoryCard
    {
        public string id,memoryId,title,owner,detail;
    }
    [Serializable] public sealed class ShowBeat
    {
        public string speaker, text; public int loop, chapter;
    }
    [Serializable] public sealed class ShowContestant
    {
        public ShowState conversation;
        public int understanding;
        public string id; public int trust, stars=20000, alliance; public bool eliminated;
        // Stars are never shown to the player in the mansion flow, so they no longer count toward selection.
        public int Merit=>trust+alliance;
    }
    [Serializable] public sealed class RivalConversationBeat
    {
        public string rival, line, reply, technique;
        public int loop, chapter, turn, timeline, trustGain, understandingGain;
    }
    [Serializable] public sealed class TalkAction
    {
        public string id, line, tag, memoryA, memoryB;
        public int basePower;
        public string goal, recallA, recallB;
    }
    [Serializable] public sealed class ShowTrap { public string trigger; public int power; }
    [Serializable] public sealed class ConversationMemory
    {
        public string id, partner, playerLine, reply, topic, meaning, effect;
        public int loop, chapter, turn, timeline;
    }
    [Serializable] public sealed class ShowState
    {
        public bool deckMechanics, communicationMechanics;
        public string firstApproach="";
        public List<string> learnedTechniques=new List<string>(),usedTechniques=new List<string>();
        // "id|line" of every fixed choice spoken this loop, so a used line never reappears in the list.
        public List<string> usedActions=new List<string>();
        // Hidden rhythm bonus bookkeeping (compliment / self-disclosure balance). Never shown in the UI.
        public bool complimentLanded; public int balanceCompliments,balanceDisclosures; public string lastBalanceKind="";
        public bool ceremonyPending; // last reply of a chapter is displayed before the selection ceremony runs
        public bool lastActionUnderstood; // previous line listened to / drew out the other person (sets up a strong approach)
        public string lastGroupWinner="",groupOpener="",lastGroupReaction=""; public int sweetStreak,groupPlayerDelta; public float groupMood; // group-talk bookkeeping
        public string techniqueNotice="";
        public List<RivalConversationBeat> rivalConversations=new List<RivalConversationBeat>();
        public List<string> comebackReviews=new List<string>();
        public int understanding;
        public string topicGoal="", recallA="", recallB="", memoryFeedback="";
        public List<ConversationMemory> conversations=new List<ConversationMemory>();
        public List<string> memorySynergies=new List<string>();
        public string archetype="",topicTag="",lastCard="",lastCardArchetype="",pendingTopic="",pendingTopicSeed="",npcScene="",epilogue="";
        public int warmth,wisdom,initiative,shield,sabotage,vulnerability,lingering,lingeringTurns,intimacy,jealousy,lastPower,rivalResentment;
        public bool interrupted,preferenceKnown;
        public List<ShowTrap> traps=new List<ShowTrap>();
        public List<string> ownedRare=new List<string>();
        public int version=1,seed; public uint randomState;
        public ShowPhase phase=ShowPhase.Title;
        public ShowDifficulty difficulty;
        public int openingPage,loop,chapter,turn,turnsSpoken,bank,agi,focus,stress,attune,purity,thorns;
        public int guard,flatBonus,starsBonus,stressBonus,topicIndex,intentPressure,forecast=-1;
        public string followup="";
        public string supportNarration="";
        public string chapterCheckpoint="";
        public int timeLeaps;
        public string target="yuto",weather="clear",route="",preferredTag="creation",topic="",lastNpc="",notice="",lastEliminated="";
        public bool introduced,greeted,crafted,combined,perspective,acquired,composedThisTurn,banked,rewardFromAbility,complimented,deepened,romanceTalked,firstPartCleared,nameKnown,impressionShared;
        public string nameTip="";
        public int relationshipStage;
        public string stageMoment="";
        public string selectedMemory="",secondMemory="";
        public List<string> deck=new List<string>(),draw=new List<string>(),hand=new List<string>(),discard=new List<string>(),played=new List<string>(),offers=new List<string>();
        public List<ShowMemory> memories=new List<ShowMemory>();
        public List<MemoryCard> memoryCards=new List<MemoryCard>();
        public List<ShowBeat> log=new List<ShowBeat>();
        public List<ShowBeat> presentedLog=new List<ShowBeat>();
        public List<ShowContestant> contestants=new List<ShowContestant>();
        public List<TalkAction> actions=new List<TalkAction>();
        public ShowContestant Player=>contestants.Find(x=>x.id=="himari");
        public int RemainingHours=>Math.Max(0,100-(chapter*4+turn)*8);
        public int LoopLimit=>difficulty==ShowDifficulty.Easy?int.MaxValue:difficulty==ShowDifficulty.Normal?5:3;
        public bool CanLoop=>phase==ShowPhase.Eliminated&&loop<LoopLimit&&(difficulty==ShowDifficulty.Easy||thorns<3);
    }
}
