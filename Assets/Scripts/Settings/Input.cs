using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Settings
{
    public static class Input
    {
        public enum OtherCode
        {
            MOUSE_WHEEL_UP = -1,
            MOUSE_WHEEL_DOWN = -2
        }

        private static Dictionary<int, List<Action>> actions = new Dictionary<int, List<Action>>();

        public static void BindKey(int code, Action action)
        {
            if (!actions.ContainsKey(code))
                actions[code] = new List<Action>();

            actions[code].Add(action);
        }

        public static void UnbindKey(int code)
        {
            actions.Remove(code);
        }

        public static void UnbindAll()
        {
            actions.Clear();
        }

        public static void ExecuteBoundActions()
        {
            foreach (var kvp in actions.Where((pair) => KeyDown(pair.Key)))
            {
                kvp.Value.ForEach((action) => action());
            }
        }

        public static bool Key(int inputCode)
        {
            if (Enum.IsDefined(typeof(KeyCode), inputCode))
            {
                switch ((KeyCode)inputCode)
                {
                    case KeyCode.Mouse0:
                        return UnityEngine.Input.GetMouseButton(0);
                    case KeyCode.Mouse1:
                        return UnityEngine.Input.GetMouseButton(1);
                    case KeyCode.Mouse2:
                        return UnityEngine.Input.GetMouseButton(2);
                    default:
                        return UnityEngine.Input.GetKey((KeyCode)inputCode);
                }
            }
            else
            {
                if (Enum.IsDefined(typeof(OtherCode), inputCode))
                {
                    switch ((OtherCode)inputCode)
                    {
                        case OtherCode.MOUSE_WHEEL_UP:
                            return UnityEngine.Input.mouseScrollDelta.y > 0f;
                        case OtherCode.MOUSE_WHEEL_DOWN:
                            return UnityEngine.Input.mouseScrollDelta.y < 0f;
                    }
                }

                throw new InvalidOperationException("The input code '" + inputCode + "' is not defined!");
            }
        }

        public static bool KeyDown(int inputCode)
        {
            if (Enum.IsDefined(typeof(KeyCode), inputCode))
            {
                switch ((KeyCode)inputCode)
                {
                    case KeyCode.Mouse0:
                        return UnityEngine.Input.GetMouseButtonDown(0);
                    case KeyCode.Mouse1:
                        return UnityEngine.Input.GetMouseButtonDown(1);
                    case KeyCode.Mouse2:
                        return UnityEngine.Input.GetMouseButtonDown(2);
                    default:
                        return UnityEngine.Input.GetKey((KeyCode)inputCode);
                }
            }
            else
            {
                if (Enum.IsDefined(typeof(OtherCode), inputCode))
                {
                    switch ((OtherCode)inputCode)
                    {
                        case OtherCode.MOUSE_WHEEL_UP:
                            return UnityEngine.Input.mouseScrollDelta.y > 0f;
                        case OtherCode.MOUSE_WHEEL_DOWN:
                            return UnityEngine.Input.mouseScrollDelta.y < 0f;
                    }
                }

                throw new InvalidOperationException("The input code '" + inputCode + "' is not defined!");
            }
        }
    }
}