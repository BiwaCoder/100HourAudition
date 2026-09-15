"""Non-destructive detail upgrade. Blender 4.2+, --background --factory-startup --python this.py."""
import bpy, math, random, json, time
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent; ROOT=P.parents[2]; OUT=ROOT/'Assets/Environments/SeasideMansion/DetailV2';OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(P.parent/'SeasideMansion.blend'))
random.seed(72)
oldprefix=('Gallery column','Palm trunk','Palm frond','Palm leaflet','Garden leaf cluster','Colorful flowers','Fountain','Tier ','Basin marble')
archive=bpy.data.collections.new('Original details - archived');bpy.context.scene.collection.children.link(archive)
for o in list(bpy.data.objects):
 if o.name.startswith(oldprefix):
  for c in list(o.users_collection):c.objects.unlink(o)
  archive.objects.link(o)
archive.hide_render=True;archive.hide_viewport=True
col=bpy.data.collections.new('Detail V2 - crafted assets');bpy.context.scene.collection.children.link(col)
lib=bpy.data.collections.new('Detail V2 - reusable masters');bpy.context.scene.collection.children.link(lib)
materials={}
def mat(n,c,metal=0,rough=.55):
 m=bpy.data.materials.new('D2 '+n);m.diffuse_color=(*c,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough
 if n in ['Limestone','Bark']:
  tex=m.node_tree.nodes.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=24 if n=='Limestone' else 9
  bump=m.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.2;bump.inputs['Distance'].default_value=.025
  m.node_tree.links.new(tex.outputs['Fac'],bump.inputs['Height']);m.node_tree.links.new(bump.outputs['Normal'],bs.inputs['Normal'])
 materials[n]=m;return m
mat('Limestone',(.77,.72,.61),rough=.42);mat('Brass',(.45,.27,.085),.75,.26);mat('Bark',(.22,.13,.065),rough=.9)
mat('Leaves',(.065,.20,.045),rough=.58);mat('Leaf tips',(.19,.31,.055),rough=.65);mat('Silver leaves',(.21,.30,.18),rough=.8)
mat('Coral petals',(.78,.055,.15),rough=.65);mat('Violet petals',(.26,.075,.39),rough=.65);mat('Flower centre',(.95,.68,.24),rough=.6)
mat('Water',(.025,.24,.28),.35,.16);mat('Cascade',(.24,.65,.67),.2,.1)
class Mesh:
 def __init__(self):self.v=[];self.f=[];self.mi=[];self.colors=[]
 def add(self,v,f,m,color=None):
  off=len(self.v);self.v.extend(v);self.f.extend([tuple(off+i for i in face) for face in f]);self.mi.extend([list(materials).index(m)]*len(f));self.colors.extend([color or (1,1,1,1)]*len(v))
 def object(self,name,collection=lib):
  me=bpy.data.meshes.new(name);me.from_pydata(self.v,[],self.f);me.update();ob=bpy.data.objects.new(name,me);collection.objects.link(ob)
  for m in materials.values():me.materials.append(m)
  for p,idx in zip(me.polygons,self.mi):p.material_index=idx;p.use_smooth=True
  colors=me.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
  for c,v in zip(colors.data,self.colors):c.color=v
  return ob

def lathe(b,profile,mat='Limestone',n=96,flutes=0,depth=0):
 v=[];f=[]
 for r,z in profile:
  for j in range(n):
   a=j*2*math.pi/n;rr=r+depth*(.5+.5*math.cos(a*flutes)) if flutes else r
   v.append((rr*math.cos(a),rr*math.sin(a),z))
 for i in range(len(profile)-1):
  for j in range(n):f.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
 b.add(v,f,mat)
def tube(b,points,r,m,sides=6):
 pts=[Vector(p) for p in points];v=[];f=[]
 for i,p in enumerate(pts):
  d=(pts[min(i+1,len(pts)-1)]-pts[max(i-1,0)]).normalized();u=d.cross(Vector((0,0,1)))
  if u.length<.01:u=Vector((1,0,0))
  u.normalize();w=d.cross(u).normalized();rr=r*(1-.65*i/max(1,len(pts)-1))
  for j in range(sides):v.append(tuple(p+rr*(math.cos(j*math.tau/sides)*u+math.sin(j*math.tau/sides)*w)))
 for i in range(len(pts)-1):
  for j in range(sides):f.append((i*sides+j,i*sides+(j+1)%sides,(i+1)*sides+(j+1)%sides,(i+1)*sides+j))
 b.add(v,f,m)
def leaf(b,start,end,width,m='Leaves',segments=5,bend=.08):
 a=Vector(start);d=Vector(end)-a;side=d.cross(Vector((0,0,1))).normalized()
 if side.length<.01:side=Vector((1,0,0))
 v=[];f=[];shade=random.uniform(.75,1.25)
 for i in range(segments+1):
  t=i/segments;c=a+d*t+Vector((0,0,bend*math.sin(math.pi*t)));w=width*math.sin(math.pi*t)**.8
  for j in [-1,0,1]:v.append(tuple(c+side*w*j+Vector((0,0,(1-abs(j))*w*.24))))
 for i in range(segments):
  for j in range(2):f.append((i*3+j,(i+1)*3+j,(i+1)*3+j+1,i*3+j+1))
 b.add(v,f,m,(shade,shade,shade,1))
def fountain():
 b=Mesh();lathe(b,[(0,.04),(3.02,.04),(3.08,.08),(3.08,.18),(3.01,.23),(2.99,.52),(3.07,.55),(3.10,.60),(3.07,.66),(2.85,.66),(2.79,.61),(2.79,.23),(.5,.23)])
 lathe(b,[(2.995,.25),(3.01,.27),(3.01,.30),(2.995,.32)],'Brass')
 # scalloped carved basin arches
 for k in range(48):
  a=k*math.tau/48;pts=[]
  for j in range(13):
   t=j/12;ang=a+(t-.5)*.104;pts.append((3.005*math.cos(ang),3.005*math.sin(ang),.27+.23*math.sin(t*math.pi)))
  tube(b,pts,.014,'Limestone',5)
 lathe(b,[(0,.455),(2.775,.455)],'Water',128)
 lathe(b,[(.58,.23),(.59,.33),(.52,.41),(.48,.45),(.40,.55),(.25,1.08),(.30,1.22),(.40,1.29)],flutes=16,depth=.022)
 for z,r in [(1.22,1.50),(2.18,.89)]:
  lathe(b,[(.22,z),(.32,z-.03),(.5*r,z+.02),(.75*r,z+.13),(.91*r,z+.29),(r,z+.42),(r,z+.47),(r-.035,z+.50),(r-.12,z+.47),(.81*r,z+.26),(.48*r,z+.17),(.22,z+.17)],n=128,flutes=24,depth=.022)
  for rib in range(24):
   angle=rib*math.tau/24
   pts=[((.5+.5*t)*r*math.cos(angle),(.5+.5*t)*r*math.sin(angle),z+.02+.40*t*t) for t in [j/12 for j in range(13)]]
   tube(b,pts,.018,'Limestone',5)
  lathe(b,[(r+.012,z+.42),(r+.018,z+.435),(r+.018,z+.46),(r+.005,z+.475)],'Brass',128)
  lathe(b,[(0,z+.435),(r-.10,z+.435)],'Water',128)
  lathe(b,[(.30,z+.47),(.29,z+.52),(.20,z+.62),(.16,z+.79),(.22,z+.94),(.28,z+.98)],n=64,flutes=16,depth=.014)
  for k in range(32):
   a=k*math.tau/32
   bottom=.46 if z<2 else 1.66
   pts=[((r+.1*t)*math.cos(a),(r+.1*t)*math.sin(a),z+.46-(z+.46-bottom)*t*t) for t in [j/16 for j in range(17)]]
   tube(b,pts,.016,'Cascade',5)
 for k in range(8):
  a=k*math.tau/8;pts=[]
  for j in range(25):
   t=j/24;r=2.55-1.2*t;pts.append((r*math.cos(a),r*math.sin(a),.46+1.2*4*t*(1-t)))
  tube(b,pts,.023,'Cascade',6)
 tube(b,[(0,0,3.16+.43*math.sin(math.pi*j/20)) for j in range(11)],.045,'Cascade',8)
 return b.object('Fountain')
def column():
 b=Mesh();lathe(b,[(0,0),(.39,0),(.4,.06),(.4,.16),(.36,.19),(.35,.24),(.30,.27),(.29,.31),(.31,.34),(.31,.38),(.28,.42),(.255,.46)],n=96)
 prof=[]
 for i in range(25):
  t=i/24;prof.append((.247-.023*t+.016*math.sin(t*math.pi),.46+3.51*t))
 lathe(b,prof,n=144,flutes=18,depth=.012)
 lathe(b,[(.245,3.97),(.285,4.01),(.30,4.04),(.30,4.10),(.27,4.14),(.27,4.20),(.30,4.24),(.36,4.38),(.40,4.43),(.40,4.48),(.43,4.51),(.43,4.58),(0,4.58)],n=96)
 for z,r in [(.32,.315),(4.10,.30),(4.46,.403)]:lathe(b,[(r,z),(r+.005,z+.008),(r+.005,z+.025),(r,z+.03)],'Brass')
 for a in [i*math.tau/4 for i in range(4)]:
  for j in range(-2,3):
   pts=[]
   for i in range(7):
    t=i/6;ang=a+j*.042*t;r=.282+.082*t;pts.append((r*math.cos(ang),r*math.sin(ang),4.20+t*(.18-.023*abs(j))))
   tube(b,pts,.01,'Brass',5)
 return b.object('Column')
def palm():
 b=Mesh();h=6
 def center(z):return Vector((.36*(z/h)**2,.12*math.sin(z/h*2),z))
 v=[];f=[];ns=20;nr=121
 for i in range(nr):
  z=h*i/(nr-1);p=center(z);r=.22-.095*z/h+.007*math.sin(z*math.pi*12)+.004*math.sin(z*51)
  for j in range(ns):
   a=j*math.tau/ns;v.append(tuple(p+Vector((r*math.cos(a),r*math.sin(a),0))))
 for i in range(nr-1):
  for j in range(ns):f.append((i*ns+j,i*ns+(j+1)%ns,(i+1)*ns+(j+1)%ns,(i+1)*ns+j))
 b.add(v,f,'Bark')
 top=center(h)
 for k in range(15):
  a=k*2.399;L=2.3+random.random()*.7;rise=.45+(k%3)*.28;drop=.7+(k%4)*.2
  axis=Vector((math.cos(a),math.sin(a),0));side=Vector((-math.sin(a),math.cos(a),0))
  def spine(t):return top+axis*(L*t)+Vector((0,0,rise*math.sin(t*math.pi)-drop*t*t))
  tube(b,[spine(i/16) for i in range(17)],.04,'Leaf tips',6)
  for j in range(1,26):
   t=j/27;ln=(.30+.58*math.sin(t*math.pi))*(1-.5*t)
   for s in [-1,1]:
    base=spine(t);tip=base+side*(s*ln)+axis*(.20+.24*t)+Vector((0,0,-.18-.25*t))
    leaf(b,base,tip,.045*(1-.55*t),'Leaf tips' if k%5==0 else 'Leaves',5,.075)
 for k in range(5):
  a=k*math.tau/5;lat=Mesh(); # compact elongated coconut made of curved profile
  start=len(b.v);temp=Mesh();lathe(temp,[(.01,-.16),(.11,-.12),(.15,0),(.12,.15),(.02,.20)],'Leaf tips',16)
  b.add([tuple(Vector(v)+top+Vector((.2*math.cos(a),.2*math.sin(a),-.1))) for v in temp.v],temp.f,'Leaf tips')
 return b.object('Palm')
def plants():
 b=Mesh()
 # three species interleaved, grouped as a reusable 4.2 x 3.3 metre bed
 for ix in range(5):
  for iy in range(4):
   origin=Vector(((ix-2)*.79+random.uniform(-.08,.08),(iy-1.5)*.79,.35));kind=(ix+iy)%3
   stems=10 if kind==1 else 13
   for k in range(stems):
    a=k*2.399+random.random()*.3;length=random.uniform(.40,.75);d=Vector((math.cos(a)*.30,math.sin(a)*.30,length));end=origin+d
    tube(b,[origin,origin+d*.5,end],.012,'Silver leaves' if kind==1 else 'Bark',5)
    for j in range(2,7):
     t=j/7;base=origin+d*t
     for side in [-1,1]:
      ang=a+side*1.1+j*.6;ln=.14 if kind==1 else .24
      tip=base+Vector((ln*math.cos(ang),ln*math.sin(ang),.05))
      leaf(b,base,tip,.014 if kind==1 else .055,'Silver leaves' if kind==1 else 'Leaves',3,.03)
    if kind==1:
     for n in range(7):
      p=end+Vector((0,0,n*.027))
      for j in range(4):
       ang=j*math.pi/2+n;leaf(b,p,p+Vector((.042*math.cos(ang),.042*math.sin(ang),.02)),.019,'Violet petals',2,.012)
    elif kind==2:
     for n in range(2):
      p=end+Vector((random.uniform(-.09,.09),random.uniform(-.09,.09),.02))
      for j in range(3):
       ang=j*math.tau/3+k;leaf(b,p,p+Vector((.13*math.cos(ang),.13*math.sin(ang),.035)),.064,'Coral petals',4,.026)
      tube(b,[p,p+Vector((0,0,.045))],.012,'Flower centre',5)
 return b.object('PlantBed')
masters=[fountain(),column(),palm(),plants()]
# Export reusable LOD meshes, preserve linked instances in editable full mansion.
stats={}
def export(objects,path):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,use_mesh_modifiers=True,add_leaf_bones=False)
for ob in masters:
 ob.data.calc_loop_triangles();stats[ob.name]={'lod0_triangles':len(ob.data.loop_triangles),'vertices':len(ob.data.vertices)}
 export([ob],OUT/(ob.name+'LOD0.fbx'))
 lod=ob.copy();lod.data=ob.data.copy();lib.objects.link(lod);lod.name=ob.name+' LOD1';d=lod.modifiers.new('Distance simplification','DECIMATE');d.ratio=.32 if ob.name!='PlantBed' else .45
 export([lod],OUT/(ob.name+'LOD1.fbx'))
 ev=lod.evaluated_get(bpy.context.evaluated_depsgraph_get());me=ev.to_mesh();me.calc_loop_triangles();stats[ob.name]['lod1_triangles']=len(me.loop_triangles);ev.to_mesh_clear();bpy.data.objects.remove(lod,do_unlink=True)
 def instance(loc,scale=1,rot=0):
  o=ob.copy();o.data=ob.data;col.objects.link(o);o.location=loc;o.scale=(scale,)*3;o.rotation_euler.z=rot
 if ob.name=='Fountain':instance((0,0,0))
 if ob.name=='Column':
  for x in [-10,-6,6,10]:
   for y in [-9,9]:instance((x,y,0))
 if ob.name=='Palm':
  for x in [-8.5,8.5]:
   for y in [-7.8,7.8]:instance((x,y,0),.75,random.random()*math.tau)
  for x in [-22,22]:
   for y in [-17,-5,8,18]:instance((x,y,0),1,random.random()*math.tau)
 if ob.name=='PlantBed':
  for x in [-6.8,6.8]:
   for y in [-5.7,5.7]:instance((x,y,0))
lib.hide_render=True;lib.hide_viewport=True
# Ocean modifier benchmark; retain editable modifier, export one moderate-density surface.
envc=bpy.data.collections['Environment - not exported']
old=bpy.data.objects.get('Ocean')
if old:old.hide_render=True;old.hide_viewport=True
me=bpy.data.meshes.new('Ocean seed');me.from_pydata([(0,0,0)],[],[]);sea=bpy.data.objects.new('Ocean modifier - authored waves',me);col.objects.link(sea);sea.location=(0,203,-1.1)
mod=sea.modifiers.new('Coastal ocean simulation','OCEAN');mod.spatial_size=320;mod.wave_scale=.62;mod.choppiness=.6;mod.wind_velocity=7;mod.wave_scale_min=.08;mod.time=1;mod.random_seed=12
bench=[]
for res in [7,9,12]:
 mod.resolution=res;mod.viewport_resolution=res;t=time.perf_counter();bpy.context.view_layer.update();ev=sea.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=ev.to_mesh();mesh.calc_loop_triangles();bench.append({'resolution':res,'triangles':len(mesh.loop_triangles),'vertices':len(mesh.vertices),'evaluate_seconds':round(time.perf_counter()-t,4)});ev.to_mesh_clear()
mod.resolution=9;mod.viewport_resolution=9;sea.data.materials.append(materials['Water'])
for p in sea.data.polygons:p.use_smooth=True
export([sea],OUT/'OceanSurface.fbx')
# Coarse far horizon in source only. Unity uses its broad existing ocean plane with matching shader.
if old:old.hide_render=False;old.hide_viewport=False;old.location.z=-1.2
stats['ocean_benchmark']=bench
(P/'mesh_budget.json').write_text(json.dumps(stats,indent=2))
(OUT/'Palette.json').write_text(json.dumps({'materials':[{'name':m.name,'color':list(m.diffuse_color),'metallic':m.node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value,'roughness':m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value} for m in materials.values()]},indent=2))
# Save source with separate masters, archival details and editable Ocean modifier.
bpy.ops.wm.save_as_mainfile(filepath=str(P/'SeasideMansion-DetailV2.blend'))
print('DETAIL_V2_COMPLETE',json.dumps(stats))
