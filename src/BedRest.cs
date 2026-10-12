using Il2Cpp;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;
public partial class Main {
 Bed restingBed;Vector3 restDestination,bedRestOrigin,restForward;float restBlocked,restApproachTime;
 bool Resting=>restingBed!=null;
 static bool UsableDogBed(Bed bed){
  if(bed==null||!bed.gameObject.activeInHierarchy||bed.IsWornOut())return false;
  var gear=bed.GetComponentInParent<GearItem>();
  if(gear!=null&&gear.m_InPlayerInventory)return false;
  // Fixed beds (including mod beds) and any type of placed bedroll.
  return !bed.m_Bedroll||bed.GetState()==BedRollState.Placed;
 }
 void ToggleBedRest(){
  if(Resting||restPoseActive){WakeBedRest();HUDMessage.AddMessage(DogLanguage.Text("Perro: me levanto y voy contigo"));return;}
  if(dog==null){HUDMessage.AddMessage(DogLanguage.Text("Saca primero al perro: Ctrl+Alt+P"));return;}
  if(Towing||huntTarget!=null||carriedPrey!=null){HUDMessage.AddMessage(DogLanguage.Text("Desengancha al perro o termina la caza antes de descansar"));return;}
  var player=GameManager.GetPlayerTransform();if(player==null)return;
  Bed selected=null;float closest=8f;
  foreach(var bed in Object.FindObjectsOfType<Bed>()){
   if(!UsableDogBed(bed))continue;
   float distance=Vector3.Distance(player.position,bed.transform.position);
   if(distance<closest){closest=distance;selected=bed;}
  }
  if(selected==null){HUDMessage.AddMessage(DogLanguage.Text("Necesitas un saco desplegado o una cama cerca (8 m)"));return;}
  if(Vector3.Distance(dog.transform.position,selected.transform.position)>15f){HUDMessage.AddMessage(DogLanguage.Text("Acerca primero al perro al saco"));return;}
  BedRestLayout(selected,out var bedCenter,out var bedForward);
  if(Vector3.Dot(bedForward,dog.transform.forward)<0)bedForward=-bedForward;
  if(!MotionGround(bedCenter,out var place)){HUDMessage.AddMessage(DogLanguage.Text("No encuentro apoyo seguro en el saco"));return;}
  restingBed=selected;restDestination=place;restForward=bedForward;bedRestOrigin=selected.transform.position;
  restBlocked=0;restApproachTime=0;following=true;ResetMotion();
  HUDMessage.AddMessage(DogLanguage.Text("Perro: voy a tumbarme en el saco. Ctrl+Alt+B para volver contigo"));
 }
 void CheckBedRest(){
  if((restingBed==null&&restPoseActive)||(restingBed!=null&&(!UsableDogBed(restingBed)||Vector3.Distance(bedRestOrigin,restingBed.transform.position)>.5f)))WakeBedRest();
 }
 internal void WakeBedRest(){
  bool wasResting=restingBed!=null||restPoseActive;
  restingBed=null;restBlocked=0;restApproachTime=0;EndRestPose();
  if(wasResting){following=true;ResetMotion();}
 }
 bool BedRestTick(){
  if(!Resting)return false;
  CheckBedRest();if(!Resting)return false;
  if(restPoseActive)return true;
  float dt=Mathf.Min(Time.deltaTime,.1f);restApproachTime+=dt;
  var delta=restDestination-dog.transform.position;delta.y=0;
  if(delta.magnitude>.18f){
   bool moved=NaturalMove(restDestination,1.2f,.12f);
   restBlocked=moved?0:restBlocked+dt;
   if(restBlocked>3f||restApproachTime>30f){WakeBedRest();HUDMessage.AddMessage(DogLanguage.Text("No puedo llegar al saco: despeja el paso"));return false;}
   return true;
  }
  // Keep the dog's root on the actual support surface; only its skeleton folds.
  dog.transform.position=restDestination;
  var forward=restForward;
  if(forward.sqrMagnitude>.1f)dog.transform.rotation=Quaternion.LookRotation(forward);
  if(!BeginRestPose()){WakeBedRest();HUDMessage.AddMessage(DogLanguage.Text("No se pudo preparar la postura tumbada del perro"));return false;}
  HUDMessage.AddMessage(DogLanguage.Text("Perro tumbado: Ctrl+Alt+B o llamar con Ctrl+Alt+P para levantarlo"));
  return true;
 }
}
