using UnityEngine;

namespace Demos
{
    [System.Serializable]
    public struct DemoTick
    {
        public long Time { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public bool Crouched { get; private set; }

        public DemoTick(long time, Vector3 pos, Quaternion rot, bool crouched) : this()
        {
            Time = time;
            Position = pos;
            Rotation = rot;
            Crouched = crouched;
        }
    }
}
