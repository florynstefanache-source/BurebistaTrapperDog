using UnityEngine;
namespace BurebistaTrapperDog;
public partial class Main {
 readonly struct RestBonePose {
  public readonly string Name;public readonly Vector3 Position;public readonly Quaternion Rotation;
  public RestBonePose(string name,Vector3 position,Quaternion rotation){Name=name;Position=position;Rotation=rotation;}
 }
 static readonly RestBonePose[] SleepingPose={
new RestBonePose("Skeleton_0",new Vector3(0.00000000f,0.00000000f,0.00000000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,1.00000000f)),
new RestBonePose("Skeleton_1",new Vector3(0.00000000f,0.00000000f,0.00000000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,1.00000000f)),
new RestBonePose("Bone_2",new Vector3(0.00000022f,6.82443125f,-3.50000000f),new Quaternion(0.50000006f,-0.49999994f,-0.50000000f,-0.49999999f)),
new RestBonePose("Bone1_3",new Vector3(-9.50000000f,-0.00000000f,2.37500000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone2_4",new Vector3(-3.75000000f,0.00000000f,2.12500000f),new Quaternion(0.00000009f,0.86169647f,0.00000005f,-0.50742407f)),
new RestBonePose("Bone3_5",new Vector3(-3.50000000f,0.00000000f,4.00000000f),new Quaternion(-0.00000016f,0.92756887f,0.00000033f,0.37365223f)),
new RestBonePose("Bone4_6",new Vector3(-3.25000000f,-0.00000000f,-1.62500000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone5_7",new Vector3(-0.00000086f,-1.24999964f,2.12499976f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone6_8",new Vector3(-0.00000107f,1.50000036f,2.12500024f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone17_9",new Vector3(-0.62500030f,3.87500024f,-5.37499952f),new Quaternion(-0.00000001f,-0.55211775f,0.00000006f,-0.83376615f)),
new RestBonePose("Bone18_10",new Vector3(0.12499971f,-0.00000050f,-4.87500000f),new Quaternion(-0.00000009f,0.07178828f,0.00000001f,0.99741989f)),
new RestBonePose("Bone19_11",new Vector3(-1.12499976f,-0.00000004f,-6.25000000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone21_12",new Vector3(-0.62499970f,-3.62499976f,-5.37500048f),new Quaternion(-0.00000001f,-0.55211776f,0.00000006f,-0.83376614f)),
new RestBonePose("Bone22_13",new Vector3(0.12499980f,-0.00000050f,-4.87500000f),new Quaternion(-0.00000009f,0.07178820f,0.00000001f,0.99741990f)),
new RestBonePose("Bone23_14",new Vector3(-1.12500072f,-0.00000004f,-6.25000000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone7_15",new Vector3(6.37500000f,-0.00000000f,0.37500000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone8_16",new Vector3(0.62499976f,3.37500000f,-3.12499952f),new Quaternion(-0.00000010f,0.48982108f,-0.00000005f,-0.87182298f)),
new RestBonePose("Bone9_17",new Vector3(0.99999976f,-0.00000027f,-5.50000000f),new Quaternion(0.00000015f,-0.99831338f,0.00000026f,0.05805517f)),
new RestBonePose("Bone10_18",new Vector3(2.12499976f,-0.00000007f,-1.74999988f),new Quaternion(-0.00000002f,-0.37859426f,0.00000003f,0.92556274f)),
new RestBonePose("Bone11_19",new Vector3(0.12500070f,-0.00000022f,-3.37500000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone13_20",new Vector3(0.62500024f,-2.87500000f,-3.12500048f),new Quaternion(-0.00000010f,0.48982099f,-0.00000005f,-0.87182303f)),
new RestBonePose("Bone14_21",new Vector3(1.00000119f,-0.00000027f,-5.50000095f),new Quaternion(0.00000015f,-0.99831338f,0.00000026f,0.05805510f)),
new RestBonePose("Bone15_22",new Vector3(2.12500024f,-0.00000007f,-1.75000000f),new Quaternion(-0.00000002f,-0.37859436f,0.00000003f,0.92556270f)),
new RestBonePose("Bone16_23",new Vector3(0.12500022f,-0.00000022f,-3.37499976f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f)),
new RestBonePose("Bone24_24",new Vector3(3.87500000f,-0.00000000f,0.62500000f),new Quaternion(-0.00000007f,0.51326083f,-0.00000003f,-0.85823267f)),
new RestBonePose("Bone25_25",new Vector3(1.75000000f,0.00000000f,-5.75000000f),new Quaternion(0.00000000f,0.00000000f,0.00000000f,-1.00000000f))
 };
 readonly List<(Transform bone,Vector3 position,Quaternion rotation,RestBonePose target)> restBones=new();
 bool restPoseActive;float restPoseBlend,restBreathingTime;
 bool BeginRestPose(){
  if(dog==null||animation==null)return false;
  var hierarchy=dog.GetComponentsInChildren<Transform>(true);
  foreach(var target in SleepingPose)if(!hierarchy.Any(t=>t.name==target.Name))return false;
  // Sample the neutral idle pose once, then stop the head/locomotion clips.
  StopIdleGesture();animation.enabled=true;animation.Play("Idle");
  var idle=animation["Idle"];if(idle!=null){idle.normalizedTime=0;idle.speed=0;}
  animation.Sample();animation.enabled=false;restBones.Clear();
  foreach(var target in SleepingPose){var bone=hierarchy.First(t=>t.name==target.Name);restBones.Add((bone,bone.localPosition,bone.localRotation,target));}
  restPoseBlend=0;restBreathingTime=0;restPoseActive=true;ResetMotion();return true;
 }
 void ApplyRestPose(){
  if(!restPoseActive||dog==null)return;
  float dt=Mathf.Min(Time.deltaTime,.1f);restPoseBlend=Mathf.Min(1,restPoseBlend+dt/1.5f);restBreathingTime+=dt;
  float blend=restPoseBlend*restPoseBlend*(3-2*restPoseBlend);
  foreach(var item in restBones){
   if(item.bone==null)continue;
   item.bone.localPosition=Vector3.Lerp(item.position,item.target.Position,blend);
   item.bone.localRotation=Quaternion.Slerp(item.rotation,item.target.Rotation,blend);
   // Very small breathing motion, in original skeleton units, below the neck.
   if(item.target.Name=="Bone_2")item.bone.localPosition+=Vector3.up*(Mathf.Sin(restBreathingTime*1.6f)*.035f*blend);
  }
 }
 void EndRestPose(){
  if(!restPoseActive)return;
  foreach(var item in restBones)if(item.bone!=null){item.bone.localPosition=item.position;item.bone.localRotation=item.rotation;}
  restBones.Clear();restPoseActive=false;
  if(animation!=null){animation.enabled=true;playing="";nextHeadMovement=Time.time+HeadGestureInterval;Play("Idle");}
 }
}
