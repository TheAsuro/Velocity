﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Selectable))]
    public class MenuItem : MonoBehaviour
    {
        public List<KeyCode> nextKeys = new List<KeyCode>() { KeyCode.Tab };
        public Vector3 nextDirection = Vector3.down;

        private void Update()
        {
            foreach (KeyCode nextKey in nextKeys)
            {
                if (Input.GetKeyDown(nextKey))
                {
                    GetComponent<Selectable>().FindSelectable(nextDirection).Select();
                }
            }
        }
    }
}
