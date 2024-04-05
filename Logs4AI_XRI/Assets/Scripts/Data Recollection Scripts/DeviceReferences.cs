using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.XR;


namespace DataCollection3D
{
    public static class DeviceReferences
    {
        public static bool handTrackingEnabled;

        /// <summary>
        /// Represents participant is using hands, controller, or neither
        /// </summary>
        public enum TrackingType
        {
            None = 0,
            Controller = 1,
            Hand = 2
        }

        /// <summary>
        /// Oculus SeGets the current tracked device i.e. hand or controller
        /// </summary>
        /// <returns> Enum representing whether user is using hand or controller or neither </returns>
        public static TrackingType GetCurrentTrackedDevice()
        {
            return TrackingType.Controller;
        }

        internal static void Initialize()
        {
            DataRecollectionXRI.OnUpdate += Cognitive3D_Manager_OnUpdate;
        }

        //updates controller and hmd inputdevices to call events when states change
        private static void Cognitive3D_Manager_OnUpdate(float deltaTime)
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.isValid != HMDDevice.isValid)
            {
                InvokeHMDValidityChangeEvent(head.isValid);
                HMDDevice = head;
            }
        }
        public static bool SDKSupportsEyeTracking
        {
            get
            {
                var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                Eyes eyedata;

                if(head.TryGetFeatureValue(CommonUsages.eyesData, out eyedata))
                {
                    Debug.Log("head got feature {0}");
                    return head.TryGetFeatureValue(CommonUsages.eyesData, out eyedata);
                }
                else
                return false;
            }
        }
        public static bool SDKSupportsRoomSize
        {
            //should be everything except AR SDKS
            get
            {
                return true;
            }
        }

        #region HMD

        static InputDevice HMDDevice;

        public delegate void onDeviceValidityChange(InputDevice device, XRNode node, bool isValid);
        public static event onDeviceValidityChange OnHMDValidityChange;
        public static void InvokeHMDValidityChangeEvent(bool isValid) { if (OnHMDValidityChange != null) { OnHMDValidityChange(HMDDevice, XRNode.Head, isValid); } }

        private static Camera cam;
        public static Camera HMDCameraComponent
        {
            get
            {
                if (cam == null)
                {
                    if (HMD != null)
                    {
                        cam = HMD.GetComponent<Camera>();
                    }
                }
                return cam;
            }
        }



        private static Transform _hmd;
        /// <summary>Returns HMD based on included SDK, or Camera.Main if no SDK is used. MAY RETURN NULL!</summary>
        public static Transform HMD
        {
            get
            {
                if (_hmd == null)
                {
                    if (_hmd == null)
                    {
                        if (Camera.main != null)
                        {
                            _hmd = Camera.main.transform;
                        }
                    }
                }
                return _hmd;
            }
        }

        #endregion

    }
}
