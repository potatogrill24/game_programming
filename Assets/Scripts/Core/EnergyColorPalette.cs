using UnityEngine;

namespace GameProgramming.Core
{
    public static class EnergyColorPalette
    {
        public static Color ToColor(EnergyColor energyColor)
        {
            switch (energyColor)
            {
                case EnergyColor.Yellow:
                    return new Color(1f, 0.85f, 0.2f);
                case EnergyColor.Blue:
                    return new Color(0.2f, 0.65f, 1f);
                case EnergyColor.Purple:
                    return new Color(0.72f, 0.35f, 1f);
                default:
                    return Color.white;
            }
        }

        public static void ApplyToRenderer(Renderer targetRenderer, EnergyColor energyColor, float emissionIntensity)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Material[] materials = Application.isPlaying ? targetRenderer.materials : targetRenderer.sharedMaterials;

            foreach (Material material in materials)
            {
                ApplyToMaterial(material, energyColor, emissionIntensity);
            }
        }

        public static void ApplyToMaterial(Material material, EnergyColor energyColor, float emissionIntensity)
        {
            if (material == null)
            {
                return;
            }

            Color color = ToColor(energyColor);

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emissionIntensity);
            }
        }
    }
}
