using GameProgramming.Player;
using UnityEngine;

namespace GameProgramming.World.Interaction
{
    public abstract class InteractableBase : MonoBehaviour
    {
        public abstract void Interact(AstronautInteractor interactor);
    }
}
