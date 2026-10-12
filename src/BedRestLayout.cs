using Il2Cpp;
using UnityEngine;
namespace BurebistaTrapperDog;
public partial class Main {
 // Use the rendered mesh's oriented bounds: a bed's root forward may point
 // across its sleeping surface. Bounds remain readable for native game meshes.
 static void BedRestLayout(Bed bed,out Vector3 center,out Vector3 forward){
  center=bed.transform.position;
  forward=Vector3.ProjectOnPlane(bed.transform.forward,Vector3.up).normalized;
  float bestArea=0;
  foreach(var filter in bed.GetComponentsInChildren<MeshFilter>()){
   if(filter.sharedMesh==null||!filter.gameObject.activeInHierarchy)continue;
   var bounds=filter.sharedMesh.bounds;var points=new List<Vector3>();var average=Vector3.zero;
   foreach(float x in new[]{-1f,1f})foreach(float y in new[]{-1f,1f})foreach(float z in new[]{-1f,1f}){
    var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z));
    var point=filter.transform.TransformPoint(corner);points.Add(point);average+=point;
   }
   average*=.125f;
   float xx=0,xz=0,zz=0;
   foreach(var point in points){var d=point-average;xx+=d.x*d.x;xz+=d.x*d.z;zz+=d.z*d.z;}
   float angle=.5f*Mathf.Atan2(2*xz,xx-zz);
   var axis=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var across=new Vector3(-axis.z,0,axis.x);
   float length=0,width=0;
   foreach(var point in points){var d=point-average;length=Mathf.Max(length,Mathf.Abs(Vector3.Dot(d,axis))*2);width=Mathf.Max(width,Mathf.Abs(Vector3.Dot(d,across))*2);}
   float area=length*width;
   // Ignore thin straps and ambiguous, square decorative parts.
   if(length<.5f||width<.25f||length<width*1.15f||area<=bestArea)continue;
   bestArea=area;center=average;forward=axis;
  }
  if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
 }
}
