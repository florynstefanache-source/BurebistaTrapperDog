using Il2Cpp;
using UnityEngine;
namespace BurebistaTrapperDog;

// Shared by follow and towing. The hunting implementation is deliberately kept intact.
public partial class Main {
 bool locomotionActive,locomotionRun;
 float visualSpeed;
 readonly float[] steeringAngles={0,25,-25,50,-50};
 void ResetMotion(){currentSpeed=0;visualSpeed=0;locomotionActive=false;locomotionRun=false;havePlayerPosition=false;}
 void UpdateGait(float speed,float dt){
  visualSpeed=Mathf.MoveTowards(visualSpeed,speed,8f*dt);
  locomotionRun=MotionRules.RunGait(visualSpeed,locomotionRun);
  float scale=Mathf.Max(.7f,dog.transform.lossyScale.z);
  if(animation!=null&&animation["Walk"]!=null)animation["Walk"].speed=Mathf.Clamp(visualSpeed/(1.4f*scale),.15f,2.1f);
  if(animation!=null&&animation["Run"]!=null)animation["Run"].speed=Mathf.Clamp(visualSpeed/(2.45f*scale),.15f,2.8f);
  Play(visualSpeed>.08f?(locomotionRun?"Run":"Walk"):"Idle");
 }
 void PauseMotion(){if(restPoseActive)return;ResetMotion();if(dog!=null)Play("Idle");}
 bool MotionGround(Vector3 candidate,out Vector3 ground){
  ground=candidate;float best=float.MaxValue;
  foreach(var hit in Physics.RaycastAll(candidate+Vector3.up*1.2f,Vector3.down,3f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)){
   if(IgnoreHuntCollider(hit.collider,null)||IsTowCollider(hit.collider)||hit.collider.GetComponentInParent<BaseAi>()!=null||hit.normal.y<MotionRules.MinimumGroundNormalY)continue;
   if(hit.distance<best){best=hit.distance;ground=hit.point;}
  }
  return best<float.MaxValue;
 }
 bool NaturalMove(Vector3 destination,float maximum,float stopDistance,bool towing=false){
  float dt=Mathf.Min(Time.deltaTime,.1f);
  if(dt<=0||dog==null)return false;
  var start=dog.transform.position;var startRotation=dog.transform.rotation;var delta=destination-start;delta.y=0;
  float remaining=delta.magnitude;
  locomotionActive=MotionRules.ShouldMove(remaining,stopDistance,locomotionActive);
  float goal=locomotionActive?MotionRules.BrakingSpeed(remaining,stopDistance,maximum):0;
  currentSpeed=MotionRules.Approach(currentSpeed,goal,dt);
  bool moved=false;
  if(remaining>stopDistance&&currentSpeed>.01f){
   var desired=delta.normalized;
   foreach(float angle in steeringAngles){
    var direction=Quaternion.Euler(0,angle,0)*desired;
    var rotated=Quaternion.RotateTowards(startRotation,Quaternion.LookRotation(direction),Mathf.Max(turnSpeed,140f)*dt);
    var forward=Vector3.ProjectOnPlane(rotated*Vector3.forward,Vector3.up).normalized;
    float alignment=Mathf.Clamp01(Vector3.Dot(forward,direction));
    float step=Mathf.Min(currentSpeed*dt*alignment,Mathf.Max(0,remaining-stopDistance));
    dog.transform.rotation=rotated;
    if(step<.0001f)break;
    if(!MotionGround(start+forward*step,out var next)||Mathf.Abs(next.y-start.y)>MotionRules.MaximumGroundStep)continue;
    var stepVector=next-start;
    bool blocked=false;
    foreach(var hit in Physics.SphereCastAll(start+Vector3.up*.45f,.23f,stepVector.normalized,stepVector.magnitude+.15f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)){
     if(IgnoreHuntCollider(hit.collider,null)||IsTowCollider(hit.collider)||hit.normal.y>=MotionRules.MinimumGroundNormalY)continue;
     blocked=true;break;
    }
    if(blocked)continue;
    dog.transform.position=next;moved=true;break;
   }
  }
  float actual=(dog.transform.position-start).magnitude/dt;
  if(!moved&&goal>.01f)currentSpeed=Mathf.MoveTowards(currentSpeed,0,10f*dt);
  UpdateGait(actual,dt);
  return moved;
 }
 void FollowMotion(Transform player,bool sprinting){
  var delta=player.position-dog.transform.position;delta.y=0;
  float maximum=sprinting||playerSpeed>walkSpeed+.6f||delta.magnitude>7f
   ?Mathf.Min(10f,Mathf.Max(runSpeed,playerSpeed+.6f)):walkSpeed;
  if(!following){currentSpeed=0;locomotionActive=false;UpdateGait(0,Mathf.Min(Time.deltaTime,.1f));return;}
  NaturalMove(player.position,maximum,2.5f);
 }
}
