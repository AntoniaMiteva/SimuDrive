using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class StartDrive1 : MonoBehaviour
{
    [SerializeField] private CarController carController; 
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;

    private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput;
    private float clutchReleaseThreshold = 0.5f;

    private enum StepState { Step1, Step2, Step3, Step4, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        panelProblem.SetActive(false);
        panelDone.SetActive(false);
        textMeshProUGUI.text = "Натиснете съединителя до долу.";
    }

    void Update()
    {
        clutchInput = Input.GetAxis("Clutch");
        acceleratorInput = Input.GetAxis("Accelerator");


        previousClutchInput = clutchInput; 

        if (carController.Speed <= 5f && carController.Gear == 1 && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
        {
            panelProblem.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            panelProblem.SetActive(false);
            Time.timeScale = 1f;
        }

        switch (currentStep)
        {
            case StepState.Step1:
                {
                    if (clutchInput > 0.5f || Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        textMeshProUGUI.text = "Превключете на първа скорост, като придърпате скоростния лост към тялото Ви и след това нагоре.";
                        currentStep = StepState.Step2;
                    }
                    else
                    {
                        textMeshProUGUI.text = "Натиснете съединителя до долу.";
                    }

                }
                break;


            case StepState.Step2:
                if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Подайте леко и внимателно газ.";
                    currentStep = StepState.Step3;
                }
                break;

            case StepState.Step3:
                if ((acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Когато колата потегли, внимателно и плавно вдигнете крака си от педала на съединителя.";
                    currentStep = StepState.Step4;
                }
                break;

            case StepState.Step4:
                if (carController.Speed > 2f)
                {
                    if ((carController.isSteeringWheelConnected && clutchInput < 0.2f) || Input.GetKeyUp(KeyCode.LeftShift))
                    {
                        textMeshProUGUI.text = "Съединителят е махнат плавно.";
                        currentStep = StepState.Completed;
                        carController.enabled = false;
                        Time.timeScale = 0f;
                        panelDone.SetActive(true);
                        panelProblem.SetActive(false);
                        textMeshProUGUI.text = "";
                    }
                    else
                    {
                        textMeshProUGUI.text = "Когато колата потегли, внимателно и плавно вдигнете крака си от педала на съединителя.";
                    }
                }
                break;


        }
    }

    public void BackToStart()
    {
        SceneManager.LoadScene("Drive Menu");
    }
}
