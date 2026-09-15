import bpy,json
from mathutils import Vector
from pathlib import Path
sc=bpy.context.scene; dg=bpy.context.evaluated_depsgraph_get()
# Straight segments around fountain, through front opening and each room portal.
routes=[('entrance',(0,-23),(0,-6)),('around fountain A',(0,-6),(3.8,-6)),('around fountain B',(3.8,-6),(3.8,8)),('north lounge',(0,8),(0,17))]
for s in [-1,1]:
 routes.append(('gallery '+str(s),(11.5*s,-8),(11.5*s,8)))
 for y in [-6,0,6]: routes.append(('room '+str((s,y)),(11.5*s,y),(15*s,y)))
checks=[]
for n,a,b in routes:
 for z in [.4,1,1.7]:
  origin=Vector((*a,z)); delta=Vector((*b,z))-origin
  hit,loc,norm,face,obj,matrix=sc.ray_cast(dg,origin,delta.normalized(),distance=delta.length)
  checks.append({'route':n,'height':z,'clear':not hit,'obstacle':obj.name if hit else None})
report={'all_routes_clear':all(c['clear'] for c in checks),'checks':checks}
Path(bpy.data.filepath).with_name('route_check.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
