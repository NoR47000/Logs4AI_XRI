using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_OVR
using static OVRPlugin;
#endif

public class EyeTrackingData : MonoBehaviour
{
    public string _objectGazedAt = string.Empty;
    public Ray _gazeray;
#if USE_OVR
    public OVRPlugin.EyeGazesState _currentEyeGazesState;
    public Transform _leftEyeObj;
    public Transform _rightEyeObj;

    public OVRPlugin.EyeGazeState _eyeGazeL;
    public OVRPlugin.EyeGazeState _eyeGazeR;


    private void GetEyeData() 
    {
        if (OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
        {
            // Get validity of eyestate and confidence level
            _eyeGazeL = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Left];
            _eyeGazeR = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Right];

            // Create a ray for the direction the player is looking at
            Vector3 rayOrigin = _leftEyeObj.position + 0.5f * (_rightEyeObj.position - _leftEyeObj.position);
            Vector3 rayDirection = 0.5f * (_leftEyeObj.forward + _rightEyeObj.forward);

            // Create the ray
            gazeray = new Ray(rayOrigin, rayDirection);

            // Perform the raycast
            RaycastHit hit;
            if (Physics.Raycast(gazeray, out hit))
            {
                // Print the name of the first object hit by the ray
                //Debug.Log("First object hit: " + hit.collider.gameObject.name);
                _objectGazedAt = hit.collider.gameObject.name;
            }
            else
            {
                _objectGazedAt = string.empty;
                //Debug.Log("No objects hit.");
            }
        }
        else
        {
            gazeray = null;
            Debug.Log("EyeTracking is not working");
        }
    }

    private void GetOVREyes()
    {
        if (OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
        {
            List<OVREyeGaze> oVREyes = new List<OVREyeGaze>(FindObjectsOfType<OVREyeGaze>());

            foreach(OVREyeGaze ovrEye in oVREyes)
            {
                if(ovrEye.Eye == OVREyeGaze.EyeId.Left)
                {
                    _leftEyeObj = ovrEye.transform;
                    Debug.Log("Left eye exists : "+ ovrEye);
                }
                else if(ovrEye.Eye == OVREyeGaze.EyeId.Right)
                {
                    _rightEyeObj = ovrEye.transform;
                    Debug.Log("Right eye exists : " + ovrEye);
                }
            }
        }
    }
#endif
    // Start is called before the first frame update
    void Start()
    {
#if USE_OVR
        GetOVREyes();
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if USE_OVR
        GetEyeData();
#endif
    }
}
