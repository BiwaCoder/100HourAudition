"""Standalone Blender authoring. Writes ONLY beside this script, never to Unity Assets.
Z-up, front -Y, meters. Original Himari designs; no external character assets.
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent
sys.path.insert(0,str(P))
from palette import merge_palette
TAU=math.tau
STYLE=sys.argv[sys.argv.index('--')+1] if '--' in sys.argv else 'street'
CUTE=STYLE=='anime';WIRE=STYLE=='wire'; H=1.70 if CUTE else 1.84
S=H/1.84
parts=[];glow_parts=[];mats={};bone_defs={}

def clean():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for d in list(bpy.data.materials):bpy.data.materials.remove(d)
clean()
char=bpy.data.collections.new('CHARACTER - export this collection');bpy.context.scene.collection.children.link(char)
studio=bpy.data.collections.new('STUDIO - preview only');bpy.context.scene.collection.children.link(studio)

def move(o,col):
 for c in list(o.users_collection):c.objects.unlink(o)
 col.objects.link(o)

def toon(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 n=m.node_tree.nodes;n.clear();link=m.node_tree.links
 out=n.new('ShaderNodeOutputMaterial');out.location=(560,0)
 em=n.new('ShaderNodeEmission');em.location=(350,0);em.inputs['Strength'].default_value=1
 geo=n.new('ShaderNodeNewGeometry');geo.location=(-600,0)
 dot=n.new('ShaderNodeVectorMath');dot.operation='DOT_PRODUCT';dot.inputs[1].default_value=(-.45,-.65,.62);dot.location=(-410,0)
 rem=n.new('ShaderNodeMath');rem.operation='MULTIPLY_ADD';rem.inputs[1].default_value=.5;rem.inputs[2].default_value=.5;rem.location=(-230,0)
 ramp=n.new('ShaderNodeValToRGB');ramp.location=(-40,0);ramp.color_ramp.interpolation='CONSTANT'
 vals=[(.0,.48),(.35,.75),(.68,1)]
 r=ramp.color_ramp;r.elements.remove(r.elements[1]);r.elements[0].position=0;r.elements[0].color=(*(c*vals[0][1] for c in color),1)
 for p,k in vals[1:]:r.elements.new(p).color=(*(c*k for c in color),1)
 link.new(geo.outputs['Normal'],dot.inputs[0]);link.new(dot.outputs['Value'],rem.inputs[0]);link.new(rem.outputs[0],ramp.inputs[0]);link.new(ramp.outputs[0],em.inputs['Color']);link.new(em.outputs[0],out.inputs['Surface'])
 mats[name]=m;return m

def pulse(name,color,phase):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes;n.clear();l=m.node_tree.links
 out=n.new('ShaderNodeOutputMaterial');em=n.new('ShaderNodeEmission');em.inputs['Color'].default_value=(*color,1);l.new(em.outputs[0],out.inputs[0]);em.location=(420,0);out.location=(650,0)
 tex=n.new('ShaderNodeTexNoise');tex.noise_dimensions='4D';tex.inputs['Scale'].default_value=5.5;tex.inputs['Detail'].default_value=1.8;tex.location=(-250,0)
 coord=n.new('ShaderNodeTexCoord');l.new(coord.outputs['Generated'],tex.inputs['Vector']);coord.location=(-460,0)
 f=tex.inputs['W'].driver_add('default_value');f.driver.expression=f'frame / 38.0 + {phase}'
 ramp=n.new('ShaderNodeValToRGB');ramp.location=(-25,0);r=ramp.color_ramp;r.elements[0].position=.35;r.elements[0].color=(.09,.09,.09,1);r.elements[1].position=.71;r.elements[1].color=(1,1,1,1)
 mul=n.new('ShaderNodeMath');mul.operation='MULTIPLY';mul.inputs[1].default_value=7;mul.location=(220,0);l.new(tex.outputs['Fac'],ramp.inputs[0]);l.new(ramp.outputs[0],mul.inputs[0]);l.new(mul.outputs[0],em.inputs['Strength'])
 mats[name]=m;return m

toon('Ivory tailored cloth',(.92,.93,.89));toon('Pearl folded edges',(.71,.80,.82));toon('Snow highlights',(.99,.99,.97));toon('Ink black hair',(.009,.012,.019));toon('Hair cool facets',(.024,.033,.050));toon('Hair silk highlights',(.05,.065,.085))
toon('Warm skin',(.78,.51,.37));toon('Soft lip',(.37,.15,.14));toon('Dark lash',(.018,.011,.02));toon('Eye white',(.96,.94,.88));toon('Eye iris',(.14,.07,.055));toon('Eye shine',(1,1,1));toon('Silver clasp',(.49,.59,.63));toon('Warm gold pin',(.68,.48,.19));toon('Shoe charcoal',(.025,.034,.042))
pulse('Living pearl wire',(.70,.88,1),0);pulse('Silver warm accents',(1,.82,.55),1.7)

# All geometry helpers work in authoring coordinates for the current silhouette.
def mesh(name,verts,faces,material,bone='Chest',smooth=False,glow=False):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update()
 o=bpy.data.objects.new(name,me);char.objects.link(o);me.materials.append(mats[material]);o['part']=name
 for p in me.polygons:p.use_smooth=smooth
 g=o.vertex_groups.new(name=bone);g.add(list(range(len(verts))),1,'REPLACE')
 (glow_parts if glow else parts).append(o);return o

def rings(name,rows,material,bone='Chest',N=12,pleat=0,open_panels=False):
 # rows: z, x-radius, y-radius, center-x, center-y
 v=[];f=[]
 for j,(z,rx,ry,cx,cy) in enumerate(rows):
  for i in range(N):
   a=TAU*i/N;ratio=1+(pleat if i%2==0 else -pleat)
   v.append((cx+rx*math.cos(a)*ratio,cy+ry*math.sin(a)*ratio,z))
 for j in range(len(rows)-1):
  for i in range(N):
   if open_panels and i%4 in (1,2):continue
   ni=(i+1)%N;f.append((j*N+i,j*N+ni,(j+1)*N+ni,(j+1)*N+i))
 if not open_panels:
  f.append(tuple(reversed(range(N))));f.append(tuple((len(rows)-1)*N+i for i in range(N)))
 return mesh(name,v,f,material,bone)

def oval(name,center,radius,material,bone='Head',N=12,R=7,smooth=False):
 rows=[]
 for j in range(R+1):
  a=-math.pi/2+math.pi*j/R;k=max(.025,math.cos(a));rows.append((center[2]+radius[2]*math.sin(a),radius[0]*k,radius[1]*k,center[0],center[1]))
 o=rings(name,rows,material,bone,N);[setattr(p,'use_smooth',smooth) for p in o.data.polygons];return o

def tube(name,points,r,material,bone='Chest',N=5,glow=False):
 v=[];f=[];ps=[Vector(p) for p in points]
 for i,p in enumerate(ps):
  tangent=(ps[min(i+1,len(ps)-1)]-ps[max(i-1,0)]).normalized();ref=Vector((0,0,1)) if abs(tangent.z)<.9 else Vector((1,0,0));u=tangent.cross(ref).normalized();w=tangent.cross(u).normalized()
  for j in range(N):v.append(tuple(p+r*(u*math.cos(TAU*j/N)+w*math.sin(TAU*j/N))))
 for i in range(len(ps)-1):
  for j in range(N):f.append((i*N+j,i*N+(j+1)%N,(i+1)*N+(j+1)%N,(i+1)*N+j))
 return mesh(name,v,f,material,bone,False,glow)

def edge_loop(name,z,rx,ry,material,bone='Pelvis',N=24,r=.0025,pleat=0):
 pts=[]
 for i in range(N+1):
  a=TAU*i/N;k=1+(pleat if i%2==0 else -pleat);pts.append((rx*math.cos(a)*k,ry*math.sin(a)*k,z))
 return tube(name,pts,r,material,bone,4,WIRE)

def panel(name,points,thick,material,bone='Chest'):
 # Author points front-facing; thickness extends to the back (+Y).
 n=len(points);v=list(points)+[(x,y+thick,z) for x,y,z in points];f=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]
 for i in range(n):j=(i+1)%n;f.append((i,j,j+n,i+n))
 o=mesh(name,v,f,material,bone)
 # Consistent normals for arbitrary wound pattern pieces.
 bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT');o.select_set(False)
 return o

def bind_blend(o,a,b,za,zb):
 o.vertex_groups.clear();ga=o.vertex_groups.new(name=a);gb=o.vertex_groups.new(name=b)
 for v in o.data.vertices:
  t=max(0,min(1,(v.co.z-zb)/(za-zb)))
  ga.add([v.index],t,'REPLACE');gb.add([v.index],1-t,'REPLACE')

# Bone plan, including skirt and long hair controls.
waist=1.04*S;shoulder=1.43*S;neck=1.54*S
hc=1.66*S if not CUTE else 1.49
hr=(.13,.115,.17) if not CUTE else (.179,.148,.211)
headtop=hc+hr[2]
legx=.104*S
bone_defs={'Root':((0,0,0),(0,0,.15),None),'Pelvis':((0,0,.94*S),(0,0,1.06*S),'Root'),'Spine':((0,0,1.06*S),(0,0,1.24*S),'Pelvis'),'Chest':((0,0,1.24*S),(0,0,shoulder),'Spine'),'Neck':((0,0,shoulder),(0,0,neck),'Chest'),'Head':((0,0,neck),(0,0,headtop),'Neck')}
for sign,side in [(-1,'L'),(1,'R')]:
 sx=sign*(.205 if not CUTE else .205);elx=sign*.279;wrx=sign*.315
 bone_defs.update({f'UpperArm.{side}':((sx,0,shoulder),(elx,-.005,1.17*S),'Chest'),f'Forearm.{side}':((elx,-.005,1.17*S),(wrx,-.028,.95*S),f'UpperArm.{side}'),f'Hand.{side}':((wrx,-.028,.95*S),(sign*.326,-.034,.865*S),f'Forearm.{side}'),f'Thigh.{side}':((sign*legx,0,.93*S),(sign*legx,0,.53*S),'Pelvis'),f'Shin.{side}':((sign*legx,0,.53*S),(sign*legx,0,.11*S),f'Thigh.{side}'),f'Foot.{side}':((sign*legx,0,.11*S),(sign*legx,-.14,.05),f'Shin.{side}'),f'Hair.{side}':((sign*.13,.045,hc+.1),(sign*.16,.06,1.12*S),'Head')})
bone_defs['Hair.Back']=((0,.1,hc+.1),(0,.13,1.00*S),'Head')
for n,x,y in [('Front',0,-.08),('Back',0,.08),('Left',-.1,0),('Right',.1,0)]:bone_defs['Skirt.'+n]=((x,y,waist),(x*2,y*2,.58*S),'Pelvis')

# Lower body. Covered upper legs support skirt animation without visible holes.
for sign,side in [(-1,'L'),(1,'R')]:
 x=sign*legx
 leg=rings('Leg '+side,[(.115*S,.043,.05,x,0),(.52*S,.055,.064,x,0),(.90*S,.054,.060,x,0)],'Warm skin',f'Thigh.{side}',10)
 bind_blend(leg,f'Thigh.{side}',f'Shin.{side}',.62*S,.44*S)
 if WIRE:
  # Restrained line anatomy beneath an open dress, not a detailed face/body.
  leg.data.materials[0]=mats['Hair cool facets']
  for dx in [-.044,.044]:tube('Leg light '+side,[(x+dx,-.018,.14*S),(x+dx,-.018,.52*S),(x+dx,-.018,.85*S)],.0021,'Living pearl wire',f'Shin.{side}',4,True)
 boot_top=(.35 if STYLE=='street' else .13)*S
 rings('Ivory boot '+side,[(.038,.066,.125,x,-.045),(.095,.071,.116,x,-.038),(boot_top,.057,.068,x,0)],'Ivory tailored cloth',f'Foot.{side}',10 if CUTE else 8)
 rings('Dark sole '+side,[(.018,.068,.126,x,-.044),(.038,.068,.126,x,-.044)],'Shoe charcoal',f'Foot.{side}',10 if CUTE else 8)
 edgepts=[(x-.066,-.065,.082),(x,.0 if not CUTE else -.07,.105),(x+.066,-.065,.082)]
 if CUTE:tube('Mary Jane strap '+side,edgepts,.011,'Pearl folded edges',f'Foot.{side}',6)
 else:tube('Boot front seam '+side,[(x,-.118,.085),(x,-.07,boot_top-.01)],.0025,'Silver clasp',f'Foot.{side}',4)

# A fitted but fully covered blouse/jacket, with a flared skirt.
body=rings('White fitted bodice',[(waist,.137,.085,0,0),(1.22*S,.16,.10,0,0),(1.38*S,.206,.11,0,0),(1.47*S,.17,.091,0,0),(neck,.058,.058,0,0)],'Ivory tailored cloth','Chest',12 if CUTE else 8)
bind_blend(body,'Chest','Spine',1.35*S,1.09*S)
if WIRE:
 body.data.materials[0]=mats['Hair cool facets']
 for a in [0,math.pi/2,math.pi,3*math.pi/2]:
  tube('Bodice light',[(.139*math.cos(a),.087*math.sin(a),waist),(.163*math.cos(a),.103*math.sin(a),1.22*S),(.21*math.cos(a),.114*math.sin(a),1.38*S),(.062*math.cos(a),.062*math.sin(a),neck)],.003,'Living pearl wire','Chest',4,True)
hem=(.59 if not CUTE else .58)*S;N=24 if CUTE else 20
skirt=rings('Pleated white skirt',[(hem,.32 if CUTE else .30,.23,0,0),(.75*S,.26,.181,0,0),(waist,.142,.09,0,0)],'Ivory tailored cloth','Pelvis',N,.035 if CUTE else .075,WIRE)
# Split skirt control weights by azimuth, with a firm waistband.
skirt.vertex_groups.clear();gs={n:skirt.vertex_groups.new(name=n) for n in ['Pelvis','Skirt.Front','Skirt.Back','Skirt.Left','Skirt.Right']}
for v in skirt.data.vertices:
 x,y,z=v.co;name=('Skirt.Front' if y<0 else 'Skirt.Back') if abs(y)/.23>abs(x)/.30 else ('Skirt.Left' if x<0 else 'Skirt.Right');t=max(0,min(1,(waist-z)/.26));gs['Pelvis'].add([v.index],1-t,'REPLACE');gs[name].add([v.index],t,'REPLACE')
edge_loop('Pearl hem piping',hem+.012,.322 if CUTE else .302,.233,'Living pearl wire' if WIRE else 'Pearl folded edges','Pelvis',N,.0035,.035 if CUTE else .075)
edge_loop('Waist trim',waist+.012,.145,.094,'Silver warm accents' if WIRE else 'Silver clasp','Pelvis',20,.006)
if WIRE:
 for i in range(N):
  a=TAU*i/N;tube('Skirt luminous pleat',[(.144*math.cos(a),.093*math.sin(a),waist),(.265*math.cos(a),.185*math.sin(a),.75*S),(.31*math.cos(a),.239*math.sin(a),hem)],.0026,'Living pearl wire','Pelvis',4,True)

# Long sleeves and graceful relaxed A-pose hands.
for sign,side in [(-1,'L'),(1,'R')]:
 sx=sign*.205;elx=sign*.279;wrx=sign*.315
 sleeve=rings('Long sleeve '+side,[(.95*S,.038,.042,wrx,-.028),(1.17*S,.054,.057,elx,-.005),(shoulder,.075 if not CUTE else .084,.078,sx,0)],'Ivory tailored cloth',f'UpperArm.{side}',10 if CUTE else 6)
 bind_blend(sleeve,f'UpperArm.{side}',f'Forearm.{side}',1.23*S,1.11*S)
 if CUTE:oval('Soft puff sleeve '+side,(sign*.230,-.004,1.345*S),(.095,.091,.133),'Snow highlights',f'UpperArm.{side}',12,6)
 cuff=rings('Tailored cuff '+side,[(.941*S,.045,.048,wrx,-.028),(.986*S,.048,.05,wrx,-.025)],'Pearl folded edges',f'Forearm.{side}',10 if CUTE else 6)
 hand=oval('Hand '+side,(sign*.326,-.034,.898*S),(.033,.031,.064*S),'Warm skin',f'Hand.{side}',10,5)
 oval('Thumb '+side,(sign*.300,-.056,.913*S),(.014,.017,.026),'Hair cool facets' if WIRE else 'Warm skin',f'Hand.{side}',8,4)
 if WIRE:
  sleeve.data.materials[0]=mats['Hair cool facets'];hand.data.materials[0]=mats['Hair cool facets']
  for k in [-1,1]:tube('Sleeve light '+side,[(sx+k*.074,-.027,shoulder),(elx+k*.053,-.04,1.17*S),(wrx+k*.039,-.06,.95*S)],.0026,'Living pearl wire',f'UpperArm.{side}',4,True)

# Fashion details: architectural lapel vs rounded collar and ribbon.
if CUTE:
 for sign in [-1,1]:
  oval('Peter Pan collar',(sign*.068,-.086,1.455*S),(.075,.025,.051),'Snow highlights','Chest',10,5)
 for z in [1.35,1.27,1.19]:oval('Pearl blouse button',(0,-.108,z*S),(.009,.007,.010),'Warm gold pin','Chest',8,4)
 for sign in [-1,1]:
  panel('Waist ribbon bow',[(0,-.106,waist), (sign*.092,-.115,waist+.035),(sign*.09,-.111,waist-.032)],.012,'Snow highlights','Pelvis')
  panel('Ribbon tails',[(sign*.013,-.105,waist),(sign*.050,-.128,waist-.12),(sign*.081,-.13,waist-.11),(sign*.046,-.107,waist)],.007,'Pearl folded edges','Pelvis')
 oval('Bow pearl',(0,-.12,waist),(.020,.015,.025),'Warm gold pin','Pelvis',8,4)
 # A scalloped underskirt edge, geometric rather than texture-based.
 for i in range(16):
  a=TAU*i/16;oval('Scalloped white hem',(.298*math.cos(a),.212*math.sin(a),hem+.002),(.040,.025,.020),'Snow highlights','Pelvis',8,4)
else:
 panel('Asymmetric sculpted lapel',[(-.04,-.069,neck),(-.181,-.112,1.415*S),(-.065,-.139,1.25*S),(.056,-.081,1.47*S)],.017,'Snow highlights')
 panel('Opposite narrow lapel',[(.059,-.079,1.49*S),(.184,-.104,1.41*S),(.112,-.13,1.30*S)],.014,'Pearl folded edges')
 tube('Offset silver closure',[(.047,-.099,1.44*S),(-.031,-.118,1.24*S),(-.054,-.097,waist+.02)],.0032,'Silver clasp','Chest',5)
 panel('Angular belt clasp',[(-.027,-.104,waist+.034),(.027,-.104,waist+.034),(.027,-.104,waist-.004),(-.027,-.104,waist-.004)],.009,'Silver clasp','Pelvis')
 panel('White hip fold',[(-.141,-.075,waist),(-.22,-.143,.84*S),(-.084,-.19,.78*S),(-.039,-.109,waist)],.009,'Snow highlights','Pelvis')

# Face is slightly larger and softer in the anime silhouette.
face=oval('Face',(0,-.012,hc),hr,'Hair cool facets' if WIRE else 'Warm skin','Head',16 if CUTE else 10,9 if CUTE else 6)
# Black scalp dome, then a real waist-length curtain behind the face.
rows=[(headtop+.020,.025,.025,0,0),(hc+hr[2]*.8,hr[0]*.97,hr[1]*1.13,0,-.014),(hc+.035,hr[0]*1.10,hr[1]*1.13,0,.018)]
rings('Black hair crown',list(reversed(rows)),'Ink black hair','Head',16 if CUTE else 10)
# 180-degree rear wrap with multiple vertical rows, no face-covering front bowl.
v=[];f=[];R=7 if CUTE else 5;N=12 if CUTE else 8
for j in range(R):
 t=j/(R-1);z=(hc+.10)*(1-t)+(1.00*S)*t;rx=hr[0]*1.09+.035*math.sin(t*math.pi);ry=hr[1]+.025+.025*t
 for i in range(N+1):
  a=math.pi*i/N;x=rx*math.cos(a);y=.025+ry*math.sin(a);zz=z+(.035*math.cos(i*1.7) if j==R-1 else 0)
  v.append((x,y,zz))
for j in range(R-1):
 for i in range(N):q=j*(N+1)+i;f.append((q,q+1,q+N+2,q+N+1))
curtain=mesh('Long black hair - waist length',v,f,'Ink black hair','Hair.Back')
sol=curtain.modifiers.new('Hair thickness','SOLIDIFY');sol.thickness=.016
# Layered side locks preserve the long-hair silhouette from front and side.
for sign,side in [(-1,'L'),(1,'R')]:
 x=sign*hr[0]
 pts=[(x*.78,-.055,hc+.11),(x*1.11,-.065,hc+.03),(x*1.15,-.043,1.27*S),(x*.95,-.073,1.03*S)]
 outer=[(px+sign*.049,py+.015,pz+.015) for px,py,pz in pts]
 panel('Long face framing lock '+side,pts+list(reversed(outer)),.025,'Ink black hair',f'Hair.{side}')
 tube('Hair sheen '+side,[(x*1.18,-.067,hc+.06),(x*1.24,-.053,1.32*S),(x*1.04,-.082,1.08*S)],.002 if CUTE else .003,'Hair silk highlights',f'Hair.{side}',4)
 if WIRE:tube('Luminous hair strand '+side,[(x,-.073,hc+.11),(x*1.2,-.078,1.34*S),(x*1.1,-.09,1.04*S)],.0018,'Living pearl wire',f'Hair.{side}',4,True)
# Sweep bangs, parted slightly off center. Tapered locks, not a helmet.
for x0,x1,tip in ([(-1,-.50,-.72),(-.55,-.10,-.32),(-.13,.30,.06),(.26,.65,.47),(.60,1,.80)] if CUTE else [(-1,-.25,-.68),(-.28,.29,-.12),(.24,1,.68)]):
 y=-hr[1]-.012
 panel('Swept black fringe',[(x0*hr[0],y*.76,hc+hr[2]*.79),(x1*hr[0],y*.8,hc+hr[2]*.81),(tip*hr[0],y,hc+.044)],.024,'Ink black hair','Head')
# Subtle posterior facets and a white ribbon make the back view intentional.
for a in [.4,.9,1.5,2.1,2.7]:
 tube('Long hair facet seam',[(hr[0]*math.cos(a),.025+hr[1]*math.sin(a),hc+.08),((hr[0]+.025)*math.cos(a),.03+(hr[1]+.04)*math.sin(a),1.30*S),(hr[0]*math.cos(a),.03+(hr[1]+.03)*math.sin(a),1.02*S)],.0017,'Hair cool facets','Hair.Back',4)
if CUTE:
 for sign in [-1,1]:panel('White hair ribbon',[(0,.191,hc+.06),(sign*.104,.183,hc+.12),(sign*.096,.205,hc-.013)],.015,'Snow highlights','Head')
 oval('Hair ribbon knot',(0,.20,hc+.045),(.024,.016,.025),'Pearl folded edges','Head',8,4)
if not WIRE:
 for sign in [-1,1]:
  ex=sign*(.073 if CUTE else .050);ez=hc+.007;ey=-hr[1]-.010;ew=.041 if CUTE else .027;eh=.048 if CUTE else .023
  # Thin almond patches follow the front plane of the head, avoiding bulging eyeballs.
  outline=[(-1,0),(-.5,.78),(.3,1),(1,.06),(.48,-.66),(-.35,-.78)]
  pts=[(ex+ew*x,ey,ez+eh*z) for x,z in outline];panel('Almond eye white',pts,.001,'Eye white','Head')
  oval('Dark iris',(ex,ey-.002,ez),(.020 if CUTE else .012,.003,.031 if CUTE else .018),'Eye iris','Head',10,6)
  oval('Pupil',(ex,ey-.005,ez),(.010 if CUTE else .006,.002,.023 if CUTE else .012),'Dark lash','Head',8,5)
  oval('Eye glint',(ex-.006,ey-.008,ez+.013),(.005,.002,.007),'Eye shine','Head',6,4)
  tube('Upper eyelash',[(ex-ew,ey-.005,ez),(ex-ew*.5,ey-.005,ez+eh*.78),(ex+ew*.3,ey-.005,ez+eh),(ex+ew,ey-.005,ez+eh*.06)],.0022,'Dark lash','Head',4)
  tube('Soft eyebrow',[(ex-ew*.72,ey+.006,ez+eh+.025),(ex,ey+.003,ez+eh+.029),(ex+ew*.72,ey+.006,ez+eh+.024)],.0028,'Ink black hair','Head',4)
  oval('Ear',(sign*hr[0]*.97,0,hc-.01),(.024,.029,.043),'Warm skin','Head',8,5)
  oval('Pearl earring',(sign*hr[0]*1.04,-.007,hc-.053),(.009,.01,.012),'Snow highlights','Head',8,4)
 # Delicate geometric nose and a gently curved mouth; no aggressive expression.
 oval('Nose',(0,-hr[1]-.013,hc-.038),(.013,.018,.019),'Warm skin','Head',8,4)
 tube('Quiet smile',[(-.024,-hr[1]*.91-.013,hc-.079),(0,-hr[1]*.94-.014,hc-.085),(.024,-hr[1]*.91-.013,hc-.079)],.0022,'Soft lip','Head',5)
else:
 pts=[(hr[0]*math.cos(TAU*i/20),-hr[1]*.33,hc+hr[2]*math.sin(TAU*i/20)) for i in range(21)]
 tube('Blank face light contour',pts,.0026,'Living pearl wire','Head',4,True)

# Real armature, named controls, rigid + blended weights and two preview actions.
armdata=bpy.data.armatures.new('Himari humanoid skeleton');arm=bpy.data.objects.new('Himari_Rig',armdata);char.objects.link(arm);bpy.context.view_layer.objects.active=arm;arm.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
for name,(a,b,parent) in bone_defs.items():
 eb=armdata.edit_bones.new(name);eb.head=a;eb.tail=b
 if parent:eb.parent=armdata.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');arm.show_in_front=True;arm.select_set(False)
# Apply thickness before joining so the .blend has editable polygon hair surfaces.
for o in parts:
 if o.modifiers:
  bpy.context.view_layer.objects.active=o
  for m in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=m.name)

def join(objs,name):
 if not objs:return None
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 mod=o.modifiers.new('Himari skin','ARMATURE');mod.object=arm;o.parent=arm;return o
body=join(parts,'Himari_'+STYLE+'_Body');wire=join(glow_parts,'Himari_'+STYLE+'_LuminousContours')
merge_palette(body)
# Inverted hull for preview; exported FBX intentionally omits this shader-specific shell.
ink=bpy.data.materials.new('PREVIEW ink hull');ink.use_nodes=True;ns=ink.node_tree.nodes;ns.clear();lk=ink.node_tree.links
out=ns.new('ShaderNodeOutputMaterial');mix=ns.new('ShaderNodeMixShader');geo=ns.new('ShaderNodeNewGeometry');em=ns.new('ShaderNodeEmission');em.inputs[0].default_value=(.008,.011,.017,1);trans=ns.new('ShaderNodeBsdfTransparent');lk.new(geo.outputs['Backfacing'],mix.inputs[0]);lk.new(em.outputs[0],mix.inputs[1]);lk.new(trans.outputs[0],mix.inputs[2]);lk.new(mix.outputs[0],out.inputs[0])
if not WIRE:
 hull=body.copy();hull.data=body.data.copy();hull.name='PREVIEW_Outline';char.objects.link(hull);hull.data.materials.clear();hull.data.materials.append(ink)
 # Normal inflation via solidify: only the flipped shell is drawn.
 so=hull.modifiers.new('Toon outline shell','SOLIDIFY');so.thickness=.0018 if CUTE else .0028;so.offset=1;so.use_flip_normals=True;so.use_rim=False
 # Original inner faces would duplicate the fill. Hide them with zero-thickness material routing is avoided:
 # create the outer copy directly from per-vertex normals, keeping source mesh unmodified.
 hull.modifiers.remove(so)
 for vert in hull.data.vertices:vert.co+=vert.normal*(.0018 if CUTE else .0028)
 # Reverse all shell polygons through edit mode.
 bpy.ops.object.select_all(action='DESELECT');hull.select_set(True);bpy.context.view_layer.objects.active=hull;bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.flip_normals();bpy.ops.object.mode_set(mode='OBJECT');hull.select_set(False)

arm.animation_data_create()
for actname,walk in [('Himari_Idle',False),('Himari_WalkPreview',True)]:
 action=bpy.data.actions.new(actname);arm.animation_data.action=action;action.use_fake_user=True
 end=32 if walk else 96
 for frame in range(1,end+1,4):
  phase=(frame-1)/(end-1)*TAU
  for pb in arm.pose.bones:pb.rotation_mode='XYZ';pb.rotation_euler=(0,0,0)
  arm.pose.bones['Chest'].rotation_euler.y=math.sin(phase)*.012
  for sign,side in [(-1,'L'),(1,'R')]:
   arm.pose.bones['UpperArm.'+side].rotation_euler.x=sign*math.sin(phase)*(.27 if walk else .009)
   arm.pose.bones['Forearm.'+side].rotation_euler.x=-.06 if walk else 0
   arm.pose.bones['Thigh.'+side].rotation_euler.x=-sign*math.sin(phase)*(.20 if walk else 0)
   arm.pose.bones['Shin.'+side].rotation_euler.x=max(0,sign*math.sin(phase))*.24 if walk else 0
   arm.pose.bones['Hair.'+side].rotation_euler.y=math.sin(phase+sign)*.022
  arm.pose.bones['Hair.Back'].rotation_euler.x=math.sin(phase+.5)*.02
  for n in ['Front','Back','Left','Right']:arm.pose.bones['Skirt.'+n].rotation_euler.y=math.sin(phase)*(.024 if walk else .007)
  for pb in arm.pose.bones:pb.keyframe_insert('rotation_euler',frame=frame,group=pb.name)
 # Duplicate the first sample at loop end for a clean loop.
 for pb in arm.pose.bones:
  for axis in range(3):
   curve=next((fc for fc in action.fcurves if fc.data_path==pb.path_from_id('rotation_euler') and fc.array_index==axis),None)
   if curve:curve.keyframe_points.insert(end+1,curve.evaluate(1))
arm.animation_data.action=bpy.data.actions['Himari_Idle'];bpy.context.scene.frame_set(1)

# Studio, not exported.
def studio_mat():
 m=bpy.data.materials.new('Studio slate');m.diffuse_color=(.035,.052,.075,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.035,.052,.075,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.78;return m
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,0));floor=bpy.context.object;floor.name='Studio floor';move(floor,studio);floor.data.materials.append(studio_mat())
world=bpy.context.scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.17,.25,1);world.node_tree.nodes['Background'].inputs[1].default_value=.4
for name,loc,power,size in [('Key',(-3,-4,5),650,5),('Rim',(3,2,4),750,3),('Soft fill',(3,-1,3),300,4)]:
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size;o=bpy.data.objects.new(name,data);studio.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('Portrait camera');cam=bpy.data.objects.new('Portrait camera',camdata);studio.objects.link(cam);cam.location=(2.4,-5.3,2.15);cam.rotation_euler=(Vector((0,0,.92))-cam.location).to_track_quat('-Z','Y').to_euler();camdata.type='ORTHO';camdata.ortho_scale=2.2;scene=bpy.context.scene;scene.camera=cam
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=900;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast' if 'Medium High Contrast' in [i.name for i in scene.bl_rna.properties] else 'None'
scene.render.fps=24;scene.frame_start=1;scene.frame_end=97
scene.use_nodes=True;n=scene.node_tree.nodes;n.clear();r=n.new('CompositorNodeRLayers');out=n.new('CompositorNodeComposite')
if WIRE:
 g=n.new('CompositorNodeGlare');g.glare_type='FOG_GLOW';g.quality='HIGH';g.threshold=1.2;scene.node_tree.links.new(r.outputs['Image'],g.inputs[0]);scene.node_tree.links.new(g.outputs['Image'],out.inputs[0])
else:scene.node_tree.links.new(r.outputs['Image'],out.inputs[0])
# Metadata and author-facing instructions remain in each .blend.
text=bpy.data.texts.new('README - Himari female prototype');text.write(f'''HIMARI / {STYLE.upper()} / original female protagonist\nBlack waist-length hair, ivory tailored skirt outfit. Units: meters, front -Y, up Z.\nCHARACTER collection: weighted body, 25-bone rig, optional wire contours; PREVIEW_Outline is a renderer-specific helper.\nSTUDIO collection: cameras/lights/floor, never export to Unity.\nActions: Himari_Idle (1-97), Himari_WalkPreview (1-33). Hair and skirt controls included.\nMaterials use Blender toon nodes; export base colors + geometry, then reuse Unity toon/wire shaders.\nUnity not touched. Current earlier prototype models are reserved for bachelor Yuto in a later integration step.\n''')
scene['character_role']='female protagonist / himari';scene['style']=STYLE;scene['unity_integration']='Pending; do not modify running Unity session'
# Select the editable protagonist, not the studio.
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);bpy.context.view_layer.objects.active=body
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':area.spaces.active.region_3d.view_distance=3.3;area.spaces.active.region_3d.view_location=(0,0,.95)
name={'wire':'Himari-Wire','street':'Himari-StreetCel','anime':'Himari-Anime'}[STYLE]
bpy.ops.wm.save_as_mainfile(filepath=str(P/(name+'.blend')))
# FBX excludes preview outlines/studio, contains rig + independently editable materials.
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);arm.select_set(True)
if wire:wire.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=str(P/(name+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_mesh_modifiers=True)
# Final preview outputs and statistics.
stats={'style':STYLE,'file':name+'.blend','height_m':round(headtop+.020,3),'bones':len(arm.data.bones),'vertices':len(body.data.vertices),'triangles':sum(len(f.vertices)-2 for f in body.data.polygons),'wire_triangles':sum(len(f.vertices)-2 for f in wire.data.polygons) if wire else 0,'materials':[m.name for m in body.data.materials],'actions':[a.name for a in bpy.data.actions]}
(P/(name+'-stats.json')).write_text(json.dumps(stats,indent=2))
scene.render.filepath=str(P/(name+'-front.png'));bpy.ops.render.render(write_still=True)
cam.location=(-2.7,5,2.1);cam.rotation_euler=(Vector((0,0,.93))-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(P/(name+'-back.png'));bpy.ops.render.render(write_still=True)
print('MODEL_COMPLETE',json.dumps(stats))
