using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace XRIDataCollection
{
    public class HMDControllersInputData : MonoBehaviour
    {
        public InputDevice _HMD;
        public InputDevice _rightController;
        public InputDevice _leftController;

        private void Update()
        {
            if (!_HMD.isValid || !_rightController.isValid || !_leftController.isValid)
                InitializeInputDevice();
        }

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
}

