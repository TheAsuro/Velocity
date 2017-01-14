using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class PlayerCollisionEventTrigger : MonoBehaviour
    {
        public List<Event> eventComponent;

        private void OnTriggerEnter(Collider col)
        {
            if(col.tag.Equals("Player"))
            {
                eventComponent.ForEach(comp => comp.Fire());
            }
        }
    }
}
