using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;

public partial class Main {
 BaseAi huntTarget;GearItem carriedPrey;
 float huntDeadline,blockedTime;bool preyAlerted;
 Vector3 preyGround;Quaternion preyRotation;
 readonly List<(Collider collider,bool enabled)> preyColliders=new();
 readonly List<(Rigidbody body,bool kinematic,bool gravity)> preyBodies=new();
 bool preyWasNonInteractive;

 static bool LivePrey(BaseAi ai,bool bird){
  return ai!=null&&ai.gameObject.activeInHierarchy&&ai.enabled&&ai.m_CurrentHP>0&&ai.GetAiMode()!=AiMode.Dead
   &&ai.GetAiMode()!=AiMode.Disabled&&(bird?ai.Ptarmigan!=null&&!ai.Ptarmigan.m_IsFlying:ai.Rabbit!=null);
 }
 void StartHunt(bool bird){
  if(dog==null)return;
  if(carriedPrey!=null){HUDMessage.AddMessage(DogLanguage.Text("Deja que el perro entregue primero su presa"));return;}
  FinishHunt();BaseAi chosen=null;float best=40f;
  // Only scan the live animal registry when a hunt is requested, never every frame.
  if(BaseAiManager.m_BaseAis==null){HUDMessage.AddMessage(DogLanguage.Text("No hay animales disponibles en esta zona"));return;}
  foreach(var ai in BaseAiManager.m_BaseAis){
   if(!LivePrey(ai,bird))continue;
   float distance=Vector3.Distance(dog.transform.position,ai.transform.position);
   if(distance<best&&ClearPath(dog.transform.position+Vector3.up*.45f,ai.transform.position+Vector3.up*.25f,ai)){
    chosen=ai;best=distance;
   }
  }
  if(chosen==null){HUDMessage.AddMessage(bird?DogLanguage.Text("No veo perdices en el suelo a menos de 40 m"):DogLanguage.Text("No veo conejos a menos de 40 m"));return;}
  MelonLogger.Msg("Dog hunt: persiguiendo "+chosen.name);
  huntTarget=chosen;huntDeadline=Time.time+45;blockedTime=0;preyAlerted=false;following=true;
  HUDMessage.AddMessage(bird?DogLanguage.Text("Perro: a por la perdiz"):DogLanguage.Text("Perro: a por el conejo"));
 }
 internal void RecallHunt(){huntTarget=null;following=true;blockedTime=0;huntDeadline=Time.time+90;Say("dog_bark1");HUDMessage.AddMessage(carriedPrey!=null?DogLanguage.Text("Perro: vuelvo con la presa"):DogLanguage.Text("Perro: vuelvo contigo"));}

 bool IgnoreHuntCollider(Collider c,BaseAi prey){
  if(c==null||c.isTrigger)return true;
  if(dog!=null&&c.transform.IsChildOf(dog.transform))return true;
  if(carriedPrey!=null&&c.transform.IsChildOf(carriedPrey.transform))return true;
  if(prey!=null&&c.GetComponentInParent<BaseAi>()==prey)return true;
  var player=GameManager.GetPlayerTransform();return player!=null&&c.transform.IsChildOf(player);
 }
 bool ClearPath(Vector3 from,Vector3 to,BaseAi prey){
  var delta=to-from;if(delta.sqrMagnitude<.0001f)return true;
  foreach(var hit in Physics.SphereCastAll(from,.07f,delta.normalized,delta.magnitude,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
   if(!IgnoreHuntCollider(hit.collider,prey))return false;
  return true;
 }
 bool HuntGround(Vector3 candidate,out Vector3 ground){
  ground=candidate;float best=float.MaxValue;
  foreach(var hit in Physics.RaycastAll(candidate+Vector3.up*1.2f,Vector3.down,3f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)){
   if(IgnoreHuntCollider(hit.collider,huntTarget)||hit.collider.GetComponentInParent<BaseAi>()!=null||hit.normal.y<.65f)continue;
   if(hit.distance<best){best=hit.distance;ground=hit.point;}
  }
  return best<float.MaxValue;
 }
 bool HuntMove(Vector3 destination,float speed,float stopDistance){
  Vector3 delta=destination-dog.transform.position;delta.y=0;
  if(delta.magnitude<=stopDistance){Play("Idle");return true;}
  float dt=Mathf.Min(Time.deltaTime,.05f),step=Mathf.Min(speed*dt,delta.magnitude-stopDistance);
  foreach(float angle in new[]{0f,30f,-30f,60f,-60f,85f,-85f}){
   var direction=Quaternion.Euler(0,angle,0)*delta.normalized;
   var start=dog.transform.position;bool blocked=false;
   foreach(var hit in Physics.SphereCastAll(start+Vector3.up*.4f,.23f,direction,step+.2f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
    if(!IgnoreHuntCollider(hit.collider,huntTarget)){blocked=true;break;}
   if(blocked||!HuntGround(start+direction*step,out var next)||Mathf.Abs(next.y-start.y)>.32f)continue;
   dog.transform.position=next;dog.transform.rotation=Quaternion.RotateTowards(dog.transform.rotation,Quaternion.LookRotation(direction),220*dt);
   if(animation!=null&&animation["Run"]!=null)animation["Run"].speed=Mathf.Clamp(speed/5,.7f,1.6f);
   Play("Run");return true;
  }
  Play("Idle");return false;
 }
 bool HuntingTick(Transform player){
  if(huntTarget==null&&carriedPrey==null)return false;
  try{
   if(carriedPrey!=null){
    // Keep one native, scene-owned carcass. Never create a second delivery item.
    if(carriedPrey.m_InPlayerInventory){RestorePreyPhysics();carriedPrey=null;return false;}
    if(Vector3.Distance(dog.transform.position,player.position)<2.2f){DropPrey();HUDMessage.AddMessage(DogLanguage.Text("Tu perro te ha traido la presa"));Play("Idle");return true;}
    if(Time.time>huntDeadline){DropPrey();HUDMessage.AddMessage(DogLanguage.Text("El perro ha dejado la presa en el suelo; no encontraba paso"));return false;}
    HuntMove(player.position,Mathf.Max(4,runSpeed*.8f),1.7f);
    if(HuntGround(dog.transform.position,out var ground))preyGround=ground;
    carriedPrey.transform.position=dog.transform.TransformPoint(new Vector3(0,.55f,.62f));
    carriedPrey.transform.rotation=dog.transform.rotation*Quaternion.Euler(0,90,0);
    return true;
   }
   bool bird=huntTarget.Ptarmigan!=null;
   if(!LivePrey(huntTarget,bird)||Time.time>huntDeadline||Vector3.Distance(player.position,huntTarget.transform.position)>70){
    huntTarget=null;HUDMessage.AddMessage(DogLanguage.Text("La presa se ha escapado; el perro vuelve contigo"));return false;
   }
   float distance=Vector3.Distance(dog.transform.position,huntTarget.transform.position);
   if(distance<7&&!preyAlerted&&huntTarget.GetAiMode()!=AiMode.Stunned){
    preyAlerted=true;huntTarget.SetFleeReason(BaseAi.AiFleeReason.ThrownStone);huntTarget.FleeFrom(dog.transform);huntTarget.SetAiMode(AiMode.Flee);
   }
   if(distance<.95f&&Mathf.Abs(dog.transform.position.y-huntTarget.transform.position.y)<.65f&&ClearPath(dog.transform.position+Vector3.up*.4f,huntTarget.transform.position+Vector3.up*.25f,huntTarget)){
    var prey=huntTarget;huntTarget=null; // Consume the attempt before native calls or callbacks.
    if(UnityEngine.Random.value>(bird?.5f:.75f)){
     prey.FleeFrom(dog.transform);prey.SetAiMode(AiMode.Flee);HUDMessage.AddMessage(DogLanguage.Text("La presa ha esquivado al perro"));return false;
    }
    CapturePrey(prey);return true;
   }
   if(HuntMove(huntTarget.transform.position,Mathf.Clamp(runSpeed*1.25f,5,8),.55f))blockedTime=0;else blockedTime+=Time.deltaTime;
   if(blockedTime>4){huntTarget=null;HUDMessage.AddMessage(DogLanguage.Text("El perro no encuentra paso hacia la presa"));}
   return true;
  }catch(Exception e){MelonLogger.Error("Dog hunt: "+e);FinishHunt();HUDMessage.AddMessage(DogLanguage.Text("Caza cancelada; revisa Latest.log"));return false;}
 }
 void CapturePrey(BaseAi prey){
  prey.ApplyDamage(Mathf.Max(200,prey.m_CurrentHP+1),0,DamageSource.Wolf,"");
  if(prey.m_CurrentHP>0&&prey.GetAiMode()!=AiMode.Dead)return;
  var carcass=prey.SpawnCarcassPickup();
  if(carcass==null){HUDMessage.AddMessage(DogLanguage.Text("La presa ha quedado en el suelo"));return;}
  try{if(prey!=null)prey.OnCarcassPickedUp();}
  catch{Object.Destroy(carcass.gameObject);throw;}
  MelonLogger.Msg("Dog hunt: carcasa nativa "+carcass.name);
  carriedPrey=carcass;preyGround=carcass.transform.position;preyRotation=carcass.transform.rotation;
  preyWasNonInteractive=carcass.NonInteractive;carcass.NonInteractive=true;
  foreach(var c in carcass.GetComponentsInChildren<Collider>(true)){preyColliders.Add((c,c.enabled));c.enabled=false;}
  foreach(var body in carcass.GetComponentsInChildren<Rigidbody>(true)){preyBodies.Add((body,body.isKinematic,body.useGravity));body.isKinematic=true;body.useGravity=false;}
  huntDeadline=Time.time+90;following=true;HUDMessage.AddMessage(DogLanguage.Text("Presa capturada; el perro te la trae"));
 }
 void RestorePreyPhysics(){
  foreach(var entry in preyColliders)if(entry.collider!=null)entry.collider.enabled=entry.enabled;
  foreach(var entry in preyBodies)if(entry.body!=null){entry.body.isKinematic=entry.kinematic;entry.body.useGravity=entry.gravity;}
  preyColliders.Clear();preyBodies.Clear();if(carriedPrey!=null)carriedPrey.NonInteractive=preyWasNonInteractive;
 }
 void DropPrey(){
  if(carriedPrey==null){RestorePreyPhysics();return;}
  var item=carriedPrey;
  if(dog!=null&&HuntGround(dog.transform.position+dog.transform.forward*.6f,out var ground))preyGround=ground;
  item.transform.position=preyGround+Vector3.up*.15f;item.transform.rotation=preyRotation;
  float bottom=float.MaxValue;
  foreach(var renderer in item.GetComponentsInChildren<Renderer>())if(renderer.enabled)bottom=Mathf.Min(bottom,renderer.bounds.min.y);
  if(bottom<float.MaxValue)item.transform.position+=Vector3.up*(preyGround.y+.015f-bottom);
  RestorePreyPhysics();carriedPrey=null;
 }
 internal void FinishHunt(){huntTarget=null;DropPrey();blockedTime=0;}
}
