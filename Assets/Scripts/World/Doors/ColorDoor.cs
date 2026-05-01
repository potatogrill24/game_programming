using System.Collections.Generic;
using GameProgramming.Core;
using GameProgramming.Player;
using UnityEngine;

namespace GameProgramming.World.Doors
{
    public class ColorDoor : MonoBehaviour
    {
        [SerializeField] private EnergyColor requiredColor = EnergyColor.Yellow;
        [SerializeField] private Transform doorVisual;
        [SerializeField] private Renderer[] accentRenderers;
        [SerializeField] private float openHeight = 3f;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField, Range(0f, 5f)] private float accentEmission = 2f;

        private readonly HashSet<AstronautEnergy> playersInRange = new HashSet<AstronautEnergy>();
        private Vector3 closedLocalPosition;
        private Vector3 openLocalPosition;

        private void Awake()
        {
            if (doorVisual == null)
            {
                doorVisual = transform;
            }

            closedLocalPosition = doorVisual.localPosition;
            openLocalPosition = closedLocalPosition + Vector3.up * openHeight;
            UpdateAccentVisuals();
        }

        private void Update()
        {
            if (doorVisual == null)
            {
                return;
            }

            bool shouldOpen = false;

            foreach (AstronautEnergy astronaut in playersInRange)
            {
                if (astronaut != null && astronaut.CurrentColor == requiredColor)
                {
                    shouldOpen = true;
                    break;
                }
            }

            Vector3 targetPosition = shouldOpen ? openLocalPosition : closedLocalPosition;
            doorVisual.localPosition = Vector3.Lerp(doorVisual.localPosition, targetPosition, moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                playersInRange.Add(astronaut);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                playersInRange.Remove(astronaut);
            }
        }

        private void OnValidate()
        {
            UpdateAccentVisuals();
        }

        private void UpdateAccentVisuals()
        {
            if (accentRenderers == null || accentRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer accentRenderer in accentRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(accentRenderer, requiredColor, accentEmission);
            }
        }
    }
}
