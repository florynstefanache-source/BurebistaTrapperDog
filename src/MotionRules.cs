namespace BurebistaTrapperDog;
public static class MotionRules {
 public const float MinimumGroundNormalY=.65f,MaximumGroundStep=.32f;
 public static bool ShouldMove(float distance,float stop,bool active)=>distance>stop+(active?.03f:.25f);
 public static float BrakingSpeed(float distance,float stop,float maximum)=>MathF.Min(maximum,MathF.Sqrt(MathF.Max(0,2*3.5f*(distance-stop))));
 public static float Approach(float speed,float target,float dt){
  float step=(target>speed?2.8f:5f)*Math.Clamp(dt,0,.1f);
  return target>speed?MathF.Min(target,speed+step):MathF.Max(target,speed-step);
 }
 public static bool RunGait(float speed,bool running)=>speed>(running?2.15f:2.65f);
 public static float RopeSag(float speed)=>.025f+.18f*(1-Math.Clamp(speed/1.4f,0,1));
 public static bool IsBureSled(string name)=>(name??"").Replace("(Clone)","").Trim()=="INTERACTIVE_BurebistaTrapperSled";
}
