using GameProgramming.Player;
using GameProgramming.Game;
using UnityEngine;

namespace GameProgramming.World.Mechanisms
{
    public class LaserBarrier : MechanismReceiver
    {
        [SerializeField] private GameObject[] visualRoots;
        [SerializeField] private Collider[] damageColliders;
        private bool isBlocking;

        protected override void ApplyState(bool isActive)
        {
            isBlocking = isActive;

            if (visualRoots != null)
            {
                foreach (GameObject visualRoot in visualRoots)
                {
                    if (visualRoot != null)
                    {
                        visualRoot.SetActive(isActive);
                    }
                }
            }

            if (damageColliders != null)
            {
                foreach (Collider damageCollider in damageColliders)
                {
                    if (damageCollider != null)
                    {
                        damageCollider.enabled = isActive;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBlocking)
            {
                return;
            }

            PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();
            if (playerRespawn == null)
            {
                return;
            }

            playerRespawn.Respawn();
        }
    }
}
