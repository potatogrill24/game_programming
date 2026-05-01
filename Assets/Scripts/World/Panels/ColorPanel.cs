using System.Collections;
using GameProgramming.Core;
using GameProgramming.Player;
using GameProgramming.World.Interaction;
using GameProgramming.World.Mechanisms;
using UnityEngine;

namespace GameProgramming.World.Panels
{
    public class ColorPanel : InteractableBase
    {
        [SerializeField] private EnergyColor requiredColor = EnergyColor.Yellow;
        [SerializeField] private Renderer[] panelRenderers;
        [SerializeField] private MechanismReceiver[] targets;
        [SerializeField] private bool activateOnTouch = true;
        [SerializeField] private bool toggleMode = true;
        [SerializeField] private bool singleUse;
        [SerializeField] private float resetDelay;
        [SerializeField, Range(0f, 5f)] private float idleEmission = 0.5f;
        [SerializeField, Range(0f, 5f)] private float activeEmission = 2f;

        private bool isActive;

        private void Awake()
        {
            RefreshVisuals();
        }

        public override void Interact(AstronautInteractor interactor)
        {
            if (interactor == null)
            {
                return;
            }

            TryActivate(interactor.CurrentColor, allowToggle: true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!activateOnTouch)
            {
                return;
            }

            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                TryActivate(astronaut.CurrentColor, allowToggle: false);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!activateOnTouch)
            {
                return;
            }

            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                TryActivate(astronaut.CurrentColor, allowToggle: false);
            }
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSeconds(resetDelay);
            SetActiveState(false);
        }

        private void OnValidate()
        {
            RefreshVisuals();
        }

        private void SetActiveState(bool active)
        {
            isActive = active;

            if (targets != null)
            {
                foreach (MechanismReceiver target in targets)
                {
                    if (target != null)
                    {
                        target.SetPowered(isActive);
                    }
                }
            }

            RefreshVisuals();
        }

        private void TryActivate(EnergyColor actorColor, bool allowToggle)
        {
            if (actorColor != requiredColor)
            {
                return;
            }

            if (singleUse && isActive)
            {
                return;
            }

            if (toggleMode && allowToggle)
            {
                SetActiveState(!isActive);
                return;
            }

            SetActiveState(true);

            if (resetDelay > 0f)
            {
                StopAllCoroutines();
                StartCoroutine(ResetAfterDelay());
            }
        }

        private void RefreshVisuals()
        {
            float emission = isActive ? activeEmission : idleEmission;

            if (panelRenderers == null || panelRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer panelRenderer in panelRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(panelRenderer, requiredColor, emission);
            }
        }
    }
}
