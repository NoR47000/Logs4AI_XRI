using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Hands.Samples.VisualizerSample;

public class EventListener : MonoBehaviour
{
    public EventSystem _system;

    // Start is called before the first frame update
    void Start()
    {
        _system = GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_system != null)
        {
            //UI Event
            Debug.Log("System :" + _system);
        }
    }

}
