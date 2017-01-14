using UnityEngine;

namespace Special_Levels
{
    public class JumpBridgePart : MonoBehaviour
    {
        [SerializeField]
        private JumpBridge bridge;

        private void Start()
        {
            bridge.RegisterPart(gameObject);
        }

        private void OnCollisionEnter()
        {
            bridge.CollisionEnter(gameObject);
        }

        private void OnCollisionExit()
        {
            bridge.CollisionLeave(gameObject);
        }
    }
}
