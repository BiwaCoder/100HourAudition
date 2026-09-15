import bpy,json
from pathlib import Path
p=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(p.parent/'SeasideMansion.blend'))
info={'file':bpy.data.filepath,'version':bpy.app.version_string,'objects':len(bpy.data.objects),'collections':[c.name for c in bpy.data.collections],'targets':[{'name':o.name,'location':list(o.location),'dimensions':list(o.dimensions)} for o in bpy.data.objects if o.name.startswith(('Gallery column','Palm trunk','Fountain','Tier','Basin','Ocean'))]}
(p/'source_inventory.json').write_text(json.dumps(info,indent=2))
print('SOURCE_INSPECTED',len(info['targets']))
