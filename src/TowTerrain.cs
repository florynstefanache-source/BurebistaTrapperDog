namespace BurebistaTrapperDog;
public readonly record struct TowSupport(float X,float Z,float Height);
public static class TowTerrain {
 // Fit a ground plane through the available supports. Missing runner support
 // does not invalidate a centre that the dog's own ground query can walk on.
 public static bool Fit(IReadOnlyList<TowSupport> points,out float slopeX,out float slopeZ){
  slopeX=slopeZ=0;if(points.Count<3)return false;
  double n=points.Count,sx=0,sz=0,sy=0,xx=0,zz=0,xz=0,xy=0,zy=0;
  foreach(var p in points){if(!float.IsFinite(p.X)||!float.IsFinite(p.Z)||!float.IsFinite(p.Height))return false;sx+=p.X;sz+=p.Z;sy+=p.Height;xx+=p.X*p.X;zz+=p.Z*p.Z;xz+=p.X*p.Z;xy+=p.X*p.Height;zy+=p.Z*p.Height;}
  xx-=sx*sx/n;zz-=sz*sz/n;xz-=sx*sz/n;xy-=sx*sy/n;zy-=sz*sy/n;
  double determinant=xx*zz-xz*xz;if(Math.Abs(determinant)<1e-8)return false;
  slopeX=(float)((xy*zz-zy*xz)/determinant);slopeZ=(float)((zy*xx-xy*xz)/determinant);
  return float.IsFinite(slopeX)&&float.IsFinite(slopeZ);
 }
}
