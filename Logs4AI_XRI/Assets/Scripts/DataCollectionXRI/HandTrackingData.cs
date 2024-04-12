using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Processing;

public class HandTrackingData : MonoBehaviour
{

    public XRHand _leftHand;
    public XRHand _rightHand;

    public Pose _leftHandPose = Pose.identity;
    public Pose _rightHandPose = Pose.identity;

    public List<Pose> _leftHandJointPoses = new List<Pose>();
    public List<Pose> _rightHandJointPoses = new List<Pose>();

    public bool _leftIsTracked = false; // Left hand is Tracked
    public bool _rightIsTracked = false; // Right hand is Tracked

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
            _rightHandPose= _rightHand.rootPose;

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

    #region subsystem
    void Update()
    {
        setSubsystem();
        GetHandPose(m_Subsystem);
    }

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
}
