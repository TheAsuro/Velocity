using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    public class SpeedCrosshair : MonoBehaviour
    {
        private void Update()
        {
            GetComponent<Image>().material.SetFloat("_Speed", WorldInfo.info.RaceScript.Movement.XzVelocity);
        }
    }
}