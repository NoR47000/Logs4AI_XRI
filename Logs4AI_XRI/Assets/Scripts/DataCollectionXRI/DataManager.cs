using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRIDataCollection
{
    public class DataManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        #region Updates

        public delegate void onUpdate(float deltaTime);

        public static event onUpdate OnUpdate;
        private static void InvokeUpdateEvent(float deltaTime) { if (OnUpdate != null) { OnUpdate(deltaTime); } }

        void Update()
        {

            InvokeUpdateEvent(CurrentTime());
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

