using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using XRIDataCollection;

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

        private EventSystem Events()
        {
            return _eventSystem;
        }

        #endregion

        #region Interaction Listener
        /// <summary>
        /// Get the virtual object the player is touching
        /// </summary>
        
        private HMDControllersInputData _inputData;
        private HandTrackingData _handTrackingData;

        private XRInputModalityManager.InputMode inputMode;

        // Set in Editor to get interaction layers 
        // public LayerMask layerMask;

        protected List<Collider> _leftHitColliders;
        protected List<Collider> _rightHitColliders;

        static float sphereRadius = 0.1f;
        private void SphereCollision(Vector3 position, List<Collider> _hitColliders)
        {
            _hitColliders.Clear();
            Collider[] hitColliders = Physics.OverlapSphere(position, sphereRadius/*, layerMask*/);
            foreach (Collider collider in hitColliders)
            {
                _hitColliders.Add(collider);
            }
        }

        // Represents what virtual object the player touches with his hands or controllers
        private void Interactions()
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
        #endregion

        #region CSV

        #region Create Header

        private string header = "TimeStamp,Event,Interaction";

        private void Header() { writer.WriteLine(header); }

        #endregion

        #region Create Row

        private string row = string.Empty;

        private void Row()
        {
            _leftHitColliders = new List<Collider>();
            _rightHitColliders = new List<Collider>();
            Events();
            Interactions();

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

                row += eventSystemObject;

                row += "none,";

                writer.WriteLine(row);
            }
            else
            {
                for (int i = 0; i < _leftHitColliders.Count; i++)
                {
                    row = currentTime;
                    row += eventSystemObject;
                    row += _leftHitColliders[i].name + ",";
                    writer.WriteLine(row);
                }
                for (int i = 0; i < _rightHitColliders.Count; i++)
                {
                    row = currentTime;
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

        // Generate unique file name using timestamp
        string fileName = "XR_Event_Interaction_Data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        // File path
        private string filePath;
        public StreamWriter writer;

        public void InitFile()
        {
            filePath = Path.Combine(directoryPath, fileName);

            // Open or create file for CSV
            writer = new StreamWriter(filePath);

            Header();
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
