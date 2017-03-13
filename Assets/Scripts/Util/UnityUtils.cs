using System;
using System.Collections;
using System.Security.Policy;
using Api;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

        public static void RunSynchronously(this IEnumerator enumerator, int maxCalls = 0)
        {
            int callCount = 0;
            bool aborted = false;
            while (enumerator.MoveNext())
            {
                callCount++;
                if (maxCalls > 0 && callCount >= maxCalls)
                {
                    aborted = true;
                    break;
                }
            }
            Assert.IsFalse(aborted);
        }

        public static void CreateEntry(this EventTrigger eventTrigger, EventTriggerType triggerType, UnityAction<BaseEventData> action)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener(action);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = triggerType,
                callback = trigger,
            };
            eventTrigger.triggers.Add(entry);
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Content { get; private set; }
        public bool Error { get; private set; }
        public string ErrorText { get; private set; }

        public EventArgs(T content) : base()
        {
            Content = content;
            Error = false;
        }

        public EventArgs(T content, bool error, string errorText) : base()
        {
            Content = content;
            Error = error;
            ErrorText = errorText;
        }
    }
}
