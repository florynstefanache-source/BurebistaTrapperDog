using System.Reflection;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object=UnityEngine.Object;
[assembly:MelonInfo(typeof(BurebistaTrapperDog.Main),"Burebista Trapper Dog","0.6.5","Burebista")]
[assembly:MelonGame("Hinterland","TheLongDark")]
namespace BurebistaTrapperDog;
public partial class Main:MelonMod {
 AssetBundle bundle;GameObject prefab,dog;Animation animation;bool following=true;string playing="";Vector3 lastPlayerPosition;bool havePlayerPosition;float playerSpeed;
 public override void OnInitializeMelon(){Instance=this;MelonLogger.Msg("Dog 0.6.4: Ctrl+Alt+P llamar; Ctrl+Alt+O seguir/esperar. Ctrl+Alt+L recoger (10 kg). Ctrl+Alt+H: menu de caza. Ctrl+Alt+T: enganche Bure Trapper Sled. Ctrl+Alt+B: tumbarse en saco/levantarse. Guardado con ModData.");}
 void Load(){if(prefab!=null)return;string p=Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"BurebistaTrapperDog","trapperdog");bundle=AssetBundle.LoadFromFile(p);if(bundle==null)throw new Exception("Falta el bundle trapperdog");prefab=bundle.LoadAsset<GameObject>("Assets/Dog/Dog.prefab");if(prefab==null)throw new Exception("Falta Dog.prefab");prefab.hideFlags|=HideFlags.DontUnloadUnusedAsset;ApplyShepherdAppearance(prefab);}
 bool Ground(Vector3 position,out Vector3 ground){ground=position;if(!Physics.Raycast(position+Vector3.up*2,Vector3.down,out RaycastHit hit,5f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))return false;if(hit.normal.y<.65f)return false;ground=hit.point;return true;}
 bool FindPlace(Transform target,out Vector3 place){
  var forward=Vector3.ProjectOnPlane(target.forward,Vector3.up).normalized;
  if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
  foreach(float distance in new[]{3f,2f,1.5f})foreach(float angle in new[]{0f,45f,-45f,90f,-90f,180f}){
   var candidate=target.position+(Quaternion.Euler(0,angle,0)*forward)*distance;
   if(Ground(candidate,out place)&&Mathf.Abs(place.y-target.position.y)<2.5f)return true;
  }
  place=target.position;MelonLogger.Warning("Dog: no se encontro suelo cerca del jugador real en "+target.position);return false;
 }
 void Play(string name){
 if(animation==null)return;
 if(name=="Idle")name=IdleAnimation();else StopIdleGesture();
 if(playing==name)return;
 bool oldGait=playing=="Walk"||playing=="Run",newGait=name=="Walk"||name=="Run";
 if(oldGait&&newGait&&animation[playing]!=null&&animation[name]!=null)
  animation[name].normalizedTime=animation[playing].normalizedTime%1f;
 animation.CrossFade(name,.22f);playing=name;
}
 public override void OnUpdate(){voice.Poll();try{
 if(GameManager.GetPlayerManagerComponent()!=null&&DogLanguage.Refresh())Carrier.Texts();
 InventorySoundTick();CheckBedRest();
 if(Towing&&!TowIsValid())ReleaseTow();
 if(dogMenu){PauseMotion();var owner=GameManager.GetPlayerManagerComponent();if(owner==null||owner.GetControlMode()!=PlayerControlMode.Locked||InterfaceManager.GetInstance().AnyOverlayPanelEnabled()||Input.GetKeyDown(KeyCode.Escape)||(Input.GetKey(KeyCode.LeftControl)&&Input.GetKey(KeyCode.LeftAlt)&&Input.GetKeyDown(KeyCode.H)))CloseDogMenu();else DogMenuKeys();return;}
 var player=GameManager.GetPlayerManagerComponent();if(player==null||Time.timeScale<=0)return;if(player.PlayerIsClimbing()){WakeBedRest();ReleaseTow();return;}var target=GameManager.GetPlayerTransform();if(target==null)return;
 if(!EnsureState())return;SoundTick();
 if(player.GetControlMode()!=PlayerControlMode.Normal||InterfaceManager.GetInstance().AnyOverlayPanelEnabled()){PauseMotion();return;}
 var travelled=target.position-lastPlayerPosition;travelled.y=0;
 if(havePlayerPosition&&Time.deltaTime>.0001f&&travelled.magnitude<10)
  playerSpeed=Mathf.Lerp(playerSpeed,Mathf.Min(travelled.magnitude/Time.deltaTime,12f),Mathf.Clamp01(Time.deltaTime*8));
 else playerSpeed=0;
 lastPlayerPosition=target.position;havePlayerPosition=true;
 bool modifier=Input.GetKey(KeyCode.LeftControl)&&Input.GetKey(KeyCode.LeftAlt);
 if(modifier&&Input.GetKeyDown(KeyCode.B)){ToggleBedRest();return;}
 if(modifier&&Input.GetKeyDown(KeyCode.T)){WakeBedRest();ToggleTow();return;}
 if(modifier&&Input.GetKeyDown(KeyCode.H)){OpenDogMenu();return;}
 if(modifier&&Input.GetKeyDown(KeyCode.P)){
 if(Resting||restPoseActive){WakeBedRest();following=true;HUDMessage.AddMessage(DogLanguage.Text("Perro: voy contigo"));return;}
 if(Towing)ReleaseTow();
 if(huntTarget!=null||carriedPrey!=null){RecallHunt();return;}
  Carrier.Texts();
  if(state.Packed&&Carrier.Count()==0){HUDMessage.AddMessage(DogLanguage.Text("Recoge el perro de tu inventario o contenedor antes de sacarlo"));}
  else if(dog!=null&&Vector3.Distance(target.position,dog.transform.position)>5f){following=true;Say("dog_bark1");HUDMessage.AddMessage(DogLanguage.Text("Perro: voy hacia ti"));}
  else if(FindPlace(target,out var place)){
   bool carried=Carrier.Count()>0;
   Spawn(place);
   if(carried){GameManager.GetInventoryComponent().RemoveGearFromInventory(Carrier.Name,1,true);if(Carrier.Count()>0){Object.Destroy(dog);dog=null;state.Packed=true;throw new Exception(DogLanguage.Text("No se pudo retirar el perro de la mochila"));}}
   Capture();Say(UnityEngine.Random.value>.5f?"dog_bark1":"dog_bark2");HUDMessage.AddMessage(DogLanguage.Text("Perro: te sigo"));
  }else HUDMessage.AddMessage(DogLanguage.Text("No encuentro suelo seguro para el perro"));
 }
 if(modifier&&Input.GetKeyDown(KeyCode.L)){PackDog(target);return;} if(dog==null)return;
 if(modifier&&Input.GetKeyDown(KeyCode.O)){WakeBedRest();ReleaseTow();FinishHunt();following=!following;HUDMessage.AddMessage(following?DogLanguage.Text("Perro: te sigo"):DogLanguage.Text("Perro: me quedo aqui"));}
 if(HuntingTick(target))return;
 if(BedRestTick())return;
 if(TowTick())return;
 FollowMotion(target,player.PlayerIsSprinting());
 }catch(Exception e){MelonLogger.Error("Dog: "+e);WakeBedRest();ReleaseTow();CloseDogMenu();FinishHunt();if(dog!=null)Object.Destroy(dog);dog=null;}}
}








