using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRIDataCollection;
using UnityEngine.XR;
using TMPro;
using UnityEngine.UI;
#if USE_XR_TOOLKIT
using UnityEngine.XR.Hands;
#endif

[RequireComponent(typeof(HMDControllersInputData))]
[RequireComponent(typeof(HandTrackingData))]
public class DisplayInputData : MonoBehaviour
{
    public TextMeshPro display;

    public string _textInput;

    private HMDControllersInputData _inputData;
    private HandTrackingData _handTrackingData;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputData = GetComponent<HMDControllersInputData>();    
        _handTrackingData = GetComponent<HandTrackingData>();
    }

    // Update is called once per frame
    void Update()
    {
        ///CommonUsages.deviceVelocity
        ///CommonUsages.deviceRotation
        ///CommonUsages.devicePosition
        _textInput = "";

        if (_inputData._rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightControllerPos))
        {
            _textInput += "right controller : " +rightControllerPos.ToString("F2");
        }
        if(_handTrackingData._leftIsTracked )
        {
            _textInput += "left Hand position : " + _handTrackingData._leftHandPose.position;

            int nbOfJoints = _handTrackingData._leftHandJointPoses.Count;
            for(int i = 0; i< nbOfJoints; i++)
            {
                _textInput += XRHandJointIDUtility.FromIndex(i) + " " + _handTrackingData._leftHandJointPoses[i].position;
            }
        }


        display.text = _textInput;
    }
}
