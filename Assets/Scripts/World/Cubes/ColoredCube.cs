using GameProgramming.Core;
using UnityEngine;

namespace GameProgramming.World.Cubes
{
    [RequireComponent(typeof(Rigidbody))]
    public class ColoredCube : MonoBehaviour
    {
        [SerializeField] private EnergyColor cubeColor = EnergyColor.Yellow;
        [SerializeField] private Renderer[] cubeRenderers;
        [SerializeField, Range(0f, 5f)] private float emissionIntensity = 1.5f;

        private Rigidbody cachedBody;
        private Collider cachedCollider;

        public EnergyColor CubeColor => cubeColor;
        public bool IsCarried { get; private set; }
        public Rigidbody Rigidbody => cachedBody;
        public Collider MainCollider => cachedCollider;

        private void Awake()
        {
            cachedBody = GetComponent<Rigidbody>();
            cachedCollider = GetComponent<Collider>();
            ApplyVisuals();
        }

        public void SetCarried(bool carried)
        {
            if (cachedBody == null)
            {
                cachedBody = GetComponent<Rigidbody>();
            }

            IsCarried = carried;
            cachedBody.isKinematic = carried;
            cachedBody.useGravity = !carried;

            if (!carried)
            {
                cachedBody.linearVelocity = Vector3.zero;
                cachedBody.angularVelocity = Vector3.zero;
            }
        }

        private void OnValidate()
        {
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (cubeRenderers == null || cubeRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer cubeRenderer in cubeRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(cubeRenderer, cubeColor, emissionIntensity);
            }
        }
    }
}
