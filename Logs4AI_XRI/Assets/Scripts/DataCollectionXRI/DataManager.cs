using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace XRIDataCollection
{
    public class DataManager : MonoBehaviour
    {
        #region Get Data



        private HMDControllersInputData _inputData;
        private HandTrackingData _handTrackingData;

        private List<Pose> _leftHandPoses;
        private List<Pose> _rightHandPoses;

        private XRInputModalityManager.InputMode inputMode;

        private void HandInput()
        {
            inputMode = XRInputModalityManager.currentInputMode.Value;

            // Hand and Controllers
            if (inputMode == XRInputModalityManager.InputMode.MotionController && (_inputData._leftController.isValid || _inputData._rightController.isValid))
            {
                CSVPoseData(_inputData._leftController);
                CSVPoseData(_inputData._rightController);
            }
            if (inputMode == XRInputModalityManager.InputMode.TrackedHand && (_handTrackingData._leftIsTracked || _handTrackingData._rightIsTracked))
            {
                //Position, Rotation, Velocity, Angul�rVelocity of Hand
                //Position,Rotation of joints
            }
        }

        ///<summary>
        /// Add device Data to the CSV file 
        ///</summary>
        private void CSVPoseData(InputDevice inputDevice)
        {
            //Position, Rotation, Velocity, AngularVelocity of Hand

            ///////CONTINUER
            //Position,Rotation of joints
            ////CONTINUER
        }

        /// <summary>
        /// Get Device Data
        /// calls CSVPoseData to print on file
        /// </summary>
        private void GetDeviceData()
        {

        }


        private List<string> poseLabels = new List<string>{
            "Position X","Position Y","Position Z",
            "Rotation X","Rotation Y","Rotation Z","Rotation W"};

        private string header = string.Empty;
        //Adds List of labels for pose to header (example : label = LeftHand -> Add ("leftHand position X, leftHand position Y, ..., leftHand Rotation W") to header )  
        private void DataLabelPose(string label)
        {
            foreach(string poseLabel in poseLabels)
                header += label +" "+poseLabel+",";
        }


        #endregion

        #region Write CSV
        // Liste des poses � exporter
        public List<Pose> poses = new List<Pose>();

        // Directory where CSV files will be stored
        public string directoryPath = "CSVExports";


        // Generate unique file name using timestamp
        string fileName = "hand_tracking_data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        // M�thode pour exporter les donn�es de poses vers un fichier CSV
        public void ExportPoseDataToCSV()
        {
            // V�rifier s'il y a des donn�es de pose � exporter
            if (poses.Count == 0)
            {
                Debug.LogWarning("La liste des poses est vide. Aucune donn�e � exporter.");
                return;
            }

            string filePath = Path.Combine(directoryPath, fileName);

            // Ouvrir ou cr�er le fichier CSV
            StreamWriter writer = new StreamWriter(filePath);

            // En-t�te du fichier CSV
            writer.WriteLine("Position X,Position Y,Position Z,Rotation X,Rotation Y,Rotation Z,Rotation W");

            // �crire les donn�es de chaque pose dans le fichier CSV
            foreach (Pose pose in poses)
            {
                writer.WriteLine(
                    pose.position.x + "," +
                    pose.position.y + "," +
                    pose.position.z + "," +
                    pose.rotation.x + "," +
                    pose.rotation.y + "," +
                    pose.rotation.z + "," +
                    pose.rotation.w
                );
            }

            // Fermer le fichier CSV
            writer.Close();

            Debug.Log("Les donn�es de poses ont �t� export�es vers " + filePath);
        }
        #endregion

        #region Updates

        public delegate void onUpdate(float deltaTime);

        public static event onUpdate OnUpdate;
        private static void InvokeUpdateEvent(float deltaTime) { if (OnUpdate != null) { OnUpdate(deltaTime); } }

        void Update()
        {
            InvokeUpdateEvent(CurrentTime());
            inputMode = XRInputModalityManager.currentInputMode.Value;
        }
        #endregion

        #region Current time since epoch ( 1970-1-1 midnight )
        public static long CurrentTime()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        #endregion
    }
}

