using System.Reflection;
using MelonLoader;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;
public partial class Main {
 Texture2D shepherdTexture,shepherdIcon;bool iconAttempted;
 Texture2D ShepherdIcon(){
  if(iconAttempted)return shepherdIcon;iconAttempted=true;
  try{
   var path=Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"BurebistaTrapperDog","GermanShepherdIcon.png");
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,true);
   if(!ImageConversion.LoadImage(texture,File.ReadAllBytes(path),false)){Object.Destroy(texture);throw new Exception("PNG de icono no válido");}
   texture.name="BureGermanShepherdInventoryIcon";texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Trilinear;texture.hideFlags|=HideFlags.DontUnloadUnusedAsset;shepherdIcon=texture;
  }catch(Exception error){MelonLogger.Warning("Perro: no se pudo cargar el icono transparente: "+error.Message);}
  return shepherdIcon;
 }
 void ApplyShepherdAppearance(GameObject target){
  // Apply to our prefab only. Instances, including the inventory visual,
  // inherit the same material without touching any native animal assets.
  try{
   string path=Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"BurebistaTrapperDog","GermanShepherd.png");
   if(!File.Exists(path))throw new FileNotFoundException("Falta GermanShepherd.png; copia también la carpeta del mod",path);
   var body=target.GetComponentInChildren<SkinnedMeshRenderer>(true);
   if(body==null||body.sharedMaterial==null)throw new Exception("No se encontró el material del perro");
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,true);
   if(!ImageConversion.LoadImage(texture,File.ReadAllBytes(path),false)){Object.Destroy(texture);throw new Exception("No se pudo leer la textura del pastor alemán");}
   texture.name="BureGermanShepherdAlbedo";texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=4;
   texture.hideFlags|=HideFlags.DontUnloadUnusedAsset;
   var material=new Material(body.sharedMaterial);material.name="BureGermanShepherdMaterial";
   material.mainTexture=texture;material.color=Color.white;
   if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",0);
   if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",.06f);
   material.hideFlags|=HideFlags.DontUnloadUnusedAsset;body.sharedMaterial=material;shepherdTexture=texture;
   MelonLogger.Msg("Perro: pelaje de pastor alemán aplicado ("+texture.width+" x "+texture.height+")");
  }catch(Exception error){MelonLogger.Warning("Perro: se conserva el aspecto anterior: "+error.Message);}
 }
}
