using GameProgramming.Player;
using UnityEngine;

namespace GameProgramming.Game
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private Renderer[] checkpointRenderers;
        [SerializeField] private Color inactiveColor = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.6f);
        [SerializeField] private float inactiveEmission = 0.1f;
        [SerializeField] private float activeEmission = 2f;

        private bool isActive;

        private void Reset()
        {
            Collider checkpointCollider = GetComponent<Collider>();
            checkpointCollider.isTrigger = true;
        }

        private void Awake()
        {
            RefreshVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();
            if (playerRespawn == null)
            {
                return;
            }

            playerRespawn.SetCheckpoint(respawnPoint != null ? respawnPoint : transform);
            isActive = true;
            RefreshVisuals();

            CheckpointNotificationHUD notificationHud = FindFirstObjectByType<CheckpointNotificationHUD>();
            if (notificationHud != null)
            {
                notificationHud.Show();
            }
        }

        private void OnValidate()
        {
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            Color color = isActive ? activeColor : inactiveColor;
            float emission = isActive ? activeEmission : inactiveEmission;

            if (checkpointRenderers == null || checkpointRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer checkpointRenderer in checkpointRenderers)
            {
                if (checkpointRenderer == null)
                {
                    continue;
                }

                Material[] materials = Application.isPlaying ? checkpointRenderer.materials : checkpointRenderer.sharedMaterials;

                foreach (Material material in materials)
                {
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
                        material.SetColor("_EmissionColor", color * emission);
                    }
                }
            }
        }
    }
}
