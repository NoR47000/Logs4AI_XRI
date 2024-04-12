using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using XRIDataCollection;

namespace XRIDataCollection
{
    public class EventInteractionData : MonoBehaviour
    {
        #region Event Listener
        /// <summary>
        /// Get the events happening in the scene (Button pressing, UI,...)
        /// </summary>
        private EventSystem _system;

        private EventSystem Events()
        {
            return _system;
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
        public LayerMask layerMask;

        protected List<Collider> _leftHitColliders;
        protected List<Collider> _rightHitColliders;

        static float sphereRadius = 0.1f;
        private void SphereCollision(Vector3 position, List<Collider> _hitColliders)
        {
            _hitColliders.Clear();
            Collider[] hitColliders = Physics.OverlapSphere(position, sphereRadius, layerMask);
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
            if(_leftHitColliders.Count == 0 && _rightHitColliders.Count == 0 ) 
            {
                row = DataManager.CurrentTime().ToString() + ",";
                row += _system.ToShortString() +",";
                row += "none";

                writer.WriteLine(row);
            }
            else
            {
                for(int i = 0; i < _leftHitColliders.Count; i++)
                {
                    row = DataManager.CurrentTime().ToString() + ",";
                    row += _system + ",";
                    row += _leftHitColliders.ToString();
                }
                for (int i = 0; i < _rightHitColliders.Count; i++)
                {
                    row = DataManager.CurrentTime().ToString() + ",";
                    row += _system + ",";
                    row += _rightHitColliders.ToString();
                }

                writer.WriteLine(row);
            }
        }

        #endregion

        void OnApplicationQuit()
        {
            // Check if the writer is not null
            if (writer != null)
            {
                // Close the file
                writer.Close();
                Debug.Log("File closed.");
            }
        }
        #endregion

        #region Updates

        // Start is called before the first frame update
        void Start()
        {
            // Events
            try
            {
                _system = GameObject.Find("EventSystem").GetComponent<EventSystem>();
            }
            catch(Exception e) { Debug.LogException(e); }

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
            Row();
        }

        #endregion

    }
}
