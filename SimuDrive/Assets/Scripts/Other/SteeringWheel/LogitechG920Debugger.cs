using UnityEngine;
using UnityEngine.InputSystem;

public class LogitechG920Debugger : MonoBehaviour
{
    private void Update()
    {
        if (Gamepad.current != null)
        {
            Debug.Log("Steering Wheel X Axis: " + Gamepad.current.leftStick.x.ReadValue());
            Debug.Log("Throttle (Right Trigger): " + Gamepad.current.rightTrigger.ReadValue());
            Debug.Log("Brake (Left Trigger): " + Gamepad.current.leftTrigger.ReadValue());

            for (int i = 0; i < Gamepad.current.allControls.Count; i++)
            {
                if (Gamepad.current.allControls[i].IsPressed())
                {
                    Debug.Log("Button Pressed: " + Gamepad.current.allControls[i].name);
                }
            }
        }
    }
}
