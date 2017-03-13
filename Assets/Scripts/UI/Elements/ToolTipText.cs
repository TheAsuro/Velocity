using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Util;

namespace UI.Elements
{
    [RequireComponent(typeof(EventTrigger))]
    public class ToolTipText : MonoBehaviour
    {
        [SerializeField] private string tooltipText;

        private void Awake()
        {
            GetComponent<EventTrigger>().CreateEntry(EventTriggerType.PointerEnter, data => GameMenu.SingletonInstance.ShowTooltip(tooltipText));
            GetComponent<EventTrigger>().CreateEntry(EventTriggerType.PointerExit, data => GameMenu.SingletonInstance.HideTooltip());
        }
    }
}