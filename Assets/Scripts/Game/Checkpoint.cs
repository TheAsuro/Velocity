using System;
using UnityEngine;
using Util;

namespace Game
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int index = 0;

        public int Index
        {
            get { return index; }
        }

        [SerializeField] private bool respawnPositionIsRelative = true;
        [SerializeField] private bool respawnRotationIsRelative = true;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private float respawnYRotation = 0f;

        public event EventHandler<EventArgs<Checkpoint>> OnPlayerTrigger;

        private void Start()
        {
            WorldInfo.info.AddCheckpoint(this);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag.Equals("Player") && OnPlayerTrigger != null)
            {
                OnPlayerTrigger(this, new EventArgs<Checkpoint>(this));
            }
        }

        public Vector3 GetSpawnPos()
        {
            if (respawnPositionIsRelative)
                return transform.position + respawnPosition;

            return respawnPosition;
        }

        public Quaternion GetSpawnRot()
        {
            Vector3 respawnRotation = new Vector3(0f, respawnYRotation, 0f);
            if (respawnRotationIsRelative)
                return transform.rotation * Quaternion.Euler(respawnPosition);

            return Quaternion.Euler(respawnRotation);
        }
    }
}