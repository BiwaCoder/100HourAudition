"""Offline conversation/card balance lab. No Unity, network, or third-party packages.
The JSON catalog is shared with Unity; parity fixtures verify card/power transitions.
"""
from __future__ import annotations
import argparse
from dataclasses import dataclass, field, asdict
import copy
import json
from pathlib import Path
import random
from collections import Counter

CATALOG = Path(__file__).resolve().parents[2] / 'Assets/Resources/ConversationBalance.json'
ARCHETYPES = ('positive', 'strategic', 'artistic')

def load(path=CATALOG):
    data = json.loads(Path(path).read_text())
    assert data['version'] == 2
    assert len({c['id'] for c in data['cards']}) == len(data['cards'])
    for archetype in ARCHETYPES:
        assert sum(c['archetype'] == archetype and c['rarity'] == 0 for c in data['cards']) >= 3
        assert sum(c['archetype'] == archetype and c['rarity'] > 0 for c in data['cards']) >= 3
    return data

@dataclass
class State:
    archetype: str = 'positive'
    chapter: int = 0
    turn: int = 0
    focus: int = 3
    agi: int = 8
    wisdom: int = 0
    warmth: int = 0
    vulnerability: int = 0
    initiative: int = 0
    shield: int = 0
    sabotage: int = 0
    lingering: int = 0
    lingeringTurns: int = 0
    intimacy: int = 0
    flatBonus: int = 0
    interrupted: bool = False
    weather: str = 'clear'
    topic: str = '未完成のゲーム'
    topicTag: str = ''
    route: str = '窓辺'
    lastCard: str = ''
    lastCardArchetype: str = ''
    traps: list = field(default_factory=list)


def playable(s, c):
    return s.focus >= c['cost'] and s.agi >= c.get('agiCost', 0) and s.wisdom >= c.get('wisdomCost', 0)


def play(s, c, b):
    if not playable(s, c):
        return False
    s.focus -= c['cost']
    s.agi -= c.get('agiCost', 0)
    s.wisdom = min(b['maxWisdom'], max(0, s.wisdom-c.get('wisdomCost', 0)+c.get('wisdom', 0)))
    s.flatBonus += c.get('power', 0)
    if c.get('combo') and c['combo'] in (s.lastCard, s.lastCardArchetype):
        s.flatBonus += c.get('comboPower', 0)
    if c.get('trigger') == 'close' and s.intimacy >= 20 or c.get('trigger') == 'open' and s.vulnerability > 0:
        s.flatBonus += c.get('triggerPower', 0)
    s.warmth = min(b['maxWarmth'], s.warmth+c.get('warmth', 0))
    s.vulnerability += c.get('vulnerability', 0)
    s.initiative = min(b['maxInitiative'], s.initiative+c.get('initiative', 0))
    s.shield += c.get('shield', 0)
    s.sabotage += c.get('sabotage', 0)
    if c.get('lingering', 0):
        s.lingering = max(s.lingering, c['lingering'])
        s.lingeringTurns = max(s.lingeringTurns, c['duration'])
    if c.get('trap'):
        s.traps = s.traps[-2:] + [dict(trigger=c['trap'], power=c['trapPower'])]
    if c.get('weather'):
        s.weather = c['weather']
    if c.get('topicTag'):
        s.topicTag = c['topicTag']
    s.lastCard, s.lastCardArchetype = c['id'], c['archetype']
    return True


def trap_ready(s, t):
    return (t['trigger'] == 'rain' and s.weather == 'rain' or
            t['trigger'] == 'plant' and (s.topicTag == 'plant' or any(x in s.topic for x in ('花', '植物'))) or
            t['trigger'] == 'place' and any(x in s.route for x in ('庭', '窓辺', '星空')) or
            t['trigger'] == 'interrupt' and s.interrupted)


def power(s, b, base, action, memory=False):
    # base includes ordinary action/tag/memory bonuses, as in Unity DeckPower.
    p = base+s.warmth+(s.lingering if s.lingeringTurns > 0 else 0)+sum(t['power'] for t in s.traps if trap_ready(s, t))
    if s.chapter == 0 and action == 'introduce':
        p += 4
    if s.chapter == 1:
        p += s.initiative//2
        counter = any(t['trigger'] == 'interrupt' and trap_ready(s, t) for t in s.traps)
        if s.interrupted and s.shield == 0 and s.initiative < 5 and not counter and action != 'yield':
            p -= b['interruptPenalty']
        if action == 'challenge' and s.initiative < 3:
            p -= 4
    if s.chapter == 2:
        p += s.intimacy//10+(3 if memory else 0)
    if s.archetype == b['preferredArchetype']:
        p = int(p*b['preferencePercent']/100)
    return max(0, min(b['maxPower'], p))


def resolve(s, action, p):
    s.intimacy = min(100, s.intimacy+p//3+(3 if action == 'honest' else 0))
    s.traps = [t for t in s.traps if not trap_ready(s, t)]
    s.lingeringTurns = max(0, s.lingeringTurns-1)
    if s.interrupted and s.shield > 0:
        s.shield -= 1
    s.initiative = max(0, s.initiative-1+(2 if action == 'lead' else 0))


def offers(rng, pool):
    result = []
    pool = list(pool)
    for _ in range(3):
        c = rng.choices(pool, weights=[max(1, c['weight']) for c in pool])[0]
        result.append(c)
        pool.remove(c)
    return result


def simulate(seed, archetype, b, policy='greedy'):
    """Run all three chapters with AI-support, cards, choices and elimination.
    Language generation is a deterministic seed phrase, so API costs stay zero.
    Policies explore card sequences; RNG is seeded Python Random (not Unity xorshift).
    """
    rng = random.Random(seed)
    s = State(archetype=archetype, agi=4)
    deck = [c for c in b['cards'] if c['archetype'] == archetype and c['rarity'] == 0]
    draw, discard = list(deck), []
    rng.shuffle(draw)
    trust, stars, stress = 10, 24000, 0
    rivals = [dict(arch=a, trust=rng.randrange(12, 20), stars=rng.randrange(20000, 30000), state=State(archetype=a)) for a in ARCHETYPES]
    rows, rewards = [], []
    for chapter in range(3):
        s.chapter = chapter
        s.warmth=s.wisdom=s.initiative=s.shield=s.vulnerability=s.lingering=s.lingeringTurns=0
        s.traps=[]
        s.route = '窓辺' if chapter < 2 else '星空'
        for turn in range(4):
            s.turn=turn;s.focus=3;s.flatBonus=0;s.sabotage=0;s.lastCard=s.lastCardArchetype=s.topicTag='';s.weather='clear';s.interrupted=chapter==1 and turn%2==1
            hand=[]
            for _ in range(3):
                if not draw:
                    draw,discard=discard,[];rng.shuffle(draw)
                if draw:hand.append(draw.pop(0))
            actions = [('introduce', 9, False), ('pickup', 9, False)] if chapter==0 else [('lead', 10, False), ('yield', 5, False), ('challenge', 12, False)] if chapter==1 else [('recall', 10, True), ('promise', 14 if s.intimacy>=20 else 8, False), ('honest', 6, False)]
            support='none'
            if s.agi>=b['forecastCost'] and turn==0:
                s.agi-=b['forecastCost'];support='forecast'
            played=[]
            while hand:
                possible=[c for c in hand if playable(s,c)]
                if not possible:break
                def value(c):
                    future=copy.deepcopy(s);play(future,c,b)
                    return max(power(future,b,base+future.flatBonus,aid,mem) for aid,base,mem in actions)+c.get('wisdom',0)+c.get('sabotage',0)
                c=max(possible,key=value) if policy=='greedy' else rng.choice(possible)
                play(s,c,b);hand.remove(c);played.append(c)
                if c.get('generate'):s.topic=c['topicSeed']
            action,base,memory=max(actions,key=lambda a:power(s,b,a[1]+s.flatBonus,a[0],a[2])) if policy=='greedy' else rng.choice(actions)
            gain=power(s,b,base+s.flatBonus,action,memory)
            trust+=gain;stars+=gain*350+1000;stress=min(100,stress+rng.randrange(5,10)-(2 if action in ('yield','honest','recall') else 0))
            resolve(s,action,gain)
            for r in rivals:
                npc=r['state']
                if npc.chapter!=chapter:
                    npc.warmth=npc.wisdom=npc.vulnerability=npc.lingeringTurns=0;npc.traps=[]
                jealous=trust-r['trust']>=b['jealousyThreshold']
                npc.chapter=chapter;npc.turn=turn;npc.focus=3;npc.flatBonus=0;npc.sabotage=0;npc.topic=s.topic;npc.topicTag='';npc.route=s.route;npc.weather=s.weather;npc.interrupted=chapter==1 and jealous
                pool=[c for c in b['cards'] if c['archetype']==r['arch'] and c['rarity']==0 and playable(npc,c)]
                c=rng.choice(pool);play(npc,c,b)
                if c.get('generate'):npc.topic=c['topicSeed']
                npc_action='challenge' if jealous else 'honest' if r['arch']=='strategic' else 'lead'
                n=max(0,power(npc,b,b['rivalBase']+rng.randrange(b['rivalVariance'])+npc.flatBonus,npc_action)-s.sabotage)
                npc.intimacy=min(100,npc.intimacy+n//3);npc.traps=[t for t in npc.traps if not trap_ready(npc,t)];npc.lingeringTurns=max(0,npc.lingeringTurns-1)
                if jealous and npc.sabotage>0:trust=max(0,trust-2);stress=min(100,stress+2)
                r['trust']+=n;r['stars']+=2000+rng.randrange(4000)
            rows.append(dict(version=b['version'],seed=seed,archetype=archetype,chapter=chapter,turn=turn,support=support,action=action,cards=[c['id'] for c in played],offered=[c['id'] for c in played+hand],power=gain,trust=trust,agi=s.agi,focus=s.focus,wisdom=s.wisdom,initiative=s.initiative,intimacy=s.intimacy,topic=s.topic))
            discard.extend(played+hand)
        player_merit=trust+stars//5000
        loser=min(rivals,key=lambda r:r['trust']+r['stars']//5000)
        if player_merit<=loser['trust']+loser['stars']//5000 or stress>=100:
            return dict(seed=seed,archetype=archetype,win=False,chapter=chapter,rewards=rewards,trust=trust),rows
        rivals.remove(loser)
        choice=offers(rng,[c for c in b['cards'] if c['archetype']==archetype and c['rarity']>0])
        reward=rng.choice(choice);rewards.append(reward['id']);deck.append(reward);draw.append(reward)
        s.agi=min(8,s.agi+2);stress=max(0,stress-12)
    return dict(seed=seed,archetype=archetype,win=True,chapter=2,rewards=rewards,trust=trust),rows


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--runs',type=int,default=1000);parser.add_argument('--seed',type=int,default=100);parser.add_argument('--catalog',type=Path,default=CATALOG);parser.add_argument('--output',type=Path,default=Path('balance-results'));parser.add_argument('--policy',choices=['random','greedy'],default='greedy')
    args=parser.parse_args();b=load(args.catalog);args.output.mkdir(parents=True,exist_ok=True);summary={}
    with (args.output/'turns.jsonl').open('w') as telemetry:
        for arch in ARCHETYPES:
            results=[];usage=Counter();appear=Counter()
            for i in range(args.runs):
                result,rows=simulate(args.seed+i,arch,b,args.policy);results.append(result)
                for row in rows:
                    telemetry.write(json.dumps(row,ensure_ascii=False)+'\n');usage.update(row['cards']);appear.update(row['offered'])
            summary[arch]=dict(runs=args.runs,winRate=sum(r['win'] for r in results)/args.runs,meanTrust=sum(r['trust'] for r in results)/args.runs,cardUses=dict(usage),cardAppearances=dict(appear))
    (args.output/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n');print(json.dumps(summary,ensure_ascii=False,indent=2))

if __name__=='__main__':main()
