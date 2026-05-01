using System;
using GameProgramming.Core;
using UnityEngine;

namespace GameProgramming.Player
{
    public class AstronautEnergy : MonoBehaviour
    {
        [SerializeField] private EnergyColor currentColor = EnergyColor.Yellow;
        [SerializeField] private Renderer[] suitRenderers;
        [SerializeField] private bool readKeyboardInput = true;
        [SerializeField, Range(0f, 5f)] private float emissionIntensity = 2f;

        public event Action<EnergyColor> ColorChanged;

        public EnergyColor CurrentColor => currentColor;

        private void Awake()
        {
            ApplyVisuals();
        }

        private void Start()
        {
            ColorChanged?.Invoke(currentColor);
        }

        private void Update()
        {
            if (!readKeyboardInput)
            {
                return;
            }

            if (GameInput.WasPressed(KeyCode.Alpha1))
            {
                SetColor(EnergyColor.Yellow);
            }
            else if (GameInput.WasPressed(KeyCode.Alpha2))
            {
                SetColor(EnergyColor.Blue);
            }
            else if (GameInput.WasPressed(KeyCode.Alpha3))
            {
                SetColor(EnergyColor.Purple);
            }
        }

        public void SetColor(EnergyColor newColor)
        {
            if (currentColor == newColor)
            {
                return;
            }

            currentColor = newColor;
            ApplyVisuals();
            ColorChanged?.Invoke(currentColor);
        }

        public void RefreshVisuals()
        {
            ApplyVisuals();
            ColorChanged?.Invoke(currentColor);
        }

        private void OnValidate()
        {
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (suitRenderers == null || suitRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer targetRenderer in suitRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(targetRenderer, currentColor, emissionIntensity);
            }
        }
    }
}
