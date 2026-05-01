using UnityEngine;

namespace GameProgramming.World.Mechanisms
{
    public abstract class MechanismReceiver : MonoBehaviour
    {
        [SerializeField] private bool startPowered;
        [SerializeField] private bool invertSignal;

        protected bool CurrentVisualState { get; private set; }

        protected virtual void Awake()
        {
            ApplyResolvedState(startPowered);
        }

        public void SetPowered(bool powered)
        {
            ApplyResolvedState(powered);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyResolvedState(startPowered);
            }
        }

        private void ApplyResolvedState(bool powered)
        {
            CurrentVisualState = invertSignal ? !powered : powered;
            ApplyState(CurrentVisualState);
        }

        protected abstract void ApplyState(bool isActive);
    }
}
