using System;
using System.Collections;
using Api;
using UnityEngine;
using Object = UnityEngine.Object;

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

        //Create a md5 hash from a string
        public static string Md5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);

            //encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            //Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            foreach (byte hashByte in hashBytes)
            {
                hashString += Convert.ToString(hashByte, 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        public static IEnumerator RunWhenDone<T>(T request, Action<T> action) where T : Request
        {
            while (!request.Done)
                yield return null;
            action(request);
        }

        public static IEnumerator WaitForRemove(Object obj, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Object.Destroy(obj);
        }

        public static string ShortText(this TimeSpan time)
        {
            string result = "";
            if (time.Days > 0)
                result += time.Days.ToString("0") + "d ";
            if (time.Days > 0 || time.Hours > 0)
                result += time.Hours.ToString("00") + ":";
            result += time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00") + ":" + time.Milliseconds.ToString("00");
            return result;
        }
    }
}
