using HarmonyLib;
using Il2Cpp;
using UnityEngine;
namespace BurebistaTrapperDog;
public partial class Main {
 internal bool dogMenu;bool previousCursor;CursorLockMode previousLock;
 void OpenDogMenu(){
  if(dogMenu){CloseDogMenu();return;}
  var player=GameManager.GetPlayerManagerComponent();
  if(player==null||player.GetControlMode()!=PlayerControlMode.Normal||InterfaceManager.GetInstance().AnyOverlayPanelEnabled())return;
  if(dog==null){HUDMessage.AddMessage(DogLanguage.Text("Saca al perro de la mochila: Ctrl+Alt+P"));return;}
  previousCursor=Cursor.visible;previousLock=Cursor.lockState;
  player.SetControlMode(PlayerControlMode.Locked);dogMenu=true;
  KeepMenuCursor();
 }
 void KeepMenuCursor(){
  if(!dogMenu)return;
  InputManager.ShowCursor(true);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 }
 public override void OnLateUpdate(){if(dogMenu)KeepMenuCursor();if(Towing)UpdateTowRig();ApplyRestPose();}
 void SelectDogOrder(int choice){
  if(!dogMenu)return;CloseDogMenu();
  if(Towing&&(choice==1||choice==2)){HUDMessage.AddMessage(DogLanguage.Text("Desengancha primero al perro: Ctrl+Alt+T"));return;}
  if(choice==3||choice==4)ReleaseTow();
  if(choice>=1&&choice<=4)WakeBedRest();
  switch(choice){case 1:StartHunt(false);break;case 2:StartHunt(true);break;case 3:RecallHunt();break;case 4:FinishHunt();following=false;break;}
 }
 void DogMenuKeys(){
  KeepMenuCursor();
  if(Input.GetKeyDown(KeyCode.Alpha1)||Input.GetKeyDown(KeyCode.Keypad1))SelectDogOrder(1);
  else if(Input.GetKeyDown(KeyCode.Alpha2)||Input.GetKeyDown(KeyCode.Keypad2))SelectDogOrder(2);
  else if(Input.GetKeyDown(KeyCode.Alpha3)||Input.GetKeyDown(KeyCode.Keypad3))SelectDogOrder(3);
  else if(Input.GetKeyDown(KeyCode.Alpha4)||Input.GetKeyDown(KeyCode.Keypad4))SelectDogOrder(4);
  else if(Input.GetKeyDown(KeyCode.Alpha5)||Input.GetKeyDown(KeyCode.Keypad5))SelectDogOrder(5);
 }
 void CloseDogMenu(){
  if(!dogMenu)return;dogMenu=false;
  var player=GameManager.GetPlayerManagerComponent();
  if(player!=null&&player.GetControlMode()==PlayerControlMode.Locked)player.SetControlMode(PlayerControlMode.Normal);
  InputManager.ShowCursor(previousCursor);Cursor.lockState=previousLock;Cursor.visible=previousCursor;
 }
 public override void OnGUI(){
  if(!dogMenu)return;
  KeepMenuCursor();
  var old=GUI.color;var w=Mathf.Min(440,Screen.width-30);float x=(Screen.width-w)/2,y=(Screen.height-335)/2;
  GUI.color=new Color(.08f,.1f,.12f,.98f);GUI.Box(new Rect(x,y,w,335),"");GUI.color=old;
  GUI.Label(new Rect(x+20,y+15,w-40,28),DogLanguage.Text("PERRO DE TRINEO - ORDENES"));
  GUI.Label(new Rect(x+20,y+45,w-40,26),carriedPrey!=null?DogLanguage.Text("Trayendo la presa"):huntTarget!=null?DogLanguage.Text("Cazando"):Towing?DogLanguage.Text("Enganchado al trineo (Ctrl+Alt+T para soltar)"):DogLanguage.Text("Preparado"));
  if(GUI.Button(new Rect(x+20,y+80,w-40,38),DogLanguage.Text("1 — Cazar un conejo cercano")))SelectDogOrder(1);
  if(GUI.Button(new Rect(x+20,y+125,w-40,38),DogLanguage.Text("2 — Cazar una perdiz cercana")))SelectDogOrder(2);
  if(GUI.Button(new Rect(x+20,y+170,w-40,38),DogLanguage.Text("3 — Cancelar caza / venir conmigo")))SelectDogOrder(3);
  if(GUI.Button(new Rect(x+20,y+215,w-40,38),DogLanguage.Text("4 — Esperar aqui")))SelectDogOrder(4);
  if(GUI.Button(new Rect(x+20,y+270,w-40,38),DogLanguage.Text("5 — Cerrar")))SelectDogOrder(5);
 }
}

// The native game requests a hidden cursor each frame outside its own panels.
// Override only while our menu owns the player controls; closing restores normal behavior.
[HarmonyPatch(typeof(InputManager),nameof(InputManager.ShowCursor))]
static class DogMenuCursor {
 static void Prefix(ref bool __0){if(Main.Instance!=null&&Main.Instance.dogMenu)__0=true;}
}
