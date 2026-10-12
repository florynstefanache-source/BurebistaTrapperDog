using Il2CppTLD.BigCarry;
using HarmonyLib;
using Il2Cpp;
using System.Reflection;
namespace BurebistaTrapperDog;
// Keep the condition the sled already has. Other travois retain native decay.
[HarmonyPatch]
static class SledNoWear {
 static IEnumerable<MethodBase> TargetMethods()=>typeof(TravoisBigCarryItem).GetMethods().Where(m=>
  m.Name=="UpdateTimeDecay"||m.Name=="UpdateBlizzardDecay"||m.Name=="UpdateTravelDecay"||m.Name=="ApplyFallDamage");
 static bool Prefix(TravoisBigCarryItem __instance)=>!MotionRules.IsBureSled(__instance.name);
}
 

