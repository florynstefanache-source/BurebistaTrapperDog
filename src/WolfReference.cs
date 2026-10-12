using Il2Cpp;
using UnityEngine;
using MelonLoader;
namespace BurebistaTrapperDog;
public partial class Main {
 float walkSpeed=1.4f,runSpeed=5f,turnSpeed=110f,currentSpeed;
 void MatchWolf(){
  dog.transform.localScale=Vector3.one;
  // Use a loaded regular wolf, excluding timberwolves and other species.
  foreach(var ai in Resources.FindObjectsOfTypeAll<BaseAi>()){
   if(ai==null||ai.NormalWolf==null)continue;
   var wolf=ai.GetComponentInChildren<SkinnedMeshRenderer>();
   var body=dog.GetComponentInChildren<SkinnedMeshRenderer>();
   if(wolf==null||body==null||wolf.sharedMesh==null||body.sharedMesh==null)continue;
   var w=Vector3.Scale(wolf.sharedMesh.bounds.size,wolf.transform.lossyScale);
   var d=Vector3.Scale(body.sharedMesh.bounds.size,body.transform.lossyScale);
   if(w.y<.4f||w.y>3||d.y<.1f)continue;
   float height=Mathf.Clamp(w.y/d.y,.7f,2.5f);
   float length=Mathf.Clamp(Mathf.Max(w.x,w.z)/Mathf.Max(d.x,d.z),.7f,2.5f);
   dog.transform.localScale=new Vector3(height,height,length);
   if(ai.m_RunSpeed>1f)runSpeed=Mathf.Clamp(ai.m_RunSpeed,3f,9f);
   if(ai.m_WalkSpeed>.1f)walkSpeed=Mathf.Clamp(ai.m_WalkSpeed,.5f,3f);
   if(ai.m_TurnSpeedDegreesPerSecond>10)turnSpeed=Mathf.Clamp(ai.m_TurnSpeedDegreesPerSecond,45,240);
   MelonLogger.Msg("Dog: dimensiones y velocidad de lobo normal aplicadas; escala="+dog.transform.localScale+", marcha="+walkSpeed);return;
  }
  dog.transform.localScale=Vector3.one*1.45f;
  MelonLogger.Msg("Dog: no hay lobo normal cargado; escala provisional 1.45. Vuelve a llamarlo cerca de una zona con lobos para ajustar.");
 }
}

