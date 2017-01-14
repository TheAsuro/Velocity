using UnityEngine;

namespace Demos
{
    [System.Serializable]
    public class DemoTick
    {
        private decimal time;
        private Vector3 position;
        private Quaternion rotation;

        public DemoTick(decimal recordtime, Vector3 pos, Quaternion rot)
        {
            time = recordtime;
            position = pos;
            rotation = rot;
        }

        public void SetTime(decimal value)
        {
            time = value;
        }

        public decimal GetTime()
        {
            return time;
        }

        public void SetPosition(Vector3 value)
        {
            position = value;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void SetRotation(Quaternion value)
        {
            rotation = value;
        }

        public Quaternion GetRotation()
        {
            return rotation;
        }
    }
}
