System.Collections.IEnumerator Check(){
var mover=UnityEngine.Object.FindFirstObjectByType<HundredHour.JoystickDemo.JoystickCubeMover>();var body=mover.GetComponent<UnityEngine.Rigidbody>();var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
try{
yield return new UnityEngine.WaitForSeconds(1);float start=body.position.y;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));yield return new UnityEngine.WaitForSeconds(.15f);float rise=body.position.y-start;float firstVelocity=body.linearVelocity.y;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return new UnityEngine.WaitForSeconds(.05f);float before=body.linearVelocity.y;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));yield return new UnityEngine.WaitForSeconds(.05f);float after=body.linearVelocity.y;UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
yield return new UnityEngine.WaitForSeconds(1.4f);float land=body.position.y;
System.IO.File.WriteAllText("Temp/JumpCheck.txt","rise="+rise+"\nupVelocity="+firstVelocity+"\nairRepeatBlocked="+(after<before)+"\nlandingError="+UnityEngine.Mathf.Abs(land-start)+"\npass="+(rise>.2f && firstVelocity>0 && after<before && UnityEngine.Mathf.Abs(land-start)<.15f));
}finally{UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);}}
UnityEngine.Object.FindFirstObjectByType<HundredHour.JoystickDemo.JoystickCubeMover>().StartCoroutine(Check());return "Testing Space key events";
