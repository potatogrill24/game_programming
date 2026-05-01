using GameProgramming.Player;
using UnityEngine;

namespace GameProgramming.Game
{
    [RequireComponent(typeof(Collider))]
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] private GameStateController gameStateController;

        private void Reset()
        {
            Collider goalCollider = GetComponent<Collider>();
            goalCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<AstronautController>() == null)
            {
                return;
            }

            GameStateController controller = gameStateController != null ? gameStateController : GameStateController.Instance;
            if (controller != null)
            {
                controller.CompleteLevel();
            }
        }
    }
}
