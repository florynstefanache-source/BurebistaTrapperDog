using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModData;
using System.Text.Json;
using MelonLoader;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;
public class DogState {public int Version{get;set;}=1;public bool Known{get;set;}public bool Packed{get;set;}public bool Follow{get;set;}=true;public string Scene{get;set;}="";public float X{get;set;}public float Y{get;set;}public float Z{get;set;}}
public partial class Main {
 internal static Main Instance;readonly ModDataManager saved=new ModDataManager("BurebistaTrapperDog",false);
 DogState state=new DogState();bool ready,fault;string slot="";readonly DogAudio voice=new DogAudio();float nextVoice,nextInventoryVoice;bool audioError;
 internal GameObject DogPrefab(){Load();return prefab;}
 internal Texture2D CarrierIcon(){Load();return ShepherdIcon()??bundle.LoadAsset<Texture2D>("Assets/Dog/DogIcon.png");}
 public override void OnSceneWasLoaded(int buildIndex,string sceneName){WakeBedRest();ReleaseTow();CloseDogMenu();FinishHunt();voice.Dispose();ready=false;havePlayerPosition=false;}
 bool EnsureState(){
  if(fault)return false;if(ready)return true;
  var current=SaveGameSystem.GetCurrentSaveName();if(string.IsNullOrEmpty(current)||!saved.Save("1","ready"))return false;
  try {state=JsonSerializer.Deserialize<DogState>(saved.Load("state")??"{}")??new DogState();if(state.Version!=1)throw new Exception("Version de guardado no compatible");slot=current;ready=true;Carrier.Texts();if(Carrier.Count()>0){state.Known=true;state.Packed=true;}
   if(state.Known&&!state.Packed&&dog==null){var t=GameManager.GetPlayerTransform();if(t!=null){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;if(state.Scene==scene)Spawn(new Vector3(state.X,state.Y,state.Z));else if(state.Follow&&FindPlace(t,out var p))Spawn(p);}}
   following=state.Follow;return true;
  }catch(Exception e){fault=true;MelonLogger.Error("Perro: guardado protegido por error: "+e);return false;}
 }
 internal void Capture(){if(!ready||fault||slot!=SaveGameSystem.GetCurrentSaveName())return;try{state.Follow=following;if(dog!=null){var p=dog.transform.position;state.X=p.x;state.Y=p.y;state.Z=p.z;state.Scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;}if(!saved.Save(JsonSerializer.Serialize(state),"state"))throw new Exception("ModData no disponible");}catch(Exception e){MelonLogger.Error("Perro: no se pudo guardar: "+e);}}
 void Spawn(Vector3 place){WakeBedRest();Load();if(dog==null){dog=Object.Instantiate(prefab);animation=dog.GetComponentInChildren<Animation>();playing="";}MatchWolf();dog.transform.position=place;following=true;ResetMotion();StopIdleGesture();nextHeadMovement=Time.time+HeadGestureInterval;idleHeadEnd=0;Play("Idle");state.Known=true;state.Packed=false;SetupVoice();}
 void SetupVoice(){nextVoice=Time.time+UnityEngine.Random.Range(15f,30f);}
 void PlayVoice(string clip,float volume){try{voice.Play(Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location),"BurebistaTrapperDog","Audio",clip+".wav"),volume);}catch(Exception e){if(!audioError){audioError=true;MelonLogger.Error("Perro: no se pudo reproducir el sonido. Copia tambien la carpeta Audio del ZIP. "+e.Message);}}}
 void Say(string clip){if(dog==null)return;var player=GameManager.GetPlayerTransform();if(player==null)return;float distance=Vector3.Distance(player.position,dog.transform.position);if(distance>=25)return;PlayVoice(clip,.7f*Mathf.Clamp01((25-distance)/23f));}
 internal void InventoryBark(){if(Time.realtimeSinceStartup<nextInventoryVoice)return;if(voice.Busy)voice.Dispose();nextInventoryVoice=Time.realtimeSinceStartup+1.5f;PlayVoice(UnityEngine.Random.value>.5f?"dog_bark1":"dog_bark2",.65f);}
 void SoundTick(){if(!restPoseActive&&dog!=null&&!voice.Busy&&Time.time>nextVoice){Say("dog_idle"+UnityEngine.Random.Range(1,4));nextVoice=Time.time+UnityEngine.Random.Range(30f,60f);}}
 public override void OnApplicationQuit(){WakeBedRest();ReleaseTow();CloseDogMenu();FinishHunt();voice.Dispose();}
 void PackDog(Transform player){if(dog==null)return;if(Vector3.Distance(player.position,dog.transform.position)>3.5f){HUDMessage.AddMessage(DogLanguage.Text("Acercate al perro para recogerlo"));return;}if(Carrier.Count()>0){HUDMessage.AddMessage(DogLanguage.Text("Ya hay un perro en tu mochila"));return;}WakeBedRest();ReleaseTow();FinishHunt();Carrier.Give();state.Known=true;state.Packed=true;Capture();Object.Destroy(dog);dog=null;voice.Dispose();HUDMessage.AddMessage(DogLanguage.Text("Perro en la mochila: 10 kg. Ctrl+Alt+P para sacarlo."));}
}
[HarmonyPatch(typeof(SaveGameSystem),nameof(SaveGameSystem.SaveSceneData))]
static class DogSave { [HarmonyPriority(Priority.First)]static void Prefix(){Main.Instance?.FinishHunt();Main.Instance?.Capture();} }



[HarmonyPatch(typeof(Panel_Inventory),nameof(Panel_Inventory.SelectGridItem))]
static class DogInventorySound {
 static void Postfix(InventoryGridItem __0){try{var gear=__0?.GetGearItem();if(gear!=null&&Carrier.Is(gear.name))Main.Instance?.InventoryBark();}catch(Exception e){MelonLogger.Warning("Perro: seleccion de inventario: "+e.Message);}}
}
