using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class StartLevelEventTrigger : MonoBehaviour
    {
        public List<Event> eventComponent;

        private void Start()
        {
            WorldInfo.info.AddStartMethod(DoTrigger, "start level event trigger");
        }

        private void DoTrigger()
        {
            eventComponent.ForEach(comp => comp.Fire());
        }
    }
}
