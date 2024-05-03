using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataPath : MonoBehaviour
{
    public static DataPath Instance;

    public string FilePath { get; set; }
    public StreamWriter FileWriter { get; set; }

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
