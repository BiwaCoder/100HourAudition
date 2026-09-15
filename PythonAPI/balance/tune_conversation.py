"""Evaluate candidate balance settings; write a candidate without replacing Unity data."""
import argparse
import copy
import json
from pathlib import Path
try:
    from .conversation import load, simulate, ARCHETYPES
except ImportError:
    from conversation import load, simulate, ARCHETYPES

def main():
    p=argparse.ArgumentParser();p.add_argument('--runs',type=int,default=300);p.add_argument('--target',type=float,default=.75);p.add_argument('--output',type=Path,default=Path('balance-candidate'));args=p.parse_args()
    source=load();trials=[]
    # Search pressure, support budget, and the strategic synergy's numeric strength.
    for pressure in (9,11,13):
        for disclosure in (3,5,7):
            candidate=copy.deepcopy(source);candidate['rivalBase']=pressure
            next(c for c in candidate['cards'] if c['id']=='reveal')['power']=disclosure
            rates={a:sum(simulate(9000+i,a,candidate,'random')[0]['win'] for i in range(args.runs))/args.runs for a in ARCHETYPES}
            score=sum(abs(r-args.target) for r in rates.values())+max(rates.values())-min(rates.values())
            trials.append((score,rates,candidate))
    trials.sort(key=lambda t:t[0]);best=trials[0];args.output.mkdir(parents=True,exist_ok=True)
    # Keep descriptions faithful to candidate values.
    c=next(c for c in best[2]['cards'] if c['id']=='reveal');c['description']=f"好感度+{c['power']}。自己開示+1。知恵+1。"
    (args.output/'ConversationBalance.candidate.json').write_text(json.dumps(best[2],ensure_ascii=False,indent=2)+'\n')
    report=[dict(score=t[0],winRates=t[1],rivalBase=t[2]['rivalBase'],revealPower=next(c for c in t[2]['cards'] if c['id']=='reveal')['power']) for t in trials]
    (args.output/'search.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n');print(json.dumps(report[0],ensure_ascii=False))
if __name__=='__main__':main()
