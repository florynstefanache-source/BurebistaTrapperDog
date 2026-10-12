using UnityEngine;
namespace BurebistaTrapperDog;
public partial class Main {
 const float HeadGestureInterval=10f;
 float nextHeadMovement,idleHeadEnd;string idleGesture;int idleGestureChoice;
 string IdleAnimation(){
  if(animation==null||animation["Idle"]==null)return "Idle";
  if(idleGesture!=null){
   if(Time.time<idleHeadEnd)return idleGesture;
   var finished=animation[idleGesture];if(finished!=null){finished.normalizedTime=0;finished.speed=0;}
   idleGesture=null;idleHeadEnd=0;
  }
  var neutral=animation["Idle"];neutral.normalizedTime=0;neutral.speed=0;
  if(nextHeadMovement<=0)nextHeadMovement=Time.time+HeadGestureInterval;
  if(Time.time<nextHeadMovement)return "Idle";
  string choice=new[]{"Idle","Idle2","Idle3"}[idleGestureChoice++%3];
  if(animation[choice]==null)choice="Idle";
  var gesture=animation[choice];gesture.normalizedTime=0;gesture.speed=1;gesture.wrapMode=WrapMode.ClampForever;
  idleGesture=choice;idleHeadEnd=Time.time+Mathf.Max(.1f,gesture.length);nextHeadMovement=Time.time+HeadGestureInterval;
  return choice;
 }
 void StopIdleGesture(){
  if(idleGesture!=null&&animation!=null){var gesture=animation[idleGesture];if(gesture!=null){gesture.normalizedTime=0;gesture.speed=0;}}
  idleGesture=null;idleHeadEnd=0;nextHeadMovement=Time.time+HeadGestureInterval;
 }
}
