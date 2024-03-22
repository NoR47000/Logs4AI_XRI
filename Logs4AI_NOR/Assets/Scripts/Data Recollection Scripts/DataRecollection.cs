using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;  
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using TMPro;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using System.Linq;

//!\\ "using UnityEngine.InputSystem" is not usable as it brings ambiguous Input classes with "using UnityEngine.XR"

/// <summary>
/// Get the Data from the movement data from the headset and the remotes to extract them into a csv file with a 
/// </summary>
public class DataRecollection : MonoBehaviour
{
    // Based on https://forum.unity.com/threads/how-to-access-sensor-values-and-display-them.1026583/ from user LeninRaul modified by NOR
    [SerializeField]

    public TextMeshProUGUI valores;

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

    #endregion


    private bool devicePositionChosen;
    private Vector3 devicePositionValue = Vector3.zero;
    private Vector3 prevdevicePositionValue;

    private bool deviceAngularAccelerationChosen; // Added by NOR
    private Vector3 deviceAngularAccelerationValue = Vector3.zero; // Added by NOR
    private Vector3 prevdeviceAngularAccelerationValue; // Added by NOR

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

    void Update()
    {
        // Get devices that are available for data collection
        CollectionDeviceManager();

        // Debug data on console
        DisplayDataOnConsole();

        /*/// <summary>
        /// Angular Acc, Added by NOR
        /// </summary>

        InputFeatureUsage<Vector3> deviceAngularAccelerationUsage = CommonUsages.deviceAngularAcceleration; // Added by NOR

        // make sure the value is not zero and that it has changed
        if (deviceAngularAccelerationValue != prevdeviceAngularAccelerationValue)
        {
            deviceAngularAccelerationChosen = false;
        }
        if (device.TryGetFeatureValue(deviceAngularAccelerationUsage, out deviceAngularAccelerationValue) && deviceAngularAccelerationValue != Vector3.zero && !deviceAngularAccelerationChosen)
        {
            Debug.Log("in");
            valores.text += " angularACC : " + deviceAngularAccelerationValue.ToString("F3");
            prevdeviceAngularAccelerationValue = deviceAngularAccelerationValue;
            deviceAngularAccelerationChosen = true;
        }
        else if (deviceAngularAccelerationValue == Vector3.zero && deviceAngularAccelerationChosen)
        {
            valores.text += deviceAngularAccelerationValue.ToString("F3");
            prevdeviceAngularAccelerationValue = deviceAngularAccelerationValue;
            deviceAngularAccelerationChosen = false;
        }*/
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

    // Gets the pose Data from the XR Devices
    private Vector3 getXRDeviceData(InputDevice device)
    {
        // capturing position changes
        InputFeatureUsage<Vector3> devicePositionsUsage = CommonUsages.devicePosition;

        /// Position
        // make sure the value is not zero and that it has changed
        if (devicePositionValue != prevdevicePositionValue)
        {
            devicePositionChosen = false;
        }
        if (device.TryGetFeatureValue(devicePositionsUsage, out devicePositionValue) && devicePositionValue != Vector3.zero && !devicePositionChosen)
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

    // get hand for HandTracking data
    public void getHands()
    {
        if (rHand == null)
        {
            if(GameObject.Find("Right Hand Interaction Visual") != null)
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
        // Head HMD
        Debug.Log("HeadPos : " + getXRDeviceData(headDevice));

        //// Hands and Controllers
        //if (inputMode == XRInputModalityManager.InputMode.MotionController && rControllerDevice.isValid && lControllerDevice.isValid)
        //{
        //    Debug.Log("r controller device : " + getXRDeviceData(rControllerDevice));
        //    Debug.Log("l controller device : " + getXRDeviceData(lControllerDevice));
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

        // Eyes
        if (rEyeDevice.isValid && lEyeDevice.isValid)
        {
            Debug.Log("r eye device : " + getXRDeviceData(rEyeDevice));
            Debug.Log("l eye device : " + getXRDeviceData(lEyeDevice));
        }

    }



    private void CanvasPrint()
    {
        valores.text = " Pose : " + devicePositionValue.ToString("F3");
        valores.text = devicePositionValue.ToString("F3");
    }
}