using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if USE_XR_TOOLKIT
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#endif
#if USE_OVR
using static OVRPlugin;
#endif

namespace XRIDataCollection
{
    [RequireComponent(typeof(HMDControllersInputData))]
    [RequireComponent(typeof(HandTrackingData))]
    public class EventInteractionData : MonoBehaviour
    {
        #region Event Listener
        /// <summary>
        /// Get the events happening in the scene (Button pressing, UI,...)
        /// </summary>
        private EventSystem _eventSystem;
        private BaseEventData m_DummyData;

        private string _sceneName;

        private EventSystem Events()
        {
            return _eventSystem;
        }

        #endregion

        #region Interaction Listener
        /// <summary>
        /// Get the virtual object the player is touching
        /// </summary>
        /// 
        private HMDControllersInputData _inputData;
        private HandTrackingData _handTrackingData;

        // Set in Editor to get interaction layers 
        // public LayerMask layerMask;

        protected List<Collider> _leftHitColliders;
        protected List<Collider> _rightHitColliders;

        static float sphereRadius = 0.1f;
        private void SphereCollision(Vector3 position, List<Collider> _hitColliders)
        {
            _hitColliders.Clear();
            Collider[] hitColliders = Physics.OverlapSphere(position, sphereRadius);
            foreach (Collider collider in hitColliders)
            {
                _hitColliders.Add(collider);
            }
        }

#if USE_XR_TOOLKIT

        private XRInputModalityManager.InputMode inputMode;

        // Represents what virtual object the player touches with his hands or controllers
        private void XRTInteractions()
        {
            inputMode = XRInputModalityManager.currentInputMode.Value;

            if (inputMode == XRInputModalityManager.InputMode.MotionController && (_inputData._leftController.isValid || _inputData._rightController.isValid))
            {
                Vector3 lVect;
                Vector3 rVect;
                // Get left hand position
                _inputData._leftController.TryGetFeatureValue(CommonUsages.devicePosition, out lVect);
                SphereCollision(lVect, _leftHitColliders);

                // Get Right hand position
                _inputData._rightController.TryGetFeatureValue(CommonUsages.devicePosition, out rVect);
                SphereCollision(rVect, _rightHitColliders);
            }

            if (inputMode == XRInputModalityManager.InputMode.TrackedHand && (_handTrackingData._leftIsTracked || _handTrackingData._rightIsTracked))
            {
                SphereCollision(_handTrackingData._leftHandPose.position, _leftHitColliders);
                SphereCollision(_handTrackingData._rightHandPose.position, _rightHitColliders);
            }
        }
#endif

#if USE_OVR
        private void OVRInteractions()
        {
            // Check if motion controllers are active
            bool isLeftControllerActive = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            bool isRightControllerActive = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

            // Check if hand tracking is active
            OVRHand leftHand = GetHand(OVRHand.Hand.HandLeft);
            OVRHand rightHand = GetHand(OVRHand.Hand.HandRight);
            bool isLeftHandTracked = leftHand != null && leftHand.IsTracked;
            bool isRightHandTracked = rightHand != null && rightHand.IsTracked;

            if (isLeftControllerActive || isRightControllerActive)
            {
                // Get left hand position
                SphereCollision(_inputData._leftController.position, _leftHitColliders);

                // Get Right hand position
                SphereCollision(_inputData._rightController.position, _rightHitColliders);
            }

            if (isLeftHandTracked || isRightHandTracked)
            {
                SphereCollision(_handTrackingData._leftHandPose.position, _leftHitColliders);
                SphereCollision(_handTrackingData._rightHandPose.position, _rightHitColliders);
            }
        }
#endif

        #endregion

        #region CSV

        #region Create Header

        private string header = "TimeStamp,Scene,Event,Interaction";

        private void Header() { writer.WriteLine(header); }

        #endregion

        #region Create Row

        private string row = string.Empty;

        private void Row()
        {
            _leftHitColliders = new List<Collider>();
            _rightHitColliders = new List<Collider>();
            Events();
#if USE_XR_TOOLKIT
            XRTInteractions();
#elif USE_OVR
            OVRInteractions();
#endif

            string currentTime = DataManager.CurrentTime().ToString()+ ",";
            string eventSystemObject="none,";

            if (_eventSystem.IsPointerOverGameObject())
            {
                if (_eventSystem.currentSelectedGameObject)
                {
                    eventSystemObject = _eventSystem.currentSelectedGameObject.name + ",";
                    //Debug.Log(eventSystemObject);
                }
            }
            else
            {
                _eventSystem.SetSelectedGameObject(null, m_DummyData);
            }

            if (_leftHitColliders.Count == 0 && _rightHitColliders.Count == 0 ) 
            {
                row = currentTime;

                row += _sceneName;

                row += eventSystemObject;

                row += "none,";

                writer.WriteLine(row);
            }
            else
            {
                for (int i = 0; i < _leftHitColliders.Count; i++)
                {
                    row = currentTime;
                    row += _sceneName;
                    row += eventSystemObject;
                    row += _leftHitColliders[i].name + ",";
                    writer.WriteLine(row);
                }
                for (int i = 0; i < _rightHitColliders.Count; i++)
                {
                    row = currentTime;
                    row += _sceneName;
                    row += eventSystemObject;
                    row += _rightHitColliders[i] + ",";
                    writer.WriteLine(row);
                }

            }
        }

#endregion

        #region FilePath

        // Directory where CSV files will be stored
        public string directoryPath = "CSVExports/Events Interactions";

#if USE_XR_TOOLKIT
        // Generate unique file name using timestamp
        string fileName = "XRT_Event_Interaction_Data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
#elif USE_OVR
        string fileName = "OVR_Event_Interaction_Data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
#endif

        // File path
        private string filePath;
        public StreamWriter writer;

        public void InitFile()
        {
            if(DataPath.Instance._eventTrackingFilePath == null)
            {
                filePath = Path.Combine(directoryPath, fileName);
                DataPath.Instance._eventTrackingFilePath = filePath;
                // Open or create file for CSV
                writer = new StreamWriter(filePath);
                DataPath.Instance._eventTrackingFileWriter = writer;
                Header();
            }
            else
            {
                filePath = DataPath.Instance._eventTrackingFilePath;
                writer = DataPath.Instance._eventTrackingFileWriter;
            }
        }


        void OnApplicationQuit()
        {
            // Check if the writer is not null
            if (writer != null)
            {
                // Close the file
                writer.Close();
                Debug.Log("XR_Event_interaction File closed.");
            }
        }

#endregion

#endregion

        #region Updates

        [SerializeField]
        /// <summary>
        /// Capture Data every n frames
        /// </summary>
        public int _framesToSkip = 0;

        private int framesSkipped = 0;

        // Start is called before the first frame update
        void Start()
        {
            // Scene
            _sceneName = SceneManager.GetActiveScene().name +",";

            // Events
            try
            {
                _eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
                m_DummyData = new BaseEventData(_eventSystem);
            }
            catch (Exception e) { Debug.LogException(e); }

            // Interactions
            _inputData = GetComponent<HMDControllersInputData>();
            _handTrackingData = GetComponent<HandTrackingData>();
            _leftHitColliders = new List<Collider>();
            _rightHitColliders = new List<Collider>();

            InitFile();
        }

        // Update is called once per frame
        void Update()
        {
            if (framesSkipped == _framesToSkip)
            {
                Row();
                framesSkipped = 0;
            }
            else
            {
                framesSkipped++;
            }
        }

        #endregion

    }
}
