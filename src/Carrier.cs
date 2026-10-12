using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;
internal static class Carrier {
 internal const string Name="GEAR_BurebistaTrapperDog",Native="GEAR_HighQualityTools";
 static GearItem template;static GearItemData data;
 internal static bool Is(string name)=>(name??"").Replace("(Clone)","").Trim()==Name;
 internal static GearItem Template(){if(template!=null)return template;
  var native=GearItem.LoadGearItemPrefab(Native);if(native==null)throw new Exception("No hay plantilla del transportin");
  var root=Object.Instantiate(native.gameObject);root.SetActive(false);Object.DontDestroyOnLoad(root);
  data=Object.Instantiate(native.m_GearItemData);Object.DontDestroyOnLoad(data);data.name="TrapperDogCarrierData";
  data.m_PickupAudio=new Il2CppAK.Wwise.Event();data.m_StowAudio=new Il2CppAK.Wwise.Event();data.m_PutBackAudio=new Il2CppAK.Wwise.Event();
  data.m_PrefabReference=new AssetReferenceGearItem(Name);data.m_BaseWeight=ItemWeight.FromKilograms(10);
  data.m_LocalizedName=new LocalizedString{m_LocalizationID="GAMEPLAY_TrapperDogCarrier"};data.m_LocalizedDescription=new LocalizedString{m_LocalizationID="GAMEPLAY_TrapperDogCarrierDesc"};
  var icon=ScriptableObject.CreateInstance<GearItemInventoryIconSimpleData>();icon.m_Icon=Main.Instance.CarrierIcon();icon.hideFlags|=HideFlags.DontUnloadUnusedAsset;data.m_IconData=icon;
  template=root.GetComponent<GearItem>();Configure(template);return template;
 }
 internal static void Configure(GearItem gear){if(gear==null)return;if(data==null)Template();gear.name=Name;gear.m_GearItemData=data;gear.WeightKG=ItemWeight.FromKilograms(10);gear.m_StackableItem=null;gear.m_Harvest=null;
  if(gear.transform.Find("DogInventoryVisual")!=null)return;
  foreach(var renderer in gear.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
  var visual=Object.Instantiate(Main.Instance.DogPrefab(),gear.transform);visual.name="DogInventoryVisual";visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one*.5f;
 }
 internal static GearItem Give(){var gear=Object.Instantiate(Template().gameObject).GetComponent<GearItem>();gear.gameObject.SetActive(true);Configure(gear);var added=GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(gear,false,true);if(added==null||!added.m_InPlayerInventory)throw new Exception(DogLanguage.Text("No se pudo guardar el perro en la mochila"));return added;}
 internal static int Count()=>GameManager.GetInventoryComponent().GetNumGearWithName(Name);
 internal static void Texts(){var table=Localization.s_CurrentLanguageStringTable;if(table==null)return;foreach(var pair in new[]{("GAMEPLAY_TrapperDogCarrier",DogLanguage.Text("Perro de trineo")),("GAMEPLAY_TrapperDogCarrierDesc",DogLanguage.Text("Tu pastor alemán preparado para viajar contigo. Pesa 10 kg. Ctrl+Alt+P: sacarlo en un lugar seguro."))}){var e=table.GetEntryFromKey(pair.Item1)??table.AddEntryForKey(pair.Item1);for(int i=0;i<e.m_Languages.Count;i++)e.m_Languages[i]=pair.Item2;}}
}
[HarmonyPatch(typeof(GearItem),nameof(GearItem.LoadGearItemPrefab))]
static class CarrierPrefab {static bool Prefix(string name,ref GearItem __result){if(!Carrier.Is(name))return true;__result=Carrier.Template();return false;}}
[HarmonyPatch]
static class CarrierFactory {
 static IEnumerable<MethodBase> TargetMethods()=>typeof(GearItem).GetMethods().Where(m=>(m.Name=="InstantiateGearItem"||m.Name=="InstantiateAndDeserializeGearItem")&&m.GetParameters().Length>0&&m.GetParameters()[0].ParameterType==typeof(string));
 static void Prefix(ref string __0,out bool __state){__state=Carrier.Is(__0);if(__state)__0=Carrier.Native;}
 static void Postfix(GearItem __result,bool __state){if(__state)Carrier.Configure(__result);}
}
[HarmonyPatch(typeof(GearItem),nameof(GearItem.CreateSaveData))]
static class CarrierSave {static void Postfix(GearItem __instance,GearSaveData __result){if(Carrier.Is(__instance.name)&&__result!=null)__result.m_PrefabName=Carrier.Name;}}
