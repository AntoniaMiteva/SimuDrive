using UnityEngine;
using System.Collections;
using System.Text;

namespace Logitech
{
    public class LogitechSteeringWheel : MonoBehaviour
    {
        LogitechGSDK.LogiControllerPropertiesData properties;
        private string actualState;
        private string activeForces;
        private string propertiesEdit;
        private string buttonStatus;
        private string forcesLabel;
        string[] activeForceAndEffect;

        // New flag to control panel visibility
        private bool isPanelVisible = true;

        // Use this for initialization
        void Start()
        {
            activeForces = "";
            propertiesEdit = "";
            actualState = "";
            buttonStatus = "";
            forcesLabel = "Press the following keys to activate forces and effects on the steering wheel / gaming controller \n";
            forcesLabel += "Spring force : S\n";
            forcesLabel += "Constant force : C\n";
            forcesLabel += "Damper force : D\n";
            forcesLabel += "Side collision : Left or Right Arrow\n";
            forcesLabel += "Front collision : Up arrow\n";
            forcesLabel += "Dirt road effect : I\n";
            forcesLabel += "Bumpy road effect : B\n";
            forcesLabel += "Slippery road effect : L\n";
            forcesLabel += "Surface effect : U\n";
            forcesLabel += "Car Airborne effect : A\n";
            forcesLabel += "Soft Stop Force : O\n";
            forcesLabel += "Set example controller properties : PageUp\n";
            forcesLabel += "Play Leds : P\n";
            activeForceAndEffect = new string[9];
            Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
        }

        void OnApplicationQuit()
        {
            Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
        }

        void OnGUI()
        {
            if (isPanelVisible)
            {
                activeForces = GUI.TextArea(new Rect(10, 10, 180, 200), activeForces, 400);
                propertiesEdit = GUI.TextArea(new Rect(200, 10, 200, 200), propertiesEdit, 400);
                actualState = GUI.TextArea(new Rect(410, 10, 300, 200), actualState, 1000);
                buttonStatus = GUI.TextArea(new Rect(720, 10, 300, 200), buttonStatus, 1000);
                GUI.Label(new Rect(10, 400, 800, 400), forcesLabel);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //All the test functions are called on the first device plugged in(index = 0)
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                //CONTROLLER PROPERTIES
                StringBuilder deviceName = new StringBuilder(256);
                LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);
                propertiesEdit = "Current Controller : " + deviceName + "\n";
                propertiesEdit += "Current controller properties : \n\n";
                LogitechGSDK.LogiControllerPropertiesData actualProperties = new LogitechGSDK.LogiControllerPropertiesData();
                LogitechGSDK.LogiGetCurrentControllerProperties(0, ref actualProperties);
                propertiesEdit += "forceEnable = " + actualProperties.forceEnable + "\n";
                propertiesEdit += "overallGain = " + actualProperties.overallGain + "\n";
                propertiesEdit += "springGain = " + actualProperties.springGain + "\n";
                propertiesEdit += "damperGain = " + actualProperties.damperGain + "\n";
                propertiesEdit += "defaultSpringEnabled = " + actualProperties.defaultSpringEnabled + "\n";
                propertiesEdit += "combinePedals = " + actualProperties.combinePedals + "\n";
                propertiesEdit += "wheelRange = " + actualProperties.wheelRange + "\n";
                propertiesEdit += "gameSettingsEnabled = " + actualProperties.gameSettingsEnabled + "\n";
                propertiesEdit += "allowGameSettings = " + actualProperties.allowGameSettings + "\n";

                //CONTROLLER STATE
                actualState = "Steering wheel current state : \n\n";
                LogitechGSDK.DIJOYSTATE2ENGINES rec;
                rec = LogitechGSDK.LogiGetStateUnity(0);
                actualState += "x-axis position :" + rec.lX + "\n";
                actualState += "y-axis position :" + rec.lY + "\n";
                actualState += "z-axis position :" + rec.lZ + "\n";
                actualState += "x-axis rotation :" + rec.lRx + "\n";
                actualState += "y-axis rotation :" + rec.lRy + "\n";
                actualState += "z-axis rotation :" + rec.lRz + "\n";
                actualState += "extra axes positions 1 :" + rec.rglSlider[0] + "\n";
                actualState += "extra axes positions 2 :" + rec.rglSlider[1] + "\n";
                switch (rec.rgdwPOV[0])
                {
                    case (0): actualState += "POV : UP\n"; break;
                    case (4500): actualState += "POV : UP-RIGHT\n"; break;
                    case (9000): actualState += "POV : RIGHT\n"; break;
                    case (13500): actualState += "POV : DOWN-RIGHT\n"; break;
                    case (18000): actualState += "POV : DOWN\n"; break;
                    case (22500): actualState += "POV : DOWN-LEFT\n"; break;
                    case (27000): actualState += "POV : LEFT\n"; break;
                    case (31500): actualState += "POV : UP-LEFT\n"; break;
                    default: actualState += "POV : CENTER\n"; break;
                }

                //Button status :
                buttonStatus = "Button pressed : \n\n";
                for (int i = 0; i < 128; i++)
                {
                    if (rec.rgbButtons[i] == 128)
                    {
                        buttonStatus += "Button " + i + " pressed\n";
                    }
                }

                int shifterTipe = LogitechGSDK.LogiGetShifterMode(0);
                string shifterString = "";
                if (shifterTipe == 1) shifterString = "Gated";
                else if (shifterTipe == 0) shifterString = "Sequential";
                else shifterString = "Unknown";
                actualState += "\nSHIFTER MODE:" + shifterString;

                // FORCES AND EFFECTS 
                activeForces = "Active forces and effects :\n";

                // Handle input keys to toggle the visibility
                if (Input.GetKeyDown(KeyCode.H)) // Press H to hide or show the panel
                {
                    isPanelVisible = !isPanelVisible;
                }

                // Other force effects handled here as before...

            }
            else if (!LogitechGSDK.LogiIsConnected(0))
            {
                actualState = "PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER";
            }
            else
            {
                actualState = "THIS WINDOW NEEDS TO BE IN FOREGROUND IN ORDER FOR THE SDK TO WORK PROPERLY";
            }
        }
    }
}
