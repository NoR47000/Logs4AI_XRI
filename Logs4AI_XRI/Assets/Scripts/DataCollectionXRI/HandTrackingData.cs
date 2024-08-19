using System.Collections;
using System.Collections.Generic;
#if USE_XR_TOOLKIT
using UnityEngine;
using UnityEngine.XR.Hands;
#endif
#if USE_OVR
using static OVRPlugin;
#endif

public class HandTrackingData : MonoBehaviour
{

    public Pose _leftHandPose = Pose.identity;
    public Pose _rightHandPose = Pose.identity;

    public List<Pose> _leftHandJointPoses = new List<Pose>();
    public List<Pose> _rightHandJointPoses = new List<Pose>();

    public bool _leftIsTracked = false; // Left hand is Tracked
    public bool _rightIsTracked = false; // Right hand is Tracked

    #region XR Toolkit Hand Tracking Data

#if USE_XR_TOOLKIT

    #region Data Collection
    public XRHand _leftHand;
    public XRHand _rightHand;

    private void GetHandPose(XRHandSubsystem subsystem)
    {
        _leftHand = subsystem.leftHand;
        _rightHand = subsystem.rightHand;

        _leftIsTracked = _leftHand.isTracked;
        _rightIsTracked = _rightHand.isTracked;

        if (_leftIsTracked)
        {
            _leftHandPose = _leftHand.rootPose;

            _leftHandJointPoses.Clear();

            for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
            {
                var trackingData = _leftHand.GetJoint(XRHandJointIDUtility.FromIndex(i));

                if (trackingData.TryGetPose(out Pose pose))
                {
                    _leftHandJointPoses.Add(pose);
                }
            }
        }

        if (_rightIsTracked)
        {
            _rightHandPose = _rightHand.rootPose;

            _rightHandJointPoses.Clear();

            for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
            {
                var trackingData = _rightHand.GetJoint(XRHandJointIDUtility.FromIndex(i));

                if (trackingData.TryGetPose(out Pose pose))
                {
                    _rightHandJointPoses.Add(pose);
                }
            }
        }
    }

    #endregion

    #region subsystem

    void OnDisable()
    {
        if (m_Subsystem != null)
        {
            m_Subsystem = null;
        }
    }

    private void setSubsystem()
    {
        if (m_Subsystem != null)
            return;

        SubsystemManager.GetSubsystems(s_SubsystemsReuse);
        if (s_SubsystemsReuse.Count == 0)
            return;

        m_Subsystem = s_SubsystemsReuse[0];
    }

    XRHandSubsystem m_Subsystem;
    static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();
    #endregion

#endif
    #endregion

    #region OVR Oculus Integration Hand Tracking Data

#if USE_OVR
    public OVRHand _leftHand;
    public OVRHand _rightHand;

    private void GetOVRHandPose(OVRHand hand, string handName)
    {
        Vector3 handPosition = Vector3.zero;
        Quaternion handRotation = Quaternion.identity;

        List<Pose> handJointPoses = new List<Pose>();

        if (hand != null && hand.IsTracked)
        {
            handPosition = hand.transform.position;
            handRotation = hand.transform.rotation;

            Debug.Log($"{handName} Hand Position: {handPosition}, {handName} Hand Rotation: {handRotation}");

            // Get all joint data
            OVRSkeleton ovrSkeleton = hand.GetComponent<OVRSkeleton>();
            if (ovrSkeleton != null)
            {
                foreach (var bone in ovrSkeleton.Bones)
                {
                    Transform boneTransform = bone.Transform;
                    Vector3 jointPosition = boneTransform.position;
                    Quaternion jointRotation = boneTransform.rotation;

                    handJointPoses.Add(new Pose(jointPosition, jointRotation));

                    Debug.Log($"{handName} Hand Joint {bone.Id}: Position: {jointPosition}, Rotation: {jointRotation}");
                }
            }
        }
        else
        {
            Debug.Log($"{handName} Hand not tracked.");
        }

        if (handName == "Left")
        {
            _leftIsTracked = hand.IsTracked;
            _leftHandPose = new Pose(handPosition, handRotation);
            _leftHandJointPoses.Clear();
            _leftHandJointPoses = handJointPoses;
        }
        else
        {   
            _rightIsTracked = hand.IsTracked
            _rightHandPose = new Pose(handPosition, handRotation);
            _rightHandJointPoses.Clear();
            _rightHandJointPoses = handJointPoses;
        }
    }


    private void GetOVRHands()
    {
        List<OVRHand> ovrHands = new List<OVRHand>(FindObjectsOfType<OVRHand>());

        foreach (OVRHand hand in ovrHands)
        {
            OVRSkeleton ovrSkeleton = hand.gameObject.GetComponent<OVRSkeleton>();
            if (ovrSkeleton.GetSkeletonType().AsHandType() == OVRHand.Hand.HandLeft)
            {
                _leftHand = hand;
            }
            else if (ovrSkeleton.GetSkeletonType().AsHandType() == OVRHand.Hand.HandRight)
            {
                _rightHand = hand;
            }
            else
            {
                Debug.LogError(" Hand is not a hand ");
            }
        }
    }
#endif

    #endregion

    void Update()
    {
#if USE_XR_TOOLKIT
        setSubsystem();
        GetHandPose(m_Subsystem);
#elif USE_OVR
        GetOVRHandPose(_leftHand, "Left");
        GetOVRHandPose(_rightHand, "Right");
#endif
    }

    private void Start()
    {
#if USE_OVR
        GetOVRHands()
#endif
    }
}
