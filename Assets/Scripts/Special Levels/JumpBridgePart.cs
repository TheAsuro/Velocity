using UnityEngine;

public class JumpBridgePart : MonoBehaviour
{
    [SerializeField]
    private JumpBridge bridge;

    void Start()
    {
        bridge.RegisterPart(gameObject);
    }

    void OnCollisionEnter()
    {
        bridge.CollisionEnter(gameObject);
    }

    void OnCollisionExit()
    {
        bridge.CollisionLeave(gameObject);
    }
}
