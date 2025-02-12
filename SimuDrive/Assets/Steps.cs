using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steps : MonoBehaviour
{
    [SerializeField] private GameObject step1;
    [SerializeField] private GameObject step2;
    [SerializeField] private GameObject step3;
    [SerializeField] private GameObject step4;
    [SerializeField] private GameObject stepClutchPtoblem;
    [SerializeField] private CarController carController; // Reference to the CarController

    private float clutchInput;
    private float acceleratorInput; // Remove initialization here

    void Start()
    {
        step1.SetActive(true);
        step2.SetActive(false);
        step3.SetActive(false);
        step4.SetActive(false);
        stepClutchPtoblem.SetActive(false);
    }

    void Update()
    {
        clutchInput = Input.GetAxis("Clutch"); // Read clutch input on each frame
        acceleratorInput = Input.GetAxis("Accelerator"); // Move initialization here

        if (clutchInput > 0.8f || Input.GetKeyDown(KeyCode.LeftShift))
        {
            step1.SetActive(false); // Hide the first step when clutch is pressed enough
            step2.SetActive(true);
        }

        while(true)
        {
            if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (clutchInput > 0.8f || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    step2.SetActive(false);
                    step3.SetActive(true);
                    break;
                }
                else
                {
                    stepClutchPtoblem.SetActive(true);
                }
            }
        }
        

        if (acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow))
        {
            step3.SetActive(false);
        }
    }
}
