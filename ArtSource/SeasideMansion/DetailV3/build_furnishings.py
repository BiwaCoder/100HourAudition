"""Blender 4.2. Non-destructive V3 furniture, lantern and architectural joinery authoring."""
import bpy, math, json, random
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent; ROOT=P.parents[2]; OUT=ROOT/'Assets/Environments/SeasideMansion/DetailV3';OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(P.parent/'DetailV2/SeasideMansion-DetailV2.blend'))
lib=bpy.data.collections.new('Detail V3 - reusable masters');bpy.context.scene.collection.children.link(lib)
col=bpy.data.collections.new('Detail V3 - furnishings and joinery');bpy.context.scene.collection.children.link(col)
archive=bpy.data.collections.new('Original furniture - archived');bpy.context.scene.collection.children.link(archive)
for o in list(bpy.data.objects):
 if o.name.startswith(('Lounge sofa','Teal cushion','Table pedestal','Round walnut table','Garden lantern','Dining tabletop')):
  for c in list(o.users_collection):c.objects.unlink(o)
  archive.objects.link(o)
archive.hide_render=True;archive.hide_viewport=True
mats={}; parts=[]
def mat(n,c,metal=0,rough=.5,kind=0,emit=0):
 m=bpy.data.materials.new('D3 '+n);m.diffuse_color=(*c,1);m.use_nodes=True;bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough
 if emit:bs.inputs['Emission Color'].default_value=(*c,1);bs.inputs['Emission Strength'].default_value=emit
 if kind:
  tex=m.node_tree.nodes.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=180 if kind==1 else 14;tex.inputs['Detail'].default_value=3
  coord=m.node_tree.nodes.new('ShaderNodeTexCoord');mapping=m.node_tree.nodes.new('ShaderNodeVectorMath');mapping.operation='MULTIPLY';mapping.inputs[1].default_value=(1,18,2) if kind==2 else (1,1,1)
  m.node_tree.links.new(coord.outputs['Generated'],mapping.inputs[0]);m.node_tree.links.new(mapping.outputs[0],tex.inputs['Vector'])
  ramp=m.node_tree.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=(*(v*.62 for v in c),1);ramp.color_ramp.elements[1].color=(*(min(v*1.16,1) for v in c),1);m.node_tree.links.new(tex.outputs['Fac'],ramp.inputs[0]);m.node_tree.links.new(ramp.outputs[0],bs.inputs['Base Color'])
  bump=m.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.18;bump.inputs['Distance'].default_value=.002 if kind==1 else .006;m.node_tree.links.new(tex.outputs['Fac'],bump.inputs['Height']);m.node_tree.links.new(bump.outputs[0],bs.inputs['Normal'])
 mats[n]=(m,kind,emit);return m
mat('Linen',(.73,.68,.56),rough=.85,kind=1);mat('Linen welt',(.57,.51,.39),rough=.85,kind=1);mat('Teal velvet',(.024,.14,.15),rough=.77,kind=1);mat('Teal welt',(.016,.075,.082),rough=.7)
mat('Walnut',(.22,.105,.044),rough=.32,kind=2);mat('Bronze',(.23,.135,.055),.78,.32);mat('Brass',(.53,.33,.12),.8,.27);mat('Opal glass',(1,.69,.32),rough=.28,kind=4,emit=2.3)
mat('Limestone',(.72,.68,.57),rough=.62,kind=3);mat('Plaster',(.82,.79,.7),rough=.83,kind=3);mat('Shadow joint',(.19,.17,.13),rough=.85)
def mesh(n,v,f,m,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);lib.objects.link(o);me.materials.append(mats[m][0]);parts.append(o)
 for p in me.polygons:p.use_smooth=smooth
 return o

def lathe(n,profile,m,segments=96,flutes=0,depth=0):
 v=[];f=[]
 for r,z in profile:
  for j in range(segments):
   a=j*math.tau/segments;rr=r+depth*(.5+.5*math.cos(a*flutes)) if flutes else r;v.append((rr*math.cos(a),rr*math.sin(a),z))
 for i in range(len(profile)-1):
  for j in range(segments):f.append((i*segments+j,i*segments+(j+1)%segments,(i+1)*segments+(j+1)%segments,(i+1)*segments+j))
 return mesh(n,v,f,m)
def pipe(n,pts,r,m,sides=6):
 v=[];f=[]
 for i,p in enumerate(pts):
  p=Vector(p);d=(Vector(pts[min(i+1,len(pts)-1)])-Vector(pts[max(0,i-1)])).normalized();u=d.cross(Vector((0,0,1)))
  if u.length<.001:u=Vector((1,0,0))
  u.normalize();w=d.cross(u)
  for j in range(sides):v.append(tuple(p+r*(u*math.cos(j*math.tau/sides)+w*math.sin(j*math.tau/sides))))
 for i in range(len(pts)-1):
  for j in range(sides):f.append((i*sides+j,i*sides+(j+1)%sides,(i+1)*sides+(j+1)%sides,(i+1)*sides+j))
 return mesh(n,v,f,m)
def box(n,loc,size,m,bevel=.02):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=n
 for c in list(o.users_collection):c.objects.unlink(o)
 lib.objects.link(o);o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mats[m][0]);parts.append(o)
 if bevel:
  b=o.modifiers.new('Rounded joinery edges','BEVEL');b.width=bevel;b.segments=3;bpy.ops.object.modifier_apply(modifier=b.name)
  b=o.modifiers.new('Weighted face normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=b.name)
 return o

def cushion(n,loc,size,m,welt,throw=False):
 # Two curved cloth panels sewn around a square perimeter. Crown, pinched corners and seam-tension wrinkles.
 v=[];f=[];N=28 if throw else 18
 for side in [-1,1]:
  for j in range(N+1):
   t=-1+2*j/N
   for i in range(N+1):
    s=-1+2*i/N;shape=max(0,(1-s*s)*(1-t*t));crown=shape**.38
    wrinkle=.013*math.sin(42*s+8*t)*math.exp(-((abs(t)-.84)/.15)**2)*shape**.4 if throw else .002*math.sin(23*s)*shape
    x=s*size[0]/2*(1+.028*(1-t*t));y=t*size[1]/2*(1+.028*(1-s*s));z=side*(size[2]*((.025+.475*crown) if throw else (.13+.37*crown))+wrinkle)
    v.append((x,y,z))
 for side in range(2):
  off=side*(N+1)**2
  for j in range(N):
   for i in range(N):
    q=off+j*(N+1)+i;face=(q,q+1,q+N+2,q+N+1);f.append(face if side else face[::-1])
 edge=list(range(N+1))+[j*(N+1)+N for j in range(1,N+1)]+[N*(N+1)+i for i in range(N-1,-1,-1)]+[j*(N+1) for j in range(N-1,0,-1)]
 for a,b in zip(edge,edge[1:]+edge[:1]):f.append((a,b,b+(N+1)**2,a+(N+1)**2))
 o=mesh(n,v,f,m);o.location=loc
 pts=[(v[i][0]*1.002,v[i][1]*1.002,0) for i in edge];pts.append(pts[0]);seam=pipe(n+' tailored piping',pts,.0045,welt,6);seam.location=loc
 return [o,seam]
def finish(n):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=n;bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');parts=[];return o
# Lantern: fluted metal spindle, assembled hexagonal cage and individual ribbed opal panels.
lathe('Stepped cast plinth',[(0,0),(.16,0),(.16,.045),(.15,.055),(.15,.08),(.125,.1),(.11,.14),(.10,.18)],'Bronze')
lathe('Fluted spindle',[(.092,.18),(.085,.24),(.068,.62),(.08,.66)],'Bronze',96,16,.006)
for z,r in [(.17,.103),(.2,.096),(.65,.083),(.69,.087)]:lathe('Turned collar',[(r,z),(r+.005,z+.006),(r+.005,z+.018),(r,z+.024)],'Brass')
lathe('Solid collar neck',[(.085,.65),(.085,.75)],'Bronze')
lathe('Lower bowl',[(.07,.66),(.073,.72),(.105,.75),(.17,.79),(.18,.81),(.18,.835),(.16,.85)],'Bronze',6)
for k in range(6):
 a=k*math.tau/6; b=(k+1)*math.tau/6
 pipe('Cage stile',[(.171*math.cos(a),.171*math.sin(a),.82),(.171*math.cos(a),.171*math.sin(a),1.16)],.012,'Brass',8)
 # Glass fine vertical reeding, physical geometry catches highlights
 v=[];f=[]
 for z in [.84,1.15]:
  for j in range(25):
   t=j/24;rr=.006*(.5+.5*math.cos(j*math.pi));x=.157*((1-t)*math.cos(a)+t*math.cos(b))+rr*math.cos(a+math.pi/6);y=.157*((1-t)*math.sin(a)+t*math.sin(b))+rr*math.sin(a+math.pi/6);v.append((x,y,z))
 for j in range(24):f.append((j,j+1,j+26,j+25))
 mesh('Reeded opal panel',v,f,'Opal glass')
lathe('Layered weather cap',[(.16,1.15),(.185,1.15),(.19,1.17),(.18,1.19),(.14,1.20),(.13,1.22),(.11,1.22),(.09,1.25),(.08,1.25),(.045,1.28),(.035,1.3)],'Bronze',6)
lathe('Finial',[(.026,1.28),(.034,1.31),(.025,1.34),(0,1.37)],'Brass',8)
lantern=finish('Lantern')
# Tailored three-seat sofa. A continuous lower frame and individually crowned cushions.
box('Recessed walnut frame',(0,0,.22),(2.55,.91,.12),'Walnut',.055)
box('Upholstered apron',(0,0,.34),(2.52,.9,.2),'Linen',.065)
for x in [-1.14,1.14]:
 for y in [-.32,.32]:
  o=lathe('Tapered brass foot',[(0,.015),(.046,.015),(.038,.12),(.068,.22),(0,.22)],'Brass',32);o.location=(x,y,0)
box('Continuous upholstered rear',(0,.36,.74),(2.55,.22,.91),'Linen',.09)
for x in [-.78,0,.78]:
 cushion('Crowned seat',(x,-.06,.53),(.755,.75,.23),'Linen','Linen welt')
 os=cushion('Angled back',(x,.255,.91),(.76,.67,.18),'Linen','Linen welt')
 for o in os:o.rotation_euler.x=math.radians(78)
# sculpted arm profile slopes up toward the rear; swept rounded section
for side in [-1,1]:
 v=[];f=[];N=24;S=16
 for i in range(N+1):
  t=i/N;y=-.46+.94*t;top=.78+.30*t*t;bottom=.29;center=(top+bottom)/2;h=(top-bottom)/2
  for j in range(S):
   a=j*math.tau/S;v.append((side*(1.20+.095*math.copysign(abs(math.cos(a))**.4,math.cos(a))),y,center+h*math.copysign(abs(math.sin(a))**.35,math.sin(a))))
 for i in range(N):
  for j in range(S):f.append((i*S+j,i*S+(j+1)%S,(i+1)*S+(j+1)%S,(i+1)*S+j))
 f.append(tuple(range(S-1,-1,-1)));f.append(tuple(N*S+j for j in range(S)));mesh('Sculpted curved arm',v,f,'Linen')
 pipe('Arm outer welt',[(side*1.265,-.44+.9*t,.77+.30*t*t) for t in [i/24 for i in range(25)]],.005,'Linen welt')
sofa=finish('Sofa')
os=cushion('Square throw pillow',(0,0,0),(.49,.49,.17),'Teal velvet','Teal welt',True)
pillow=finish('ThrowPillow')
# Turned walnut coffee table, solid bullnose top and separate fluted tulip pedestal.
lathe('Wide pedestal foot',[(0,.02),(.33,.02),(.35,.045),(.35,.07),(.33,.095),(.29,.12),(.235,.14),(.20,.17)],'Walnut')
lathe('Brass foot ring',[(.351,.035),(.355,.04),(.355,.065),(.351,.075)],'Brass')
lathe('Fluted concave stem',[(.20,.14),(.175,.2),(.137,.3),(.128,.4),(.14,.5),(.18,.59),(.25,.65)],'Walnut',144,24,.012)
lathe('Layered apron',[(.23,.64),(.70,.64),(.725,.66),(.725,.68),(.76,.685)],'Walnut')
lathe('Bullnose edge and radial top',[(0,.683),(.765,.683),(.788,.69),(.80,.706),(.80,.724),(.79,.737),(.77,.744),(0,.744)],'Walnut',144)
lathe('Inlaid brass circle',[(.714,.745),(.708,.745)],'Brass',144)
table=finish('Table')
# Small bevelled wall dressing panels and true layered reveals, grouped into spatial modules.
# Each wall retains its original collider; these are nonstructural surface dressings.
architect=[]
for src in list(bpy.data.collections['Architecture'].objects):
 if not src.name.startswith(('Courtyard fluted pier','Room portal jamb','Room divider','Entry facade','Exterior plinth','Exterior head','Sea window sill','Sea window head','Courtyard lintel','Gallery header','Room portal lintel')):continue
 loc=src.location.copy();sz=src.dimensions.copy();vertical=sz.z>2 and min(sz.x,sz.y)<1
 if src.name.startswith('Courtyard fluted pier'):
  for z,s,h in [(.12,1.22,.24),(.30,1.12,.08),(3.78,1.08,.07),(3.90,1.16,.15),(4.04,1.25,.10)]:box('Pier moulded collar',(loc.x,loc.y,z),(sz.x*s,sz.y*s,h),'Limestone',.018)
  for side in [-1,1]:
   box('Pier inset stone panel',(loc.x+side*(sz.x/2+.012),loc.y,2.04),(.024,.46,3.30),'Limestone',.012)
  for z in [.68,1.3,1.92,2.54,3.16]:
   for side in [-1,1]:box('Pier horizontal dressed joint',(loc.x+side*(sz.x/2+.026),loc.y,z),(.006,.455,.009),'Shadow joint',.002)
 elif vertical:
  alongX=sz.x>sz.y;length=sz.x if alongX else sz.y;thickness=sz.y if alongX else sz.x
  for side in [-1,1]:
   for z,h,extra,ma in [(.19,.32,.065,'Limestone'),(.39,.045,.07,'Limestone'),(3.94,.075,.055,'Limestone'),(4.02,.045,.075,'Limestone')]:
    pos=(loc.x,loc.y+side*(thickness/2+extra/2),z) if alongX else (loc.x+side*(thickness/2+extra/2),loc.y,z)
    size=(length,extra,h) if alongX else (extra,length,h);box('Skirting and cornice',pos,size,ma,.01)
   # Limestone lower courses in discrete slabs: recessed dark joints are real gaps.
   count=max(1,math.ceil(length/1.2))
   for i in range(count):
    offset=-length/2+(i+.5)*length/count
    pos=(loc.x+offset,loc.y+side*(thickness/2+.017),.75) if alongX else (loc.x+side*(thickness/2+.017),loc.y+offset,.75)
    size=(length/count-.012,.034,.63) if alongX else (.034,length/count-.012,.63);box('Honed limestone wainscot',pos,size,'Limestone',.008)
  # Finished jamb returns on ends, demonstrate wall thickness without narrowing existing portals.
  for end in [-1,1]:
   pos=(loc.x+end*(length/2-.045),loc.y,2.24) if alongX else (loc.x,loc.y+end*(length/2-.045),2.24)
   size=(.09,thickness+.12,3.35) if alongX else (thickness+.12,.09,3.35);box('Layered doorway reveal',pos,size,'Limestone',.016)
 else:
  # Headers and sills receive profiled overhangs, not flat painted bands.
  for z,expand,h in [(loc.z+sz.z/2+.022,.10,.065),(loc.z-sz.z/2-.018,.065,.045)]:box('Weathered sill and lintel lip',(loc.x,loc.y,z),(sz.x+expand,sz.y+expand,h),'Limestone',.015)
 if parts:architect.append(finish('Joinery '+src.name))
masters=[lantern,sofa,pillow,table]
def export(ob,path):
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,use_mesh_modifiers=True,add_leaf_bones=False)
placements=[];stats={}
def place(ob,loc,rot=0,scale=(1,1,1)):
 o=ob.copy();col.objects.link(o);o.location=loc;o.rotation_euler=(0,0,math.radians(rot));o.scale=scale
 placements.append({'asset':ob.name,'position':list(loc),'angle':rot,'scale':list(scale)})
for ob in masters:
 if ob.name in ['Lantern','Table']:
  bpy.context.view_layer.objects.active=ob;edge=ob.modifiers.new('Crisp machined transitions','EDGE_SPLIT');edge.split_angle=math.radians(35);bpy.ops.object.modifier_apply(modifier=edge.name)
 ob.data.calc_loop_triangles();stats[ob.name]=len(ob.data.loop_triangles);export(ob,OUT/(ob.name+'LOD0.fbx'))
 lod=ob.copy();lod.data=ob.data.copy();lib.objects.link(lod);d=lod.modifiers.new('Distance LOD','DECIMATE');d.ratio=.38;export(lod,OUT/(ob.name+'LOD1.fbx'));bpy.data.objects.remove(lod,do_unlink=True)
for x in [-4.5,4.5]:
 for y in [-7,0,7]:place(lantern,(x,y,0))
def seating(x,y,a=0):
 place(sofa,(x,y,0),a)
 for side in [-1,1]:
  dx=side*.86;dy=-.02;r=math.radians(a);o=pillow.copy();col.objects.link(o);o.location=(x+dx*math.cos(r)-dy*math.sin(r),y+dx*math.sin(r)+dy*math.cos(r),.87);o.rotation_euler=(math.radians(76),math.radians(side*9),r+math.radians(side*8))
  placements.append({'asset':'ThrowPillow','position':list(o.location),'rotation':list(o.rotation_euler),'angle':a,'scale':[1,1,1]})
for x in [-16.5,16.5]:
 for y in [-6,0]:seating(x,y+1.3);place(table,(x,y-.2,0))
for x in [-5,5]:
 seating(x-2,14.8,90);seating(x+2,14.8,-90);place(table,(x,14.5,0),scale=(1.25,1.25,1))
for x in [-7,7]:seating(x,-15);place(table,(x,-16.5,0),scale=(.75,.75,1))
# Dining tops retain their rectangular footprint and legs, add bullnose edge and breadboard joinery.
for x in [-15,15]:
 box('Dining walnut top',(x,14,.85),(2.2,4,.14),'Walnut',.05)
 for y in [12.12,15.88]:box('Dining breadboard end',(x,y,.854),(2.17,.16,.14),'Walnut',.018)
 for sx in [-1,1]:box('Dining brass inset',(x+sx*1.035,14,.923),(.006,3.73,.004),'Brass',.002)
 architect.append(finish('Joinery Dining '+str(x)))
for i,ob in enumerate(architect):
 name='Architecture%03d'%i;ob.name=name;export(ob,OUT/(name+'.fbx'));o=ob.copy();col.objects.link(o)
lib.hide_render=True;lib.hide_viewport=True
(OUT/'Layout.json').write_text(json.dumps({'placements':placements,'architecture':[o.name for o in architect]},indent=2))
(OUT/'Palette.json').write_text(json.dumps({'materials':[{'name':m.name,'color':list(m.diffuse_color),'metallic':m.node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value,'roughness':m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value,'kind':k,'emission':e} for m,k,e in mats.values()]},indent=2))
(P/'mesh_budget.json').write_text(json.dumps(stats,indent=2));bpy.ops.wm.save_as_mainfile(filepath=str(P/'SeasideMansion-DetailV3.blend'));print('V3_COMPLETE',stats)
