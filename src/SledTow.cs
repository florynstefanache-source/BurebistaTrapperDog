using Il2Cpp;
using Il2CppTLD.BigCarry;
using UnityEngine;
using MelonLoader;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;

public partial class Main {
 TravoisBigCarryItem towSled;Transform towAnchor;
 GameObject towRig;Material towMaterial;LineRenderer trace,strap;
 Vector3 towHeading;bool towAttached,towReady,towBlockedNotice;float towBlocked;
 bool IsTowCollider(Collider collider)=>towSled!=null&&collider!=null&&collider.transform.IsChildOf(towSled.transform);
 bool Towing=>towAttached;
 void ToggleTow(){
  if(Towing){ReleaseTow();HUDMessage.AddMessage(DogLanguage.Text("Perro desenganchado: te sigo"));return;}
  if(dog==null){HUDMessage.AddMessage(DogLanguage.Text("Saca primero el perro: Ctrl+Alt+P"));return;}
  if(huntTarget!=null||carriedPrey!=null){HUDMessage.AddMessage(DogLanguage.Text("Termina la caza y la entrega antes de enganchar"));return;}
  var system=BigCarrySystem.Instance;
  if(system==null||system.IsDroppingItem||system.CarriedItem!=null){
   HUDMessage.AddMessage(DogLanguage.Text("Deja el trineo en el suelo antes de enganchar al perro"));return;
  }
  TravoisBigCarryItem sled=null;float nearest=5f;
  foreach(var candidate in Object.FindObjectsOfType<TravoisBigCarryItem>()){
   if(candidate.IsCarried||!candidate.gameObject.activeInHierarchy||!MotionRules.IsBureSled(candidate.name)||candidate.m_CurrentHP<=0)continue;
   float distance=Vector3.Distance(candidate.transform.position,dog.transform.position);
   if(distance<nearest){nearest=distance;sled=candidate;}
  }
  if(sled==null){HUDMessage.AddMessage(DogLanguage.Text("Acerca el perro a tu Trapper Sled desplegado (menos de 5 m)"));return;}
  var anchor=sled.transform.Find("BurebistaSledVisual/DogTowAnchor");
  if(anchor==null){HUDMessage.AddMessage(DogLanguage.Text("El trineo no tiene DogTowAnchor; usa BurebistaTrapperSled 2.0.9"));return;}
  if(Vector3.Distance(dog.transform.position,anchor.position)>5f){HUDMessage.AddMessage(DogLanguage.Text("Acerca el perro a menos de 5 m del enganche"));return;}
  towReady=false;towAttached=true;towSled=sled;towAnchor=anchor;towHeading=Vector3.ProjectOnPlane(anchor.forward,Vector3.up).normalized;
  if(towHeading.sqrMagnitude<.1f)towHeading=Vector3.forward;
  ResetMotion();following=true;towBlocked=0;towBlockedNotice=false;
  try{CreateTowRig();}catch(Exception e){ClearTowRig();MelonLogger.Warning("Enganche activo sin cuerdas visibles: "+e.Message);}
  HUDMessage.AddMessage(DogLanguage.Text("Perro enganchado: camina delante para guiarlo. El perro tira. Ctrl+Alt+T para soltar"));
 }
 void ClearTowRig(){
  ClearLeatherHarness();
  if(towRig!=null)Object.Destroy(towRig);towRig=null;trace=null;strap=null;
  if(towMaterial!=null)Object.Destroy(towMaterial);towMaterial=null;
 }
 internal void ReleaseTow(){
  towReady=false;towAttached=false;towSled=null;towAnchor=null;towBlocked=0;towBlockedNotice=false;
  ClearTowRig();ResetMotion();
 }
 void CreateTowRig(){
  var shader=Shader.Find("Standard")??Shader.Find("Sprites/Default")??Shader.Find("Unlit/Color");
  if(shader==null)return;
  towMaterial=new Material(shader){name="BureDogHarnessMaterial",color=new Color(.28f,.19f,.1f,1)};
  towRig=new GameObject("BureDogTowRig");
  trace=NewTowLine("BureDogTowLine",.014f,17);strap=NewTowLine("BureDogChestStrap",.035f,17);
  CreateLeatherHarness();
 }
 LineRenderer NewTowLine(string name,float width,int points){
  var go=new GameObject(name);go.transform.SetParent(towRig.transform,false);
  var line=go.AddComponent<LineRenderer>();line.sharedMaterial=towMaterial;
  line.useWorldSpace=true;line.startWidth=width;line.endWidth=width;line.positionCount=points;
  line.startColor=Color.white;line.endColor=Color.white;return line;
 }
 bool TowIsValid(){
  var system=BigCarrySystem.Instance;
  return dog!=null&&towSled!=null&&towAnchor!=null&&towSled.gameObject.activeInHierarchy&&
   system!=null&&!system.IsDroppingItem&&system.CarriedItem==null&&!towSled.IsCarried&&towSled.m_CurrentHP>0&&MotionRules.IsBureSled(towSled.name);
 }
 bool TowTick(){
  if(!Towing)return false;
  if(!TowIsValid()){ReleaseTow();HUDMessage.AddMessage(DogLanguage.Text("Enganche soltado; el perro vuelve a seguirte"));return false;}
  float dt=Mathf.Min(Time.deltaTime,.1f);
  if(dt<=0)return true;
  var player=GameManager.GetPlayerTransform();if(player==null)return true;
  if(Vector3.Distance(player.position,dog.transform.position)>25f){PauseMotion();UpdateTowRig();return true;}
  var root=towSled.transform;
  if(!towReady){
   var staging=towAnchor.position+towHeading*2.8f;
   var gap=staging-dog.transform.position;gap.y=0;
   if(gap.magnitude>.25f){
    bool approached=NaturalMove(staging,1.2f,.12f,true);
    towBlocked=approached?0:towBlocked+dt;
    TowBlockedHint();
    UpdateTowRig();return true;
   }
   towReady=true;towBlocked=0;
  }
  var oldDog=dog.transform.position;var oldDogRotation=dog.transform.rotation;
  bool moved=NaturalMove(player.position,2.2f,3f,true);
  var pull=dog.transform.position-towAnchor.position;pull.y=0;
  bool blocked=false;
  if(pull.magnitude>2.8f){
   var heading=Vector3.ProjectOnPlane(root.forward,Vector3.up).normalized;
   if(heading.sqrMagnitude<.1f)heading=pull.normalized;
   var rotation=Quaternion.RotateTowards(Quaternion.LookRotation(heading),Quaternion.LookRotation(pull.normalized),45f*dt);
   var anchorOffset=Quaternion.Inverse(root.rotation)*(towAnchor.position-root.position);
   var position=dog.transform.position-pull.normalized*2.8f-rotation*anchorOffset;
   position.y=root.position.y;
   if(TowPathClear(root.position,root.rotation,position,rotation,out var grounded,out var terrainRotation)){
    towSled.SetPositionAndRotation(grounded,terrainRotation);
    // Native serialization retains the last dropped pose for an uncarried item.
    towSled.m_LastDroppedPosition=grounded;towSled.m_LastDroppedRotation=terrainRotation;

   }else blocked=true;
  }
  if(blocked){dog.transform.position=oldDog;dog.transform.rotation=oldDogRotation;PauseMotion();moved=false;}
  var toPlayer=player.position-dog.transform.position;toPlayer.y=0;
  towBlocked=(blocked||(!moved&&toPlayer.magnitude>3.5f))?towBlocked+dt:0;
  TowBlockedHint();
  UpdateTowRig();return true;
 }
 void TowBlockedHint(){
  if(towBlocked<=0){towBlockedNotice=false;return;}
  if(towBlocked>2f&&!towBlockedNotice){towBlockedNotice=true;HUDMessage.AddMessage(DogLanguage.Text("Paso bloqueado: el perro y el trineo esperan enganchados. Guíalos por otro lado."));}
 }
 // Use the dog's ground query, then incline the sled rather than requiring
 // every runner contact to lie within a 30 cm horizontal slab.
 bool TowPathClear(Vector3 start,Quaternion oldRotation,Vector3 end,Quaternion rotation,out Vector3 grounded,out Quaternion terrainRotation){
  grounded=end;terrainRotation=rotation;
  if(!MotionGround(end,out var center))return false;
  var supports=new List<TowSupport>();
  foreach(float x in new[]{-.5f,0f,.5f})foreach(float z in new[]{-1.35f,-.65f,0f,.7f,1.35f}){
   var to=center+rotation*new Vector3(x,0,z);
   if(MotionGround(to,out var support))supports.Add(new TowSupport(x,z,support.y-center.y));
  }
  if(TowTerrain.Fit(supports,out float slopeX,out float slopeZ)){
   var normal=(rotation*new Vector3(-slopeX,1,-slopeZ)).normalized;
   terrainRotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(rotation*Vector3.forward,normal),normal);
  }
  grounded=center;
  // Raise the inclined runners only enough to clear bumps. An unsupported
  // corner may bridge a dip, just as the dog can step across uneven ground.
  foreach(var support in supports){
   var offset=terrainRotation*new Vector3(support.X,0,support.Z);
   if(MotionGround(center+offset,out var contact))grounded.y=Mathf.Max(grounded.y,contact.y-offset.y);
  }
  foreach(float x in new[]{-.5f,0f,.5f})foreach(float z in new[]{-1.35f,-.65f,0f,.7f,1.35f}){
   var local=new Vector3(x,0,z);var from=start+oldRotation*local;var to=grounded+terrainRotation*local;
   var displacement=to-from;
   if(displacement.magnitude>.0001f){
    foreach(var hit in Physics.SphereCastAll(from+Vector3.up*.45f,.23f,displacement.normalized,displacement.magnitude+.03f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)){
     if(IsTowCollider(hit.collider)||IgnoreHuntCollider(hit.collider,null)||hit.normal.y>=MotionRules.MinimumGroundNormalY)continue;
     if(hit.point.y<=Mathf.Max(from.y,to.y)+MotionRules.MaximumGroundStep)continue;
     return false;
    }
   }
  }
  return true;
 }
 void UpdateTowRig(){
  if(trace==null||strap==null||dog==null||towAnchor==null)return;
  UpdateLeatherHarness();
 }
}
