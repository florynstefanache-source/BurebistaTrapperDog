using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaTrapperDog;
public partial class Main {
 GameObject leatherBlanket;Mesh leatherMesh;Texture2D leatherTexture;Material leatherMaterial;
 LineRenderer rightTrace,bellyBand,leftEdge,rightEdge,leftRing,rightRing,towBridle;
 GameObject towCrossbar;Mesh crossbarMesh;Material crossbarMaterial;
 float[] blanketTop,blanketWidth;const int BlanketRows=7,BlanketColumns=17;
 const float BlanketRear=-.30f,BlanketFront=.16f;
 Vector3 BlanketPoint(int row,float angle,float lift=0){
  float z=Mathf.Lerp(BlanketRear,BlanketFront,row/(float)(BlanketRows-1));
  float top=blanketTop[row];float radius=.145f;
  return new Vector3(Mathf.Sin(angle)*(blanketWidth[row]+lift),top-radius+Mathf.Cos(angle)*(radius+lift),z);
 }
 Vector3 TackPoint(Vector3 local)=>leatherBlanket.transform.TransformPoint(local);
 void CreateLeatherHarness(){
  blanketTop=new float[BlanketRows];blanketWidth=new float[BlanketRows];
  var skin=dog.GetComponentInChildren<SkinnedMeshRenderer>();var baked=new Mesh();
  var sample=new List<Vector3>();
  try{
   if(skin!=null){
    skin.BakeMesh(baked);
    foreach(var v in baked.vertices)sample.Add(dog.transform.InverseTransformPoint(skin.transform.TransformPoint(v)));

   }
   for(int row=0;row<BlanketRows;row++){
    float z=Mathf.Lerp(BlanketRear,BlanketFront,row/(float)(BlanketRows-1));float top=.50f,width=.13f;
    foreach(var v in sample){if(Mathf.Abs(v.z-z)>.065f||v.y<.32f||v.y>.65f)continue;
     if(Mathf.Abs(v.x)<.09f)top=Mathf.Max(top,v.y);
     width=Mathf.Max(width,Mathf.Abs(v.x));
    }
    blanketTop[row]=top+.012f;blanketWidth[row]=Mathf.Clamp(width+.012f,.13f,.24f);
   }
  }finally{Object.Destroy(baked);}
  // Smooth section changes to keep a supple, continuous silhouette.
  for(int row=1;row<BlanketRows-1;row++){
   blanketTop[row]=(blanketTop[row-1]+2*blanketTop[row]+blanketTop[row+1])/4;
   blanketWidth[row]=(blanketWidth[row-1]+2*blanketWidth[row]+blanketWidth[row+1])/4;
  }
  var vertices=new Vector3[BlanketRows*BlanketColumns];var uv=new Vector2[vertices.Length];var triangles=new List<int>();
  for(int row=0;row<BlanketRows;row++)for(int col=0;col<BlanketColumns;col++){
   int i=row*BlanketColumns+col;float t=col/(float)(BlanketColumns-1);
   vertices[i]=BlanketPoint(row,Mathf.Lerp(-1.68f,1.68f,t));uv[i]=new Vector2(t,row/(float)(BlanketRows-1));
   if(row==BlanketRows-1||col==BlanketColumns-1)continue;
   int j=i+BlanketColumns;
   triangles.AddRange(new[]{i,j,i+1,i+1,j,j+1});
  }
  int vertexCount=vertices.Length;var backTriangles=triangles.ToArray();
  foreach(var index in backTriangles.Reverse())triangles.Add(index+vertexCount);
  vertices=vertices.Concat(vertices).ToArray();uv=uv.Concat(uv).ToArray();
  leatherMesh=new Mesh{name="BureFittedLeatherBlanket"};leatherMesh.vertices=vertices;leatherMesh.uv=uv;leatherMesh.triangles=triangles.ToArray();leatherMesh.RecalculateNormals();leatherMesh.RecalculateBounds();
  leatherTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);leatherTexture.name="BureLeatherGrain";
  var grain=new System.Random(731);
  for(int y=0;y<64;y++)for(int x=0;x<64;x++){
   float noise=(float)grain.NextDouble()*.065f;float crease=(x%19==0||y%23==0)?.025f:0;
   leatherTexture.SetPixel(x,y,new Color(.27f+noise-crease,.135f+noise*.65f-crease,.065f+noise*.3f,1));
  }
  leatherTexture.Apply();
  var shader=Shader.Find("Standard")??Shader.Find("Diffuse")??Shader.Find("Sprites/Default");
  leatherMaterial=new Material(shader){name="BureWornLeather",mainTexture=leatherTexture,color=Color.white};
  if(leatherMaterial.HasProperty("_Glossiness"))leatherMaterial.SetFloat("_Glossiness",.08f);
  leatherBlanket=new GameObject("BureLeatherBlanket");leatherBlanket.transform.SetParent(dog.transform,false);
  leatherBlanket.AddComponent<MeshFilter>().sharedMesh=leatherMesh;leatherBlanket.AddComponent<MeshRenderer>().sharedMaterial=leatherMaterial;
  // Keep the blanket in the same stable body frame used to fit the mesh.
  // Individual skeleton bones can rotate with the head and must not own tack.
  leatherBlanket.transform.localPosition=Vector3.zero;
  leatherBlanket.transform.localRotation=Quaternion.identity;
  leatherBlanket.transform.localScale=Vector3.one;
  CreateTowCrossbar();
  rightTrace=NewTowLine("BureRightTowRope",.014f,17);bellyBand=NewTowLine("BureBellyBand",.028f,17);
  leftEdge=NewTowLine("BureLeftLeatherSeam",.006f,BlanketRows);rightEdge=NewTowLine("BureRightLeatherSeam",.006f,BlanketRows);
  leftRing=NewTowLine("BureLeftTowRing",.009f,17);rightRing=NewTowLine("BureRightTowRing",.009f,17);
  foreach(var line in new[]{trace,strap,rightTrace,bellyBand,leftEdge,rightEdge,leftRing,rightRing,towBridle}){
   line.numCornerVertices=4;line.numCapVertices=4;line.generateLightingData=true;
  }
  UpdateLeatherHarness();
 }
 void UpdateLeatherHarness(){
  if(leatherBlanket==null)return;
  float sag=MotionRules.RopeSag(visualSpeed);
  var pull=Vector3.ProjectOnPlane(dog.transform.position-towAnchor.position,Vector3.up).normalized;
  if(pull.sqrMagnitude<.1f)pull=towHeading;
  towCrossbar.transform.position=towAnchor.position+pull*.35f+Vector3.up*.06f;
  towCrossbar.transform.rotation=Quaternion.LookRotation(pull);
  var barLeft=towCrossbar.transform.TransformPoint(new Vector3(-.36f,0,0));
  var barRight=towCrossbar.transform.TransformPoint(new Vector3(.36f,0,0));
  // Both ends of the wooden equalizer connect to the existing central ring.
  towBridle.SetPosition(0,barLeft);towBridle.SetPosition(1,towAnchor.position);towBridle.SetPosition(2,barRight);
  // Rope pulls from reinforced side rings, clear of the hind legs and tail.
  for(int side=0;side<2;side++){
   float sign=side==0?-1:1;var line=side==0?trace:rightTrace;
   var end=TackPoint(BlanketPoint(0,sign*1.15f,.017f));
   var start=side==0?barLeft:barRight;
   for(int i=0;i<17;i++){float t=i/16f;line.SetPosition(i,Vector3.Lerp(start,end,t)-Vector3.up*(Mathf.Sin(Mathf.PI*t)*sag));}
   var ring=side==0?leftRing:rightRing;
   for(int i=0;i<17;i++){
    float a=i/16f*Mathf.PI*2;ring.SetPosition(i,TackPoint(BlanketPoint(0,sign*1.15f,.018f)+new Vector3(0,Mathf.Cos(a)*.027f,Mathf.Sin(a)*.027f)));
   }
  }
  for(int row=0;row<BlanketRows;row++){
   leftEdge.SetPosition(row,TackPoint(BlanketPoint(row,-1.65f,.005f)));rightEdge.SetPosition(row,TackPoint(BlanketPoint(row,1.65f,.005f)));
  }
  // Closed bands around the rib cage: broad attachment at the shoulders,
  // a second girth behind the front legs, leaving neck and joints free.
  for(int i=0;i<17;i++){
   float a=i/16f*Mathf.PI*2;
   strap.SetPosition(i,TackPoint(BlanketPoint(BlanketRows-1,a,.018f)));
   bellyBand.SetPosition(i,TackPoint(BlanketPoint(2,a,.018f)));
  }
 }
 void ClearLeatherHarness(){
  if(towCrossbar!=null)Object.Destroy(towCrossbar);towCrossbar=null;
  if(crossbarMesh!=null)Object.Destroy(crossbarMesh);crossbarMesh=null;
  if(crossbarMaterial!=null)Object.Destroy(crossbarMaterial);crossbarMaterial=null;
  towBridle=null;
  if(leatherBlanket!=null)Object.Destroy(leatherBlanket);leatherBlanket=null;
  if(leatherMesh!=null)Object.Destroy(leatherMesh);leatherMesh=null;
  if(leatherMaterial!=null)Object.Destroy(leatherMaterial);leatherMaterial=null;
  if(leatherTexture!=null)Object.Destroy(leatherTexture);leatherTexture=null;
  rightTrace=null;bellyBand=null;leftEdge=null;rightEdge=null;leftRing=null;rightRing=null;
 }
 void CreateTowCrossbar(){
  // A real round wooden bar, not a camera-facing ribbon. No gameplay collider.
  const int sides=12;var vertices=new List<Vector3>();var triangles=new List<int>();
  for(int end=0;end<2;end++)for(int i=0;i<sides;i++){
   float a=i/(float)sides*Mathf.PI*2;
   vertices.Add(new Vector3(end==0?-.36f:.36f,Mathf.Cos(a)*.025f,Mathf.Sin(a)*.025f));
  }
  for(int i=0;i<sides;i++){
   int next=(i+1)%sides;
   triangles.AddRange(new[]{i,next,i+sides,next,next+sides,i+sides});
  }
  vertices.Add(new Vector3(-.36f,0,0));vertices.Add(new Vector3(.36f,0,0));
  for(int i=0;i<sides;i++){int next=(i+1)%sides;triangles.AddRange(new[]{24,next,i,25,i+sides,next+sides});}
  crossbarMesh=new Mesh{name="BureWoodenTowEqualizer"};crossbarMesh.vertices=vertices.ToArray();crossbarMesh.triangles=triangles.ToArray();crossbarMesh.RecalculateNormals();crossbarMesh.RecalculateBounds();
  crossbarMaterial=new Material(leatherMaterial.shader){name="BureTowBarWood",color=new Color(.30f,.20f,.10f,1)};
  if(crossbarMaterial.HasProperty("_Glossiness"))crossbarMaterial.SetFloat("_Glossiness",.05f);
  towCrossbar=new GameObject("BureWoodenTowCrossbar");towCrossbar.transform.SetParent(towRig.transform,false);
  towCrossbar.AddComponent<MeshFilter>().sharedMesh=crossbarMesh;towCrossbar.AddComponent<MeshRenderer>().sharedMaterial=crossbarMaterial;
  towBridle=NewTowLine("BureCrossbarToSledRing",.018f,3);
 }


}
