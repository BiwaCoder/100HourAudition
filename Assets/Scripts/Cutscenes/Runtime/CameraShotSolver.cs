using System.Collections.Generic;
using UnityEngine;
namespace HundredHour.Cutscenes
{
    public struct ShotPose { public Vector3 position; public Quaternion rotation; public float fov; }
    // Refactored from CineDirector.Domain.CameraShotSolver and CameraMenuButtons.ZoomRoutine.
    public static class CameraShotSolver
    {
        public static float ZoomFov(float from,float to,float t)
        {
            const float sensor=24;
            return Camera.FocalLengthToFieldOfView(Mathf.Lerp(Camera.FieldOfViewToFocalLength(from,sensor),Camera.FieldOfViewToFocalLength(to,sensor),Mathf.SmoothStep(0,1,t)),sensor);
        }
        public static ShotPose Evaluate(CameraShot shot,float t,Dictionary<string,CutsceneActor> actors,Vector3 stageForward,float aspect)
        {
            var a=actors[shot.actorA];var aim=a.Aim;var forward=Vector3.ProjectOnPlane(stageForward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
            float fov=ZoomFov(shot.startFov,shot.endFov,t);float distance=shot.distance;var pos=Vector3.zero;float radius=a.framingRadius;
            if(shot.kind==ShotKind.OverShoulder || shot.kind==ShotKind.ReverseShoulder)
            {
                var b=actors[shot.actorB];if(shot.kind==ShotKind.ReverseShoulder){var swap=a;a=b;b=swap;}
                var toward=Vector3.ProjectOnPlane(b.Aim-a.Aim,Vector3.up).normalized;if(toward.sqrMagnitude<.5f)toward=forward;
                var right=Vector3.Cross(Vector3.up,toward);pos=a.Aim-toward*Mathf.Max(1.5f,shot.distance*.35f)+right*Mathf.Max(1.2f,a.framingRadius*1.7f)+Vector3.up*(shot.elevation*.25f);aim=b.Aim;
            }
            else
            {
                if(shot.kind==ShotKind.TwoShot)
                {
                    var b=actors[shot.actorB];aim=(a.Aim+b.Aim)*.5f;radius=Vector3.Distance(a.Aim,b.Aim)*.5f+Mathf.Max(a.framingRadius,b.framingRadius);
                    var side=Vector3.ProjectOnPlane(b.Aim-a.Aim,Vector3.up).normalized;
                    if(side.sqrMagnitude>.5f){var facing=Vector3.Cross(side,Vector3.up);if(Vector3.Dot(facing,forward)<0)facing=-facing;forward=facing;}
                }
                if(shot.kind==ShotKind.GroupWide || shot.kind==ShotKind.RearWide)
                {
                    aim=Vector3.zero;foreach(var actor in actors.Values)aim+=actor.Aim;aim/=actors.Count;radius=0;
                    foreach(var actor in actors.Values)radius=Mathf.Max(radius,Vector3.Distance(aim,actor.Aim)+actor.framingRadius);
                }
                if(shot.kind==ShotKind.TwoShot || shot.kind==ShotKind.GroupWide || shot.kind==ShotKind.RearWide)
                {
                    var halfFov=Mathf.Atan(Mathf.Tan(Mathf.Min(shot.startFov,shot.endFov)*Mathf.Deg2Rad*.5f)*Mathf.Min(1,aspect));
                    distance=Mathf.Max(distance,radius/Mathf.Sin(halfFov)*1.1f);
                }
                float angle=shot.yaw+(shot.kind==ShotKind.Orbit?shot.orbitDegrees*Mathf.SmoothStep(0,1,t):0);
                if(shot.kind==ShotKind.RearWide)angle+=180;
                if(shot.kind==ShotKind.DollyIn)distance*=Mathf.Lerp(1,.55f,Mathf.SmoothStep(0,1,t));
                if(shot.kind==ShotKind.DollyOut)distance*=Mathf.Lerp(.55f,1,Mathf.SmoothStep(0,1,t));
                pos=aim+Quaternion.AngleAxis(angle,Vector3.up)*forward*distance+Vector3.up*shot.elevation;
            }
            // Translate both lens and aim to truck sideways without orbiting the subject.
            var cameraRight=Quaternion.LookRotation(aim-pos,Vector3.up)*Vector3.right;
            var lateral=cameraRight*Mathf.Lerp(shot.lateralTravel*.5f,-shot.lateralTravel*.5f,Mathf.Clamp01(t));
            pos+=lateral;aim+=lateral;
            // Keep the lens above the existing terrain without modifying the terrain.
            foreach(var terrain in Terrain.activeTerrains)
            {
                var p=pos-terrain.transform.position;var size=terrain.terrainData.size;
                if(p.x>=0 && p.z>=0 && p.x<=size.x && p.z<=size.z)pos.y=Mathf.Max(pos.y,terrain.SampleHeight(pos)+terrain.transform.position.y+.35f);
            }
            return new ShotPose{position=pos,rotation=Quaternion.LookRotation(aim-pos,Vector3.up),fov=fov};
        }
    }
}
