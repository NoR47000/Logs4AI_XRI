using System.Collections;
using System.Collections.Generic;
#if USE_XR_TOOLKIT
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#endif
#if USE_OVR
using static OVRPlugin;
#endif

namespace XRIDataCollection
{
    public class HMDControllersInputData : MonoBehaviour
    {
        private void Update()
        {
#if USE_XR_TOOLKIT
            if (!_HMD.isValid || !_rightController.isValid || !_leftController.isValid)
                InitializeInputDevice();
#elif USE_OVR

            // Headset data
            GetHMDData();

            // Controller data
            GetControllerData(OVRInput.Controller.LTouch, "Left", _leftController);
            GetControllerData(OVRInput.Controller.RTouch, "Right", _rightController);
#endif
        }

        #region XR Toolkit Hand Tracking Data

#if USE_XR_TOOLKIT
        public InputDevice _HMD;
        public InputDevice _rightController;
        public InputDevice _leftController;

        // Gets HMD and Controllers (hardware) 
        private void InitializeInputDevice()
        {
            if (!_HMD.isValid)
                InitializeInputDevice(InputDeviceCharacteristics.HeadMounted, ref _HMD);
            if(!_rightController.isValid)
                InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref _rightController);
            if(!_leftController.isValid)
                InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, ref _leftController);
        }

        private void InitializeInputDevice(InputDeviceCharacteristics inputDeviceCharacteristics, ref InputDevice inputDevice)
        {
            List<InputDevice> devices = new List<InputDevice>();
            //Call InputDevices to see if it can find any device with the characteristics we want
            InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);

            //Hands might not be active so check if devices are found
            if(devices.Count > 0)
            {
                inputDevice = devices[0];
            }
        }
    }
#endif

    #endregion

    #region OVR Oculus Integration Hand Tracking Data

#if USE_OVR
        public Pose _HMD;
        public Pose _rightController;
        public Pose _leftController;

        private void GetHMDData()
        {
            _HMD = new Pose(OVRManager.instance.transform.position,OVRManager.instance.transform.rotation);
        }

        private void GetControllerData(OVRInput.Controller controller, string hand, Pose pose)
        {
            if (OVRInput.IsControllerConnected(controller))
            {
                pose = new Pose(OVRInput.GetLocalControllerPosition(controller),OVRInput.GetLocalControllerRotation(controller));
            }
            else
            {
                Debug.Log($"{hand} Controller not connected.");
            }
        }
#endif

    #endregion

}

