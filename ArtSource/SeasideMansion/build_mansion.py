"""Run with Blender 4.2+: blender --background --python build_mansion.py.
Metres; +Y faces ocean. Standalone source; does not modify Unity scenes.
"""
import bpy, math, random, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent
random.seed(100)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):
 if c.name != 'Collection': bpy.data.collections.remove(c)
base=bpy.data.collections.get('Collection'); base.name='Architecture'
def collection(n):
 c=bpy.data.collections.new(n); bpy.context.scene.collection.children.link(c); return c
roof=collection('Roof - hide to edit interiors'); garden=collection('Garden and fountain'); furniture=collection('Interior furniture'); env=collection('Environment - not exported'); lights=collection('Lighting and cameras')
active=base

def mat(n,col,metal=0,rough=.5,alpha=1,emit=0):
 m=bpy.data.materials.new(n); m.diffuse_color=(*col,alpha); m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*col,alpha); p.inputs['Metallic'].default_value=metal; p.inputs['Roughness'].default_value=rough
 if emit: p.inputs['Emission Color'].default_value=(*col,1); p.inputs['Emission Strength'].default_value=emit
 if alpha<1: p.inputs['Alpha'].default_value=alpha; m.surface_render_method='DITHERED'
 return m
ivory=mat('Warm ivory stucco',(.88,.85,.76),rough=.68); stone=mat('Pearl travertine',(.68,.65,.56),rough=.38); white=mat('White marble trim',(.94,.91,.83),rough=.28)
gold=mat('Brushed champagne brass',(.52,.31,.10),.72,.28); dark=mat('Bronze window frames',(.12,.10,.07),.7,.3); glass=mat('Ocean glazing',(.48,.77,.82),.1,.12,.16)
water=mat('Turquoise fountain water',(.025,.39,.40),.4,.17); red=mat('Audition red velvet carpet',(.43,.018,.045),rough=.9); soil=mat('Rich planting soil',(.09,.065,.035)); green=mat('Emerald foliage',(.065,.20,.075),rough=.9); leaf=mat('Palm foliage',(.16,.30,.095)); bark=mat('Palm trunk',(.23,.15,.07)); pink=mat('Bougainvillea coral',(.85,.035,.24)); purple=mat('Salvia violet',(.34,.055,.52)); orange=mat('Golden flowers',(.98,.38,.035)); cream=mat('Linen upholstery',(.73,.64,.48)); teal=mat('Deep teal velvet',(.025,.18,.19)); wood=mat('Walnut',(.19,.085,.038)); glow=mat('Warm lantern', (1,.55,.19),emit=4)

def move(o,n,m):
 o.name=n
 for c in list(o.users_collection): c.objects.unlink(o)
 active.objects.link(o)
 if m: o.data.materials.append(m)
 return o

def cube(n,loc,size,m,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=move(bpy.context.object,n,m); o.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Soft architectural edges','BEVEL'); mod.width=bevel; mod.segments=2
  o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o

def cyl(n,loc,r,depth,m,vertices=32):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=loc); return move(bpy.context.object,n,m)
def sphere(n,loc,scale,m):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=loc); o=move(bpy.context.object,n,m); o.scale=scale; return o

def tube(n,pts,r,m):
 cu=bpy.data.curves.new(n,'CURVE'); cu.dimensions='3D'; cu.resolution_u=8; cu.bevel_depth=r; cu.bevel_resolution=2
 sp=cu.splines.new('POLY'); sp.points.add(len(pts)-1)
 for p,co in zip(sp.points,pts): p.co=(*co,1)
 o=bpy.data.objects.new(n,cu); active.objects.link(o); o.data.materials.append(m); return o

def ring(n,loc,major,minor,m):
 bpy.ops.mesh.primitive_torus_add(major_radius=major,minor_radius=minor,major_segments=64,minor_segments=8,location=loc); return move(bpy.context.object,n,m)
def wall_x(n,x,y1,y2,z=2.25,h=4.5): return cube(n,(x,(y1+y2)/2,z),(.25,y2-y1,h),ivory)
def wall_y(n,y,x1,x2,z=2.25,h=4.5): return cube(n,((x1+x2)/2,y,z),(x2-x1,.25,h),ivory)
# Foundation: no steps on the walking route.
cube('Continuous ground slab',(0,0,-.22),(44,42,.44),stone)
for x in [-15,15]: cube('Wing floor',(x,0,.02),(10,38,.08),white)
for y in [-14,14]: cube('Hall floor',(0,y,.02),(20,10,.08),white)
# Inner perimeter: repeating open 2.8m doorways into the circulation corridor.
for x in [-10,10]:
 for y in [-9,-3,3,9]:
  cube('Courtyard fluted pier',(x,y,2.35),(.65,.7,4.7),ivory)
  for dy in [-.22,0,.22]: cube('Pier gold inlay',(x+(-.34 if x<0 else .34),y+dy,2.5),(.025,.045,3.5),gold,.006)
 cube('Courtyard lintel',(x,0,4.15),(.35,18,.7),ivory)
 # Outer window wall: short plinth, head and regular mullions, continuous sea light.
 wall_x('Exterior plinth',2*x,-19,19,.38,.76); wall_x('Exterior head',2*x,-19,19,4.12,.76)
 for y in range(-18,19,3): cube('Outer mullion',(2*x,y,2.3),(.22,.12,3.2),dark)
 cube('Exterior glazed wall',(2*x,0,2.3),(.035,38,3.05),glass,0)
 # Room-side corridor wall with real open portals every 6m.
 for yc in [-6,0,6]:
  wall_x('Room portal jamb',x*1.3,yc-3,yc-1.2)
  wall_x('Room portal jamb',x*1.3,yc+1.2,yc+3)
  wall_x('Room portal lintel',x*1.3,yc-1.2,yc+1.2,3.8,1.4)
 for y in [-9,-3,3,9]: wall_y('Room divider',y,min(x*1.3,2*x),max(x*1.3,2*x))
# Front and rear courtyard colonnades / ocean lounge glazing.
for y in [-9,9]:
 for x in [-10,-6,6,10]: cube('Gallery column',(x,y,2.3),(.55,.55,4.6),ivory)
 cube('Gallery header',(0,y,4.15),(20,.35,.7),ivory)
# Front wall with an unobstructed 4m wide entrance at x=0.
wall_y('Entry facade left',-19,-20,-2); wall_y('Entry facade right',-19,2,20); wall_y('Entry portal lintel',-19,-2,2,4.25,1.5)
for x in [-2.4,2.4]:
 for i in range(3): cube('Stepped entrance pylon',(x+math.copysign(i*.2,x),-19.25,2.7-i*.15),(.32,.75,5.4-i*.3),white)
 for dx in [-.07,.07]: cube('Entry gold flute',(x+dx,-19.66,2.8),(.045,.04,4.4),gold)
cube('Entrance crown',(0,-19.2,5.2),(5.6,.65,.38),white)
# Sea-facing rear facade: giant transparent windows.
wall_y('Sea window sill',19,-20,20,.28,.56); wall_y('Sea window head',19,-20,20,4.18,.64)
for x in range(-20,21,4): cube('Sea glazing mullion',(x,19,2.35),(.1,.16,3.7),dark)
cube('Panoramic ocean glass',(0,19,2.35),(40,.03,3.6),glass,0)
# Flat, separate roof volumes and characteristic stepped parapets.
active=roof
for x,y,sx,sy in [(-15,0,10.5,39),(15,0,10.5,39),(0,-14,20,10.5),(0,14,20,10.5)]:
 cube('Flat roof slab',(x,y,4.66),(sx,sy,.22),white)
 for sign in [-1,1]:
  cube('Stepped cornice',(x+sign*(sx/2-.15),y,4.91),(.3,sy,.3),ivory)
  cube('Gold roof ribbon',(x+sign*(sx/2-.31),y,4.76),(.035,sy,.07),gold)
for y in [-19.5,19.5]: cube('Facade parapet',(0,y,4.94),(41,.3,.4),ivory)
active=base
for x in [-19.8,-15,-10,10,15,19.8]:
 for y in [-19.2,19.2]:
  cube('Art Deco pilaster',(x,y,2.5),(.5,.4,5),white)
  for dx in [-.14,0,.14]: cube('Vertical brass ornament',(x+dx,y+math.copysign(.22,y),3.3),(.04,.04,2.6),gold,.006)
# Garden path, carpet and fountain.
active=garden
cube('Courtyard paving',(0,0,-.015),(20,18,.07),stone)
cube('Ceremonial red carpet',(0,-18,.095),(3,16,.035),red,.01)
for x in [-1.46,1.46]: cube('Carpet brass edge',(x,-18,.118),(.025,16,.008),gold,.002)
for x in [-6.8,6.8]:
 for y in [-5.7,5.7]:
  cube('Raised marble flower bed',(x,y,.15),(4.6,3.8,.3),white,.12)
  cube('Planting soil',(x,y,.31),(4.3,3.5,.08),soil,.08)
  for k in range(42):
   px=x+random.uniform(-2,2); py=y+random.uniform(-1.55,1.55); z=random.uniform(.45,.85)
   sphere('Garden leaf cluster',(px,py,z),(.25,.24,.35),green)
   sphere('Colorful flowers',(px,py,z+.28),(.16,.16,.15),[pink,purple,orange][k%3])
cyl('Fountain basin foundation',(0,0,.16),3.1,.32,white,64)
ring('Basin marble rim',(0,0,.43),2.9,.18,white)
cyl('Fountain water',(0,0,.36),2.73,.055,water,64)
cyl('Fountain central pedestal',(0,0,1),.34,1.3,white)
for z,r in [(1.2,1.35),(2.05,.82),(2.75,.35)]:
 cyl('Tier bowl',(0,0,z),r,.12,white,48); ring('Tier lip',(0,0,z+.06),r,.07,white); cyl('Tier water',(0,0,z+.08),r-.07,.035,water)
 cyl('Tier stem',(0,0,z+.32),.14,.52,white)
 for a in range(0,360,30):
  t=math.radians(a); pts=[]
  for j in range(13):
   q=j/12; rr=r+.35*q; pts.append((rr*math.cos(t),rr*math.sin(t),z+.12-.78*q*q))
  tube('Fountain cascade',pts,.023,water)
# Low garden lighting and palms.
def palm(x,y,h=5):
 cyl('Palm trunk',(x,y,h/2),.18,h,bark,12)
 for a in range(0,360,45):
  t=math.radians(a); pts=[(x,y,h),(x+math.cos(t)*.8,y+math.sin(t)*.8,h+.55),(x+math.cos(t)*2,y+math.sin(t)*2,h+.1),(x+math.cos(t)*2.8,y+math.sin(t)*2.8,h-.7)]
  tube('Palm frond rib',pts,.035,leaf)
  # Sweeping leaf meshes, clean double-sided leaflets.
  for k in range(1,8):
   q=k/8; cx=x+math.cos(t)*2.8*q; cy=y+math.sin(t)*2.8*q; cz=h+.65*math.sin(q*math.pi)-.7*q
   for s in [-1,1]:
    tip=(cx+math.cos(t+s*.8)*.9*(1-q/2),cy+math.sin(t+s*.8)*.9*(1-q/2),cz-.3)
    verts=[(cx-.10*math.sin(t),cy+.10*math.cos(t),cz),tip,(cx+.1*math.sin(t),cy-.1*math.cos(t),cz)]
    me=bpy.data.meshes.new('Leaf'); me.from_pydata(verts,[],[(0,1,2)]); ob=bpy.data.objects.new('Palm leaflet',me); active.objects.link(ob); me.materials.append(leaf)
for x in [-8.5,8.5]:
 for y in [-7.8,7.8]: palm(x,y,4.5)
for x in [-22,22]:
 for y in [-17,-5,8,18]: palm(x,y,6)
for x in [-4.5,4.5]:
 for y in [-7,0,7]:
  cyl('Garden lantern post',(x,y,.5),.08,1,gold,12); cyl('Garden lantern',(x,y,1),.14,.3,glow,12)
# Rooms: three usable rooms each side. Furniture stays clear of 2.4m portals.
active=furniture
def sofa(x,y,angle=0):
 parts=[((0,0,.43),(2.6,.95,.38)),((0,.39,.93),(2.6,.18,.85)),((-1.23,0,.74),(.18,.95,.52)),((1.23,0,.74),(.18,.95,.52))]
 a=math.radians(angle)
 for off,sz in parts:
  ox,oy,z=off; ob=cube('Lounge sofa',(x+ox*math.cos(a)-oy*math.sin(a),y+ox*math.sin(a)+oy*math.cos(a),z),sz,cream,.12); ob.rotation_euler.z=a
 for dx in [-.7,.7]: sphere('Teal cushion',(x+dx*math.cos(a),y+dx*math.sin(a),.83),(.36,.24,.2),teal)
def table(x,y,r=.8):
 cyl('Table pedestal',(x,y,.36),.16,.65,gold); cyl('Round walnut table',(x,y,.73),r,.08,wood,48)
for x in [-16.5,16.5]:
 for y in [-6,0,6]:
  if y==6:
   cube('Bedroom rug',(x,y,.085),(4.5,4.3,.025),teal)
   cube('Bed plinth',(x,y+1,.3),(2.2,2.8,.45),wood,.1); cube('Bed linen',(x,y+1,.64),(2.15,2.75,.35),cream,.16)
   cube('Bed headboard',(x,y+2.3,1.1),(2.4,.14,1.7),teal,.1)
   for dx in [-.55,.55]: cube('Pillow',(x+dx,y+1.8,.88),(.8,.5,.15),white,.1)
  else:
   sofa(x,y+1.3); table(x,y-.2)
   cube('Conversation room rug',(x,y,.085),(4.6,4.8,.025),teal)
# Ocean lounge and dining spaces.
for x in [-5,5]:
 cube('Ocean lounge rug',(x,14.5,.09),(7.5,6,.03),teal)
 sofa(x-2,14.8,90); sofa(x+2,14.8,-90); table(x,14.5,1)
for x in [-15,15]:
 cube('Dining tabletop',(x,14,.85),(2.2,4,.12),wood,.08)
 for dx in [-1.5,1.5]:
  for y in [12.7,14,15.3]:
   cube('Dining chair seat',(x+dx,y,.48),(.6,.65,.12),cream,.07); cube('Dining chair back',(x+dx+math.copysign(.25,dx),y,.86),(.12,.65,.8),teal,.06)
   for xx in [-.2,.2]: cube('Chair leg',(x+dx+xx,y,.23),(.05,.45,.46),gold)
# Front foyer console and sitting rooms leave center axis unobstructed.
for x in [-7,7]: sofa(x,-15); table(x,-16.5,.6)
# Exterior terrace / ocean sunset context.
active=env
sand=mat('Beach sand',(.49,.37,.22)); ocean=mat('Ocean blue',(.035,.16,.20),.35,.21)
cube('Island sand',(0,-20,-.6),(150,120,.55),sand,0)
cube('Ocean',(0,230,-.65),(1800,400,.12),ocean,0)
# Water surface shader with fine broad ripples.
p=ocean.node_tree.nodes.get('Principled BSDF'); noise=ocean.node_tree.nodes.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value=1.6; noise.inputs['Detail'].default_value=2
bump=ocean.node_tree.nodes.new('ShaderNodeBump'); bump.inputs['Strength'].default_value=.28; bump.inputs['Distance'].default_value=.12
ocean.node_tree.links.new(noise.outputs['Fac'],bump.inputs['Height']); ocean.node_tree.links.new(bump.outputs['Normal'],p.inputs['Normal'])
active=base
cube('Sea terrace',(0,22,-.05),(42,6,.2),stone)
for x in range(-20,21,2): cyl('Terrace railing post',(x,24.8,.55),.035,1.1,gold,12)
tube('Terrace top rail',[(-20,24.8,1.1),(20,24.8,1.1)],.045,gold)
# Walk markers, useful for placement in downstream engines.
for name,loc in [('Spawn_Entrance',(0,-23,1.7)),('Shot_Courtyard',(0,-7,1.7)),('Shot_OceanLounge',(0,14,1.7))]:
 ob=bpy.data.objects.new(name,None); base.objects.link(ob); ob.location=loc; ob.empty_display_type='ARROWS'
active=lights
world=bpy.data.worlds.new('Sunset sky'); bpy.context.scene.world=world; world.use_nodes=True
nodes=world.node_tree.nodes; sky=nodes.new('ShaderNodeTexSky'); sky.sky_type='NISHITA'; sky.sun_elevation=math.radians(7); sky.sun_rotation=math.radians(120); sky.altitude=.1; sky.air_density=1.2; sky.dust_density=2
world.node_tree.links.new(sky.outputs['Color'],nodes.get('Background').inputs['Color']); nodes.get('Background').inputs['Strength'].default_value=.22
bpy.ops.object.light_add(type='SUN',location=(-20,35,20)); sun=move(bpy.context.object,'Golden sunset sun',None); sun.rotation_euler=(math.radians(73),math.radians(-20),math.radians(-145)); sun.data.energy=2.3; sun.data.color=(1,.62,.34); sun.data.angle=.12
for x,y in [(0,14),(-15,0),(15,0),(0,-14),(-15,14),(15,14)]:
 bpy.ops.object.light_add(type='AREA',location=(x,y,4.25)); o=move(bpy.context.object,'Warm interior ceiling light',None); o.data.energy=450; o.data.shape='DISK'; o.data.size=5; o.data.color=(1,.68,.39)

def camera(name,loc,target,lens):
 bpy.ops.object.camera_add(location=loc); o=move(bpy.context.object,name,None); o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler(); o.data.lens=lens; o.data.clip_end=2000; return o
cam=camera('Camera_Exterior',(45,-61,42),(0,0,1.5),43)
interior=camera('Camera_Interior',(-5,10.2,1.65),(0,22,1.8),20)
courtyard=camera('Camera_Courtyard',(0,-8,1.75),(0,3,2),19)
sc=bpy.context.scene; sc.camera=cam; sc.render.engine='CYCLES'; sc.cycles.samples=24; sc.cycles.use_denoising=True
sc.render.resolution_x=1400; sc.render.resolution_y=1000; sc.render.resolution_percentage=100; sc.view_settings.view_transform='AgX'
sc.unit_settings.system='METRIC'; sc.unit_settings.scale_length=1
exec(compile((ROOT/'refine_mansion.py').read_text(), 'refine_mansion.py', 'exec'))
# Save inspectable camera view on opening.
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D': area.spaces.active.region_3d.view_perspective='CAMERA'
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SeasideMansion.blend'))
# Portable model export, excludes environment, cameras, lights; preserves grouped named objects.
bpy.ops.object.select_all(action='DESELECT')
for c in [base,roof,garden,furniture]:
 for ob in c.objects: ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(ROOT/'SeasideMansion.glb'),use_selection=True,export_apply=True)
report={'units':'metres','model_objects':sum(len(c.objects) for c in [base,roof,garden,furniture]),'courtyard_m':[20,18],'outer_m':[40,38],'door_clear_width_m':2.4,'roof_separate':True,'route':'Entrance -> south hall -> courtyard -> north lounge; side galleries connect six rooms.'}
(ROOT/'model_info.json').write_text(json.dumps(report,indent=2))
for c,name in [(cam,'exterior'),(interior,'interior'),(courtyard,'courtyard')]:
 sc.camera=c; sc.render.filepath=str(ROOT/(name+'.png')); bpy.ops.render.render(write_still=True)
sc.camera=cam; bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SeasideMansion.blend'))
print('MANSION_COMPLETE',json.dumps(report))
