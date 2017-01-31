using UnityEngine;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation
/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.
/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MOUSE_X_AND_Y = 0,
        MOUSE_X = 1,
        MOUSE_Y = 2
    }

    public RotationAxes axes = RotationAxes.MOUSE_X_AND_Y;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public bool invertY = false;

    private float rotationY = 0F;

    private void Update()
    {
        float ySens = sensitivityY;
        if (invertY)
        {
            ySens *= -1f;
        }

        if (axes == RotationAxes.MOUSE_X_AND_Y)
        {
            float rotationX = transform.localEulerAngles.y + GetMouseX() * sensitivityX;

            rotationY += GetMouseY() * ySens;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MOUSE_X)
        {
            transform.Rotate(0, GetMouseX() * sensitivityX, 0);
        }
        else
        {
            rotationY += GetMouseY() * ySens;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
    }

    private void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    private static float GetMouseX()
    {
        return Settings.GameSettings.SingletonInstance.RawMouse.value ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X");
    }

    private static float GetMouseY()
    {
        return Settings.GameSettings.SingletonInstance.RawMouse.value ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y");
    }
}