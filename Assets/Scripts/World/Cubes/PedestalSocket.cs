using GameProgramming.Core;
using GameProgramming.World.Mechanisms;
using UnityEngine;

namespace GameProgramming.World.Cubes
{
    public class PedestalSocket : MonoBehaviour
    {
        [SerializeField] private EnergyColor requiredColor = EnergyColor.Yellow;
        [SerializeField] private Renderer[] pedestalRenderers;
        [SerializeField] private MechanismReceiver[] targets;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private bool snapCubeOnActivate;
        [SerializeField] private float snapInset = 0.05f;
        [SerializeField, Range(0f, 5f)] private float idleEmission = 0.5f;
        [SerializeField, Range(0f, 5f)] private float activeEmission = 2f;

        private ColoredCube currentCube;

        private void Awake()
        {
            RefreshVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryAcceptCube(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAcceptCube(other);
        }

        private void OnTriggerExit(Collider other)
        {
            ColoredCube cube = other.GetComponentInParent<ColoredCube>();
            if (cube != null && cube == currentCube)
            {
                ReleaseCube();
            }
        }

        private void OnValidate()
        {
            RefreshVisuals();
        }

        private void TryAcceptCube(Collider other)
        {
            ColoredCube cube = other.GetComponentInParent<ColoredCube>();

            if (cube == null)
            {
                return;
            }

            if (cube.IsCarried || cube.CubeColor != requiredColor)
            {
                if (cube == currentCube)
                {
                    ReleaseCube();
                }

                return;
            }

            if (currentCube == cube)
            {
                return;
            }

            currentCube = cube;

            if (snapCubeOnActivate && snapPoint != null)
            {
                cube.transform.position = ResolveSnapPosition(cube);
                cube.transform.rotation = snapPoint.rotation;

                if (cube.Rigidbody != null)
                {
                    cube.Rigidbody.linearVelocity = Vector3.zero;
                    cube.Rigidbody.angularVelocity = Vector3.zero;
                }
            }

            SetTargets(true);
        }

        private void ReleaseCube()
        {
            currentCube = null;
            SetTargets(false);
        }

        private void SetTargets(bool powered)
        {
            if (targets != null)
            {
                foreach (MechanismReceiver target in targets)
                {
                    if (target != null)
                    {
                        target.SetPowered(powered);
                    }
                }
            }

            RefreshVisuals();
        }

        private Vector3 ResolveSnapPosition(ColoredCube cube)
        {
            if (cube == null || snapPoint == null)
            {
                return snapPoint != null ? snapPoint.position : transform.position;
            }

            Collider cubeCollider = cube.MainCollider;
            if (cubeCollider == null)
            {
                return snapPoint.position;
            }

            float cubeHalfHeight = cubeCollider.bounds.extents.y;
            Vector3 snapPosition = snapPoint.position;
            snapPosition.y -= Mathf.Min(cubeHalfHeight, snapInset);
            return snapPosition;
        }

        private void RefreshVisuals()
        {
            float emission = currentCube != null ? activeEmission : idleEmission;

            if (pedestalRenderers == null || pedestalRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer pedestalRenderer in pedestalRenderers)
            {
                EnergyColorPalette.ApplyToRenderer(pedestalRenderer, requiredColor, emission);
            }
        }
    }
}
