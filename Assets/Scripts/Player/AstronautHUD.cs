using GameProgramming.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameProgramming.Player
{
    public class AstronautHUD : MonoBehaviour
    {
        [SerializeField] private AstronautEnergy targetEnergy;
        [SerializeField] private Image colorIcon;
        [SerializeField] private Image glowFrame;
        [SerializeField] private float glowMultiplier = 1.4f;

        private void Reset()
        {
            targetEnergy = FindFirstObjectByType<AstronautEnergy>();
        }

        private void OnEnable()
        {
            if (targetEnergy != null)
            {
                targetEnergy.ColorChanged += HandleColorChanged;
            }
        }

        private void Start()
        {
            SyncState();
        }

        private void OnDisable()
        {
            if (targetEnergy != null)
            {
                targetEnergy.ColorChanged -= HandleColorChanged;
            }
        }

        private void SyncState()
        {
            if (targetEnergy == null)
            {
                return;
            }

            HandleColorChanged(targetEnergy.CurrentColor);
        }

        private void HandleColorChanged(EnergyColor newColor)
        {
            Color color = EnergyColorPalette.ToColor(newColor);

            if (colorIcon != null)
            {
                colorIcon.color = color;
            }

            if (glowFrame != null)
            {
                glowFrame.color = color * glowMultiplier;
            }
        }
    }
}
