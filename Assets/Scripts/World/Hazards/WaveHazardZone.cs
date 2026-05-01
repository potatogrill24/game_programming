using System.Collections;
using System.Collections.Generic;
using GameProgramming.Core;
using GameProgramming.Game;
using GameProgramming.Player;
using UnityEngine;

namespace GameProgramming.World.Hazards
{
    [RequireComponent(typeof(Collider))]
    public class WaveHazardZone : MonoBehaviour
    {
        [SerializeField] private float startupDelay = 1f;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private float waveTravelTime = 1.25f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private int successesToDisable = 3;
        [SerializeField] private int failuresToRespawn = 3;
        [SerializeField] private float radiusOverride;
        [SerializeField] private Transform waveVisual;
        [SerializeField] private Renderer[] waveRenderers;
        [SerializeField, Range(0f, 5f)] private float waveEmission = 2.5f;

        private readonly HashSet<AstronautEnergy> trackedPlayers = new HashSet<AstronautEnergy>();
        private Collider zoneCollider;
        private Coroutine loopRoutine;
        private float waveVisualHeight = 1f;
        private int successCount;
        private int failureCount;
        private bool zoneResolved;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;

            if (waveVisual != null)
            {
                waveVisualHeight = waveVisual.localScale.y;
            }

            SetWaveVisible(false);
        }

        private void OnEnable()
        {
            loopRoutine = StartCoroutine(WaveLoop());
        }

        private void OnDisable()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }

            SetWaveVisible(false);
            trackedPlayers.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                trackedPlayers.Add(astronaut);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            AstronautEnergy astronaut = other.GetComponentInParent<AstronautEnergy>();
            if (astronaut != null)
            {
                trackedPlayers.Remove(astronaut);
            }
        }

        private IEnumerator WaveLoop()
        {
            yield return new WaitForSeconds(startupDelay);

            while (true)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                yield return EmitWave((EnergyColor)Random.Range(0, 3));

                if (zoneResolved)
                {
                    yield break;
                }
            }
        }

        private IEnumerator EmitWave(EnergyColor waveColor)
        {
            float radius = GetEffectiveRadius();
            List<AstronautEnergy> playersSnapshot = new List<AstronautEnergy>(trackedPlayers);

            ApplyWaveColor(waveColor);
            ResetWaveScale();
            SetWaveVisible(true);

            foreach (AstronautEnergy astronaut in playersSnapshot)
            {
                if (astronaut != null)
                {
                    StartCoroutine(ResolveShock(astronaut, waveColor, radius));
                }
            }

            float elapsed = 0f;
            while (elapsed < waveTravelTime)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / waveTravelTime);
                float diameter = Mathf.Lerp(0.15f, radius * 2f, normalizedTime);

                if (waveVisual != null)
                {
                    waveVisual.localScale = new Vector3(diameter, waveVisualHeight, diameter);
                }

                yield return null;
            }

            SetWaveVisible(false);
        }

        private IEnumerator ResolveShock(AstronautEnergy astronaut, EnergyColor waveColor, float radius)
        {
            Vector3 center = transform.position;
            Vector3 astronautPosition = astronaut.transform.position;
            astronautPosition.y = center.y;

            float distance = Vector3.Distance(center, astronautPosition);
            float delay = radius > 0.01f ? Mathf.Clamp01(distance / radius) * waveTravelTime : 0f;

            yield return new WaitForSeconds(delay);

            if (astronaut == null || !trackedPlayers.Contains(astronaut))
            {
                yield break;
            }

            if (astronaut.CurrentColor == waveColor)
            {
                RegisterSuccess();
                yield break;
            }

            AstronautController controller = astronaut.GetComponent<AstronautController>();
            if (controller != null)
            {
                Vector3 direction = astronaut.transform.position - center;
                direction.y = 0f;

                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = astronaut.transform.forward;
                }

                controller.ApplyKnockback(direction, knockbackForce);
            }

            RegisterFailure(astronaut);
        }

        private float GetEffectiveRadius()
        {
            if (radiusOverride > 0f)
            {
                return radiusOverride;
            }

            Bounds bounds = zoneCollider.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.z);
        }

        private void ApplyWaveColor(EnergyColor waveColor)
        {
            if (waveRenderers == null || waveRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer waveRenderer in waveRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(waveRenderer, waveColor, waveEmission);
            }
        }

        private void SetWaveVisible(bool isVisible)
        {
            if (waveVisual != null)
            {
                waveVisual.gameObject.SetActive(isVisible);
            }

            if (waveRenderers == null || waveRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer waveRenderer in waveRenderers)
            {
                if (waveRenderer != null)
                {
                    waveRenderer.enabled = isVisible;
                }
            }
        }

        private void ResetWaveScale()
        {
            if (waveVisual != null)
            {
                waveVisual.localScale = new Vector3(0.15f, waveVisualHeight, 0.15f);
            }
        }

        private void RegisterSuccess()
        {
            if (zoneResolved)
            {
                return;
            }

            successCount++;
            if (successCount < successesToDisable)
            {
                return;
            }

            zoneResolved = true;

            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }

            SetWaveVisible(false);

            if (zoneCollider != null)
            {
                zoneCollider.enabled = false;
            }

            trackedPlayers.Clear();
        }

        private void RegisterFailure(AstronautEnergy astronaut)
        {
            if (zoneResolved)
            {
                return;
            }

            failureCount++;
            if (failureCount < failuresToRespawn)
            {
                return;
            }

            failureCount = 0;
            successCount = 0;

            PlayerRespawn playerRespawn = astronaut != null ? astronaut.GetComponent<PlayerRespawn>() : null;
            if (playerRespawn != null)
            {
                playerRespawn.Respawn();
            }
        }
    }
}
