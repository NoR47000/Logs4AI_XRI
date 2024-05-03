using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;


namespace XRIDataCollection
{
    [RequireComponent(typeof(HMDControllersInputData))]
    [RequireComponent(typeof(HandTrackingData))]
    public class DataManager : MonoBehaviour
    {
        #region Global Variables

        private HMDControllersInputData _inputData;
        private HandTrackingData _handTrackingData;

        private XRInputModalityManager.InputMode inputMode;

        // Number of joints in one Hand, Change if  updated
        int nbOfJoints = 26/*_handTrackingData._leftHandJointPoses.Count*/;

        #endregion

        #region Write CSV

        #region Create Header

        private List<string> poseLabels = new List<string>{
            "PositionX","PositionY","PositionZ",
            "RotationX","RotationY","RotationZ","RotationW"};

        private string header = string.Empty;
        //Adds List of labels for pose to header (example : label = LeftHand -> Add ("leftHand position X, leftHand position Y, ..., leftHand Rotation W") to header )  
        private void DataLabelPose(string label)
        {
            foreach (string poseLabel in poseLabels)
                header += label + "_" + poseLabel + ",";
        }

        private List<string> velocityLabels = new List<string>{
            "VelocityX","VelocityY","VelocityZ"};
        private void DataLabelVelocity(string label)
        {
            foreach (string velocityLabel in velocityLabels)
                header += label + "_" + velocityLabel + ",";
        }

        // Write the Header for the CSV file, the order should always be the same as well as the number of data per row
        private void Header()
        {
            // TimeStamp
            header = "TimeStamp,";
            // HMD 
            DataLabelPose("HMD");
            DataLabelVelocity("HMD");
            // Controllers
            DataLabelPose("left_Controller");
            DataLabelVelocity("left_Controller");
            DataLabelPose("right_Controller");
            DataLabelVelocity("right_Controller");
            // Hands 
            DataLabelPose("left_Hand");
            DataLabelPose("right_Hand");
            // Hand Joints
            for (int i = 0; i < nbOfJoints; i++)
            {
                DataLabelPose("L_"+XRHandJointIDUtility.FromIndex(i).ToString());
            }
            for (int i = 0; i < nbOfJoints; i++)
            {
                DataLabelPose("R_" + XRHandJointIDUtility.FromIndex(i).ToString());
            }

        }

        #endregion

        #region Create Data Row

        private string row = string.Empty;

        private void Row()
        {
            // Initialize Row
            row = CurrentTime().ToString() + ",";

            inputMode = XRInputModalityManager.currentInputMode.Value;

            // HMD 
            Pose hmdPose = new Pose();
            TryGetValue(_inputData._HMD,CommonUsages.devicePosition,out hmdPose.position);
            TryGetValue(_inputData._HMD, CommonUsages.deviceRotation, out hmdPose.rotation);
            DataPose(hmdPose);
            Vector3 hmdVelocity = new Vector3();
            TryGetValue(_inputData._HMD, CommonUsages.deviceVelocity, out hmdVelocity);
            DataVelocity(hmdVelocity);

            // CONTROLLERS
            if (inputMode == XRInputModalityManager.InputMode.MotionController && (_inputData._leftController.isValid || _inputData._rightController.isValid))
            {
                Pose cPose = new Pose();
                Vector3 cVelocity = new Vector3();

                // Left
                TryGetValue(_inputData._leftController, CommonUsages.devicePosition, out cPose.position);// Left Controller Position
                TryGetValue(_inputData._leftController, CommonUsages.deviceRotation, out cPose.rotation);// Left Controller Rotation
                DataPose(cPose);
                TryGetValue(_inputData._HMD, CommonUsages.deviceVelocity, out cVelocity);
                DataVelocity(cVelocity);

                //Right
                TryGetValue(_inputData._rightController, CommonUsages.devicePosition, out cPose.position);// Right Controller Position
                TryGetValue(_inputData._rightController, CommonUsages.deviceRotation, out cPose.rotation);// Right Controller Rotation
                DataPose(cPose);
                TryGetValue(_inputData._HMD, CommonUsages.deviceVelocity, out cVelocity);
                DataVelocity(cVelocity);
            }
            else
            {
                // Untracked Controller Poses
                DataPose(Pose.identity);// Left
                DataVelocity(Vector3.zero);
                DataPose(Pose.identity);// Right
                DataVelocity(Vector3.zero);
            }
            
            // HANDS
            if(inputMode == XRInputModalityManager.InputMode.TrackedHand && (_handTrackingData._leftIsTracked || _handTrackingData._rightIsTracked))
            {
                // Hands 
                DataPose(_handTrackingData._leftHandPose);// Left
                DataPose(_handTrackingData._rightHandPose);// Right
                // Hand Joints
                for (/*Left*/int i = 0; i < nbOfJoints; i++)
                {
                    DataPose(_handTrackingData._leftHandJointPoses[i]);
                }
                for (/*Rigth*/int i = 0; i < nbOfJoints; i++)
                {
                    DataPose(_handTrackingData._rightHandJointPoses[i]);
                }
            }
            else 
            {
                // Untracked Hand Poses
                DataPose(Pose.identity);// Left
                DataPose(Pose.identity);// Right
                // Untracked Hand Joint Poses
                for (/*Left and Right*/int i = 0; i < 2 * nbOfJoints; i++)
                {
                    DataPose(Pose.identity);
                }
            }
        }

        private void DataPose(Pose pose)
        {
            row += pose.position.x + "," +
                    pose.position.y + "," +
                    pose.position.z + "," +
                    pose.rotation.x + "," +
                    pose.rotation.y + "," +
                    pose.rotation.z + "," +
                    pose.rotation.w + ",";
        }

        private void DataVelocity(Vector3 vect)
        {
            row += vect.x + "," +
                    vect.y + "," +
                    vect.z + ",";
        }


        // Get value depending on InputFeatureUsage
        private Vector3 TryGetValue(InputDevice inputDevice, InputFeatureUsage<Vector3> inputFeatureUsage, out Vector3 vect)
        {
            if (inputDevice.TryGetFeatureValue(inputFeatureUsage, out vect))
            {
                return vect;
            }
            else { vect = Vector3.zero; return vect; }
        }
        private Quaternion TryGetValue(InputDevice inputDevice, InputFeatureUsage<Quaternion> inputFeatureUsage, out Quaternion quat)
        {
            if (inputDevice.TryGetFeatureValue(inputFeatureUsage, out quat))
            {
                return quat;
            }
            else { quat = Quaternion.identity; return quat; }
        }

        #endregion

        #region FilePath
        // Directory where CSV files will be stored
        public string directoryPath = "CSVExports/XRTracking";

        // Generate unique file name using timestamp
        string fileName = "XR_Tracking_Data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        // File path
        private string filePath;
        public StreamWriter writer;

        public void InitFile()
        {
            if(DataPath.Instance._XRTrackingFilePath == null)
            {
                filePath = Path.Combine(directoryPath, fileName);
                DataPath.Instance._XRTrackingFilePath = filePath;
                // Open or create file for CSV
                writer = new StreamWriter(filePath);
                DataPath.Instance._XRTrackingFileWriter = writer;

                Header();
                writer.WriteLine(header);

                Debug.Log("NEW FILE PATH");
            }
            else
            {
                Debug.Log("FOUND FILE PATH");
                filePath = DataPath.Instance._XRTrackingFilePath;
                writer = DataPath.Instance._XRTrackingFileWriter;
            }
        }


        // Export Data to CSV
        public void ExportToCSV()
        {
            Row();
            // Write Data for each row on CSV file
            writer.WriteLine(row);
        }

        void OnApplicationQuit()
        {
            // Check if the writer is not null
            if (writer != null)
            {
                // Close the file
                writer.Close();
                Debug.Log("XR_Data File closed.");
            }
        }
        #endregion

        #endregion

        #region Updates

        public delegate void onUpdate(float deltaTime);

        public static event onUpdate OnUpdate;
        private static void InvokeUpdateEvent(float deltaTime) { if (OnUpdate != null) { OnUpdate(deltaTime); } }

        [SerializeField]
        /// <summary>
        /// Capture Data every n frames
        /// </summary>
        public int _framesToSkip = 0;

        private int framesSkipped = 0;

        private void Start()
        {
            _inputData = GetComponent<HMDControllersInputData>();
            _handTrackingData = GetComponent<HandTrackingData>();

            InitFile();
        }

        void Update()
        {
            if(framesSkipped == _framesToSkip)
            {
                ExportToCSV();
                framesSkipped = 0;
            }
            else
            {
                framesSkipped++;
            }
            //InvokeUpdateEvent(CurrentTime());

        }

        #endregion

        #region Current time since epoch ( 1970-1-1 midnight )
        public static long CurrentTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        #endregion
    }
}

