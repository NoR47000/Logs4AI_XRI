using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

//!\\ "using UnityEngine.InputSystem" is not usable as it brings ambiguous Input classes with "using UnityEngine.XR"

/// <summary>
/// Get the Data from the movement data from the headset and the remotes to extract them into a csv file with a 
/// </summary>

namespace DataCollection3D
{
    public class DataRecollectionXRI : MonoBehaviour
    {
        // Based on https://forum.unity.com/threads/how-to-access-sensor-values-and-display-them.1026583/ from user LeninRaul modified by NOR
        [SerializeField]

        #region Input definitions

        private XRInputModalityManager.InputMode inputMode;

        private List<InputDevice> devices = new List<InputDevice>();

        // Head (HMD)
        private XRNode headNode = XRNode.Head; // Head Node

        private InputDevice headDevice;

        // Hand Input System
        private XRNode rControllerNode = XRNode.RightHand; // Right Controller Node
        private XRNode lControllerNode = XRNode.LeftHand; // Left Controller Node

        private InputDevice rControllerDevice;
        private InputDevice lControllerDevice;

        private XRHandTrackingEvents rHand; // Cannot be converted into InputDevice
        private XRHandTrackingEvents lHand; // Cannot be converted into InputDevice

        //Eyes
        private XRNode rEye = XRNode.RightEye;
        private XRNode lEye = XRNode.LeftEye;

        private InputDevice rEyeDevice;
        private InputDevice lEyeDevice;

        private Eyes eyes;

        [Header("Visualization Transforms")]
        public Transform fixationPointTransform;
        public Transform leftEyeTransform;
        public Transform rightEyeTransform;

        private Vector3 leftEyePosition;
        private Vector3 rightEyePosition;
        private Quaternion leftEyeRotation;
        private Quaternion rightEyeRotation;
        private Vector3 fixationPoint;

        [SerializeField]
        public Quaternion rotation = Quaternion.identity;
        public Vector3 velocity = Vector3.zero;

        private bool devicePositionChosen;
        private Vector3 devicePositionValue = Vector3.zero;
        private Vector3 prevdevicePositionValue;
        #endregion

        //Access to the hardware device and gets its information saving it in the variable device
        InputDevice GetDevice(XRNode deviceNode)
        {
            InputDevices.GetDevicesAtXRNode(deviceNode, devices); // Modified by NOR
            return devices[0];
        }



        // Checks if the device is enable, if not it takes action and calls the GetDevice function
        void OnEnable()
        {
            if (!headDevice.isValid)
            {
                headDevice = GetDevice(headNode);
            }

        }


        // Device Manager for the Data collection 
        public void CollectionDeviceManager()
        {
            // Head Mounted Display
            if (!headDevice.isValid)
            {
                headDevice = GetDevice(headNode);
            }

            inputMode = XRInputModalityManager.currentInputMode.Value;

            // Hand and Controllers
            if (inputMode == XRInputModalityManager.InputMode.MotionController && !rControllerDevice.isValid && !lControllerDevice.isValid)
            {
                rControllerDevice = GetDevice(rControllerNode);
                lControllerDevice = GetDevice(lControllerNode);
            }
            if (inputMode == XRInputModalityManager.InputMode.TrackedHand && (rHand == null || lHand == null))
            {
                getHands();
            }

            // Eyes  : For devices without eye tracking, it simulates a center eye based on the position of the Headset (tested with Quest 2)
            if (!rEyeDevice.isValid && !lEyeDevice.isValid)
            {
                rEyeDevice = GetDevice(rEye);
                lEyeDevice = GetDevice(lEye);
            }
        }


        /// <summary>
        /// Use :
        /// CommonUsages.devicePosition for position
        /// CommonUsages.deviceVelocity for velocity
        /// 
        /// </summary>
        // Gets the position Data from the XR Devices
        private Vector3 getXRDevicePositionData(InputDevice device, InputFeatureUsage<Vector3> usage)
        {
            // capturing position changes
            InputFeatureUsage<Vector3> Usage = usage;

            /// Position
            // make sure the value is not zero and that it has changed
            if (devicePositionValue != prevdevicePositionValue)
            {
                devicePositionChosen = false;
            }
            if (device.TryGetFeatureValue(Usage, out devicePositionValue) && devicePositionValue != Vector3.zero && !devicePositionChosen)
            {
                prevdevicePositionValue = devicePositionValue;
                devicePositionChosen = true;
            }
            // User is not moving but has a Position or Angular Acceleration 
            else if (devicePositionValue == Vector3.zero && devicePositionChosen)
            {
                prevdevicePositionValue = devicePositionValue;
                devicePositionChosen = false;
            }
            return devicePositionValue;
        }

        // Gets all the information from Eye Tracking
        private void getETData(InputDevice device)
        {
            var EyeControllers = new List<UnityEngine.XR.InputDevice>();
            var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.EyeTracking;
            UnityEngine.XR.InputDevices.GetDevices(EyeControllers);
            //Debug.Log("Eyecontroller size : " + EyeControllers.Count);
            foreach (var devices in EyeControllers)
            {
                //Debug.Log(string.Format("Device name '{0}' has characteristics '{1}' with Subsystem '{2}'", devices.name, devices.characteristics.ToString(), devices.subsystem));
                if (devices.TryGetFeatureValue(CommonUsages.eyesData, out eyes))
                {
                    Debug.Log("in 2");


                    if (eyes.TryGetLeftEyePosition(out leftEyePosition))
                    {
                        leftEyeTransform.localPosition = leftEyePosition;
                    }

                    if (eyes.TryGetLeftEyeRotation(out leftEyeRotation))
                    {
                        leftEyeTransform.localRotation = leftEyeRotation;
                    }

                    if (eyes.TryGetRightEyePosition(out rightEyePosition))
                    {
                        Debug.Log(rightEyePosition + "transform");

                        rightEyeTransform.localPosition = rightEyePosition;
                    }

                    if (eyes.TryGetRightEyeRotation(out rightEyeRotation))
                    {
                        rightEyeTransform.localRotation = rightEyeRotation;
                    }

                    if (eyes.TryGetFixationPoint(out fixationPoint))
                    {
                        fixationPointTransform.localPosition = fixationPoint;
                    }
                }
            }

            Debug.Log("in 1");

        }

        // get hand for HandTracking data
        public void getHands()
        {
            if (rHand == null)
            {
                if (GameObject.Find("Right Hand Interaction Visual") != null)
                {
                    rHand = GameObject.Find("Right Hand Interaction Visual").GetComponent<XRHandTrackingEvents>();
                }
            }
            if (lHand == null)
            {
                if (GameObject.Find("Left Hand Interaction Visual") != null)
                {
                    lHand = GameObject.Find("Left Hand Interaction Visual").GetComponent<XRHandTrackingEvents>();
                }
            }
        }


        // Display Data On Console for Debug
        private void DisplayDataOnConsole()
        {
            //// Head HMD
            //Debug.Log("HeadPos : " + getXRDevicePositionData(headDevice, CommonUsages.devicePosition));

            //// Hands and Controllers
            //if (inputMode == XRInputModalityManager.InputMode.MotionController && rControllerDevice.isValid && lControllerDevice.isValid)
            //{
            //    Debug.Log("r controller device : " + getXRDevicePositionData(rControllerDevice, CommonUsages.devicePosition));
            //    Debug.Log("l controller device : " + getXRDevicePositionData(lControllerDevice, CommonUsages.devicePosition));
            //}
            //if (inputMode == XRInputModalityManager.InputMode.TrackedHand)
            //{
            //    if (rHand != null && rHand.handIsTracked)
            //    {
            //        Debug.Log("r hand T Position : " + rHand.rootPose.position);
            //    }
            //    if (lHand != null && lHand.handIsTracked)
            //    {
            //        Debug.Log("l hand T Position : " + lHand.rootPose.position);
            //    }
            //}

            //// Eyes // Fraud -> is only the HMD position (can get eye position with CommonUsages.leftEyePosition but is only compared to center of headset)
            //if (rEyeDevice.isValid && lEyeDevice.isValid)
            //{

            //    //Eye position
            //    Debug.Log("r eye device pos : " + getXRDevicePositionData(rEyeDevice,CommonUsages.centerEyeAcceleration));
            //    Debug.Log("l eye device pos : " + getXRDevicePositionData(lEyeDevice, CommonUsages.devicePosition));
            //}

            getETData(headDevice);
            getETData(rEyeDevice);
        }


        #region Updates

        public delegate void onUpdate(float deltaTime);

        public static event onUpdate OnUpdate;
        private static void InvokeUpdateEvent(float deltaTime) { if (OnUpdate != null) { OnUpdate(deltaTime); } }

        void Update()
        {
            // Get devices that are available for data collection
            CollectionDeviceManager();

            // Debug data on console
            DisplayDataOnConsole();

            InvokeUpdateEvent(CurrentTime());
            Eyes eyes = new Eyes();
            int eyeB= eyes.LeftEyeOpen();
            Debug.Log("eye tracker" + eyeB);
        }

        bool LeftEyeOpen()
        {
            UnityEngine.XR.Eyes eyes;
            Debug.Log(UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftEye).TryGetFeatureValue(UnityEngine.XR.CommonUsages.eyesData, out eyes));
            if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftEye).TryGetFeatureValue(UnityEngine.XR.CommonUsages.eyesData, out eyes))
            {
                float open;
                if (eyes.TryGetLeftEyeOpenAmount(out open))
                {
                    return open > 0.6f;
                }
            }
            return false;
        }
        #endregion

        #region Current time since epoch ( 1970-1-1 midnight )
        public static long CurrentTime()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        #endregion
    }

}