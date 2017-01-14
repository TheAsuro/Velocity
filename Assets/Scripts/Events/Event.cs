using UnityEngine;

namespace Events
{
    public abstract class Event : MonoBehaviour
    {
        public virtual void Fire(params object[] parameters)
        {

        }

        public virtual void Reset()
        {
		
        }
    }
}