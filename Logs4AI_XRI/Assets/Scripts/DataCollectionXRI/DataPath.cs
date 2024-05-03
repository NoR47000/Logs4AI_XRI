using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataPath : MonoBehaviour
{
    public static DataPath Instance;

    public string _XRTrackingFilePath { get; set; }
    public string _eventTrackingFilePath { get; set; }
    public StreamWriter _XRTrackingFileWriter { get; set; }
    public StreamWriter _eventTrackingFileWriter { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DataPath instance created and set to DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log("DataPath instance already exists. Destroying this new instance.");
            Destroy(gameObject);
        }
    }
}
