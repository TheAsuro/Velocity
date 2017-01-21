using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    public class SpeedCrosshair : MonoBehaviour
    {
        void Update()
        {
            GetComponent<Image>().material.SetFloat("_Speed", GameInfo.info.GetPlayerInfo().Movement.XzVelocity);
        }
    }
}