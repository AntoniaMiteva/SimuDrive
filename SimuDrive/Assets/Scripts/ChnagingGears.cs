using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangingGears : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;


    private float clutchInput;
    private float acceleratorInput;

    private float previousClutchInput; 
    private float clutchReleaseThreshold = 0.5f; 

    private enum StepState { Step1, Step2, Step3, Step4, Step5, Step6, Step7, Step8, Step9, Step10, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        panelProblem.SetActive(false);
        panelDone.SetActive(false);
        textMeshProUGUI.text = "��������� ����������� �� ����.";
    }

    void Update()
    {
        clutchInput = Input.GetAxis("Clutch");
        acceleratorInput = Input.GetAxis("Accelerator");

        if (carController.Speed <= 5f && carController.Gear != 0 && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
        {
            panelProblem.SetActive(true); 
        }
        else
        {
            panelProblem.SetActive(false);
        }


        switch (currentStep)
        {
            case StepState.Step1:
                if (clutchInput > 0.8f || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    textMeshProUGUI.text = "����������� �� ����� �������.";
                    currentStep = StepState.Step2;
                }
                else
                {
                    textMeshProUGUI.text = "��������� ����������� �� ����.";
                }
                break;

            case StepState.Step2:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        textMeshProUGUI.text = "����������� �� ����� �������.";
                        currentStep = StepState.Step3;
                    }
                }
                break;

            case StepState.Step3:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 2 || Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        textMeshProUGUI.text = "����������� �� ����� �������.";
                        currentStep = StepState.Step4;
                    }
                }
                break;

            case StepState.Step4:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 3 || Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        textMeshProUGUI.text = "����������� �� �������� �������.";
                        currentStep = StepState.Step5;
                    }
                }
                break;

            case StepState.Step5:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 4 || Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        textMeshProUGUI.text = "����������� �� ���� �������.";
                        currentStep = StepState.Step6;
                    }
                }
                break;
            case StepState.Step6:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 5 || Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        textMeshProUGUI.text = "����������� �� ����� �������.";
                        currentStep = StepState.Step7;
                    }
                }
                break;
            case StepState.Step7:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == -1 || Input.GetKeyDown(KeyCode.R))
                    {
                        textMeshProUGUI.text = "���������� �� �������.";
                        currentStep = StepState.Step8;
                    }
                }
                break;
            case StepState.Step8:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 0 || Input.GetKeyDown(KeyCode.Alpha0))
                    {
                        textMeshProUGUI.text = "���������� �����������.";
                        currentStep = StepState.Step9;
                    }
                }
                break;
            case StepState.Step9:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    textMeshProUGUI.text = "���������� �����������.";
                }
                else
                {
                    textMeshProUGUI.text = "";
                    currentStep = StepState.Completed;
                    panelDone.SetActive(true);
                }

                break;
            
        }
    }


    public void BackToStart()
    {
        SceneManager.LoadScene("Drive Menu");
    }

}