using UnityEngine;
using UnityEngine.InputSystem;

public class GearShifterController : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction gear1Action;
    private InputAction gear2Action;
    private InputAction gear3Action;
    private InputAction gear4Action;
    private InputAction gear5Action;
    private InputAction gearRAction;

    private void Awake()
    {

        // Get the GearShifter action map
        var gearShifterMap = inputActions.FindActionMap("GearShifter");
        
        // Get the gear1 action
        gear1Action = gearShifterMap.FindAction("gear1");
        gear2Action = gearShifterMap.FindAction("gear2");
        gear3Action = gearShifterMap.FindAction("gear3");
        gear4Action = gearShifterMap.FindAction("gear4");
        gear5Action = gearShifterMap.FindAction("gear5");
        gearRAction = gearShifterMap.FindAction("gearR");

        // Enable the action
        gear1Action.Enable();
        gear2Action.Enable();
        gear3Action.Enable();
        gear4Action.Enable();
        gear5Action.Enable();
        gearRAction.Enable();
        Debug.Log("gear1 Action enabled successfully!");
    }

    private void Update()
    {
        if (gear1Action.triggered)
        {
            Debug.Log("Gear 1 Selected");
        }
        else if (gear2Action.triggered)
        {
            Debug.Log("Gear 2 Selected");
        }
        else if (gear3Action.triggered)
        {
            Debug.Log("Gear 3 Selected");
        }
        else if (gear4Action.triggered)
        {
            Debug.Log("Gear 4 Selected");
        }
        else if (gear5Action.triggered)
        {
            Debug.Log("Gear 5 Selected");
        }
        else if (gearRAction.triggered)
        {
            Debug.Log("Gear R Selected");
        }
    }
}