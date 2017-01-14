using UnityEngine;

namespace Util
{
    public static class UnityUtils
    {
        //Casts a ray from the camera into the direction of the mouse cursor
        public static bool MouseRaycast(LayerMask layers, out RaycastHit hit, Camera camera, float length = Mathf.Infinity)
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane);
            Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
            Vector3 camPos = camera.transform.position;
            Vector3 rayDirection = worldPos - camPos;
            Ray clickRay = new Ray(camPos, rayDirection);
            return Physics.Raycast(clickRay, out hit, length, layers);
        }
    }
}
