import copy
from dataclasses import fields, asdict
import json
from pathlib import Path
import sys
import unittest
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from balance.conversation import State, load, play, power, resolve, simulate, offers
import random

class ConversationBalanceTests(unittest.TestCase):
    def test_unity_transition_parity(self):
        fixtures=Path(__file__).resolve().parents[2]/'Docs/ConversationDeck/parity-fixtures.json'
        self.assertTrue(fixtures.exists(), 'Run VerifyDeckRules.cs in Unity first')
        b=load();cards={c['id']:c for c in b['cards']};names={f.name for f in fields(State)}
        for fixture in json.loads(fixtures.read_text()):
            with self.subTest(card=fixture['card'],chapter=fixture['before']['chapter'],arch=fixture['before']['archetype']):
                state=State(**{k:v for k,v in fixture['before'].items() if k in names})
                self.assertTrue(play(state,cards[fixture['card']],b))
                for k,v in asdict(state).items():self.assertEqual(v,fixture['after'][k],k)
                self.assertEqual(power(state,b,10+state.flatBonus,fixture['action'],fixture['memory']),fixture['power'])

    def test_insufficient_resources_are_atomic(self):
        b=load();c=next(c for c in b['cards'] if c['id']=='checkmate');s=State(wisdom=0);before=copy.deepcopy(s)
        self.assertFalse(play(s,c,b));self.assertEqual(s,before)

    def test_trap_is_one_shot_and_lingers_expire(self):
        b=load();s=State(weather='rain',traps=[dict(trigger='rain',power=10)],lingering=2,lingeringTurns=1)
        self.assertEqual(power(s,b,5,'pickup'),17);resolve(s,'pickup',17)
        self.assertEqual(power(s,b,5,'pickup'),5)

    def test_rare_rewards_and_reproducible_full_runs(self):
        b=load()
        for arch in ('positive','strategic','artistic'):
            pool=[c for c in b['cards'] if c['archetype']==arch and c['rarity']>0]
            draft=offers(random.Random(7),pool)
            self.assertEqual(len({c['id'] for c in draft}),3)
            self.assertEqual(simulate(11,arch,b),simulate(11,arch,b))
            result,rows=simulate(11,arch,b)
            self.assertLessEqual(len(rows),12)
            self.assertTrue(all(r['agi']>=0 and r['focus']>=0 for r in rows))

if __name__=='__main__':unittest.main()
