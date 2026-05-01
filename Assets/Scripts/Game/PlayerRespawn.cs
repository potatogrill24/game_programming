using GameProgramming.Core;
using UnityEngine;

namespace GameProgramming.Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Transform currentCheckpoint;
        [SerializeField] private bool useInitialPositionAsDefault = true;
        [SerializeField] private EnergyColor respawnColor = EnergyColor.Yellow;
        [SerializeField] private bool dropCarriedCubeOnRespawn = true;

        private CharacterController characterController;
        private GameProgramming.Player.AstronautEnergy astronautEnergy;
        private GameProgramming.Player.AstronautInteractor astronautInteractor;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            astronautEnergy = GetComponent<GameProgramming.Player.AstronautEnergy>();
            astronautInteractor = GetComponent<GameProgramming.Player.AstronautInteractor>();

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            if (currentCheckpoint == null && useInitialPositionAsDefault)
            {
                currentCheckpoint = transform;
            }
        }

        public void SetCheckpoint(Transform checkpointTransform)
        {
            currentCheckpoint = checkpointTransform;
        }

        public void Respawn()
        {
            if (dropCarriedCubeOnRespawn && astronautInteractor != null && astronautInteractor.IsCarryingCube)
            {
                astronautInteractor.ForceDropCube();
            }

            Vector3 targetPosition = initialPosition;
            Quaternion targetRotation = initialRotation;

            if (currentCheckpoint != null && currentCheckpoint != transform)
            {
                targetPosition = currentCheckpoint.position;
                targetRotation = currentCheckpoint.rotation;
            }

            characterController.enabled = false;
            transform.SetPositionAndRotation(targetPosition, targetRotation);
            characterController.enabled = true;

            if (astronautEnergy != null)
            {
                astronautEnergy.SetColor(respawnColor);
            }
        }
    }
}
