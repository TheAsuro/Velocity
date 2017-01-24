using UnityEngine;

namespace LevelEditor
{
    public class EditorCam : MonoBehaviour
    {
        public float lookSensitivity = 1f;
        public float camMoveSpeed = 3f;
        public float fastCamMoveSpeed = 10f;

        private float rotationX;
        private float rotationY;

        private void Update()
        {
            //Move faster while shift is pressed
            float currentMoveSpeed = camMoveSpeed;
            if(Input.GetKey(KeyCode.LeftShift))
                currentMoveSpeed = fastCamMoveSpeed;

            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * currentMoveSpeed * Time.deltaTime;
            Quaternion viewRot = transform.rotation;
            transform.position = transform.position + viewRot * input;


            if(Input.GetMouseButton(1))
            {
                MouseRotateCamera();
            }
        }

        private void MouseRotateCamera()
        {
            ClampCamera();

            rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
            rotationY += Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        }

        private void ClampCamera()
        {
            if(rotationX < -360f)
                rotationX += 360f;

            if(rotationX > 360f)
                rotationX -= 360f;

            if(rotationY < -360f)
                rotationY += 360f;

            if(rotationY > 360f)
                rotationY -= 360f;
        }
    }
}
