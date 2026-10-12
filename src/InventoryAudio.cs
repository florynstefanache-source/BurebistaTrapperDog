using HarmonyLib;
using Il2Cpp;
using MelonLoader;
namespace BurebistaTrapperDog;

public partial class Main {
 int selectedInventoryItem;
 void InventorySoundTick(){
  if(InterfaceManager.GetInstance()==null)return;
  if(!InterfaceManager.IsPanelEnabled<Panel_Inventory>()){selectedInventoryItem=0;return;}
  var panel=InterfaceManager.GetPanel<Panel_Inventory>();
  var item=panel?.GetCurrentlySelectedItem()?.m_GearItem;
  int selected=item!=null?item.GetInstanceID():0;
  if(selected==selectedInventoryItem)return;
  selectedInventoryItem=selected;
  if(item!=null&&Carrier.Is(item.name))InventoryBark();
 }
}

// Inventory mouse, keyboard and controller selections all reach OnSelect.
[HarmonyPatch(typeof(Panel_Inventory),nameof(Panel_Inventory.OnSelect))]
static class DogInventorySelect {
 static void Postfix(Panel_Inventory __instance){
  try {var gear=__instance.GetCurrentlySelectedItem()?.m_GearItem;
   if(gear!=null&&Carrier.Is(gear.name))Main.Instance?.InventoryBark();
  }catch(Exception e){MelonLogger.Warning("Dog inventory audio: "+e.Message);}
 }
}
[HarmonyPatch(typeof(GearItem),nameof(GearItem.PlayPickUpClip))]
static class DogPickupAudio {
 static bool Prefix(GearItem __instance){if(!Carrier.Is(__instance.name))return true;Main.Instance?.InventoryBark();return false;}
}
[HarmonyPatch]
static class DogGearSound {
 static IEnumerable<System.Reflection.MethodBase> TargetMethods()=>typeof(GearItem).GetMethods().Where(m=>m.Name=="PlaySound"||m.Name=="Play3DSound");
 static bool Prefix(GearItem __instance,ref uint __result){if(!Carrier.Is(__instance.name))return true;Main.Instance?.InventoryBark();__result=0;return false;}
}
