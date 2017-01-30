using UnityEngine;

namespace Demos
{
    [System.Serializable]
    public class DemoTick
    {
        public long Time { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public DemoTick(long time, Vector3 pos, Quaternion rot)
        {
            Time = time;
            Position = pos;
            Rotation = rot;
        }
    }
}
