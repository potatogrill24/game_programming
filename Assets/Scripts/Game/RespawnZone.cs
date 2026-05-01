using UnityEngine;

namespace GameProgramming.Game
{
    [RequireComponent(typeof(Collider))]
    public class RespawnZone : MonoBehaviour
    {
        [SerializeField] private bool affectPlayer = true;
        [SerializeField] private bool affectResettableObjects = true;

        private void Reset()
        {
            Collider zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (affectPlayer)
            {
                PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();
                if (playerRespawn != null)
                {
                    playerRespawn.Respawn();
                    return;
                }
            }

            if (affectResettableObjects)
            {
                ResettableTransform resettable = other.GetComponentInParent<ResettableTransform>();
                if (resettable != null)
                {
                    resettable.ResetState();
                }
            }
        }
    }
}
