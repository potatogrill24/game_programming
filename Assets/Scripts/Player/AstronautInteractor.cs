using GameProgramming.Core;
using GameProgramming.World.Cubes;
using GameProgramming.World.Interaction;
using UnityEngine;

namespace GameProgramming.Player
{
    [RequireComponent(typeof(AstronautEnergy))]
    [RequireComponent(typeof(AstronautController))]
    public class AstronautInteractor : MonoBehaviour
    {
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private float pickupRadius = 2f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private float dropForwardOffset = 0.75f;
        [SerializeField] private Vector3 carriedCubeEulerOffset;

        private AstronautEnergy energy;
        private AstronautController controller;
        private ColoredCube carriedCube;
        private Rigidbody carriedBody;

        public EnergyColor CurrentColor => energy.CurrentColor;
        public AstronautController Controller => controller;
        public bool IsCarryingCube => carriedCube != null;
        public ColoredCube CarriedCube => carriedCube;

        private void Awake()
        {
            energy = GetComponent<AstronautEnergy>();
            controller = GetComponent<AstronautController>();
        }

        private void Update()
        {
            if (!GameInput.WasPressed(KeyCode.E))
            {
                return;
            }

            if (IsCarryingCube)
            {
                DropCube();
                return;
            }

            TryInteract();
        }

        private void TryInteract()
        {
            ColoredCube nearbyCube = FindNearbyCube();
            if (nearbyCube != null)
            {
                PickUpCube(nearbyCube);
                return;
            }

            Ray ray = BuildInteractionRay();

            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
            {
                return;
            }

            ColoredCube cube = hit.collider.GetComponentInParent<ColoredCube>();
            if (cube != null && !cube.IsCarried)
            {
                PickUpCube(cube);
                return;
            }

            InteractableBase interactable = hit.collider.GetComponentInParent<InteractableBase>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }

        private Ray BuildInteractionRay()
        {
            if (interactionOrigin != null)
            {
                return new Ray(interactionOrigin.position, interactionOrigin.forward);
            }

            if (Camera.main != null)
            {
                return new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            }

            return new Ray(transform.position + Vector3.up, transform.forward);
        }

        private void PickUpCube(ColoredCube cube)
        {
            if (cube == null)
            {
                return;
            }

            Transform anchor = carryAnchor != null ? carryAnchor : transform;
            carriedCube = cube;
            carriedBody = cube.GetComponent<Rigidbody>();

            cube.SetCarried(true);
            cube.transform.SetParent(anchor);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.Euler(carriedCubeEulerOffset);
        }

        private ColoredCube FindNearbyCube()
        {
            Vector3 center = transform.position + Vector3.up;
            Collider[] hits = Physics.OverlapSphere(center, pickupRadius, interactMask, QueryTriggerInteraction.Collide);

            ColoredCube bestCube = null;
            float bestScore = float.MaxValue;

            foreach (Collider hit in hits)
            {
                ColoredCube cube = hit.GetComponentInParent<ColoredCube>();
                if (cube == null || cube.IsCarried)
                {
                    continue;
                }

                Vector3 offset = cube.transform.position - transform.position;
                offset.y = 0f;

                float distanceScore = offset.sqrMagnitude;
                float facingPenalty = Vector3.Dot(transform.forward, offset.normalized) < -0.2f ? 4f : 0f;
                float score = distanceScore + facingPenalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestCube = cube;
                }
            }

            return bestCube;
        }

        private void DropCube()
        {
            if (carriedCube == null)
            {
                return;
            }

            Transform anchor = carryAnchor != null ? carryAnchor : transform;

            carriedCube.transform.SetParent(null);
            carriedCube.transform.position = anchor.position + transform.forward * dropForwardOffset;
            carriedCube.SetCarried(false);

            if (carriedBody != null)
            {
                carriedBody.linearVelocity = Vector3.zero;
                carriedBody.angularVelocity = Vector3.zero;
            }

            carriedCube = null;
            carriedBody = null;
        }

        public void ForceDropCube()
        {
            DropCube();
        }
    }
}
