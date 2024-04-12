using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTest : MonoBehaviour
{
    private BaseInputModule m_CurrentInputModule;

    #region Event Listener
    /// <summary>
    /// Get the events happening in the scene (Button pressing, UI,...)
    /// </summary>
    private EventSystem _eventSystem;

    private EventSystem Events()
    {
        return _eventSystem;
    }

    #endregion

    private BaseEventData m_DummyData;

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
    }

    // Update is called once per frame
    void Update()
    {
        string eventSystemObject;
        ChangeEventModule(m_CurrentInputModule);

        if (_eventSystem.currentSelectedGameObject)
        {
            if (_eventSystem.IsPointerOverGameObject())
            {
                eventSystemObject = _eventSystem.currentSelectedGameObject.name;
                Debug.Log(eventSystemObject);
                Debug.Log(eventSystemObject + " a select ");
            }
            else
            {
                _eventSystem.SetSelectedGameObject(null, m_DummyData);
            }
        }
        else { eventSystemObject = "none,"; }

    }

    private void ChangeEventModule(BaseInputModule module)
    {
        if (m_CurrentInputModule == module)
            return;

        if (m_CurrentInputModule != null)
            m_CurrentInputModule.DeactivateModule();

        if (module != null)
            module.ActivateModule();
        m_CurrentInputModule = module;
    }
}
