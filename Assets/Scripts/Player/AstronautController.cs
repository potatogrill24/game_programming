using UnityEngine;
using GameProgramming.Core;

namespace GameProgramming.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class AstronautController : MonoBehaviour
    {
        [SerializeField] private Transform cameraReference;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float knockbackDamping = 4f;
        [SerializeField] private float shockLift = 2f;

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private Vector3 externalVelocity;

        public Vector3 Velocity => characterController != null ? characterController.velocity : Vector3.zero;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleMovement();
        }

        public void ApplyKnockback(Vector3 direction, float force)
        {
            Vector3 pushDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : -transform.forward;
            externalVelocity += pushDirection * force;
            externalVelocity.y = shockLift;
        }

        private void HandleMovement()
        {
            Vector2 moveInput = GameInput.GetMovement();
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Vector3 moveDirection = ResolveMoveDirection(input);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }

            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);

            Vector3 motion = (moveDirection * moveSpeed) + externalVelocity + verticalVelocity;
            characterController.Move(motion * Time.deltaTime);
        }

        private Vector3 ResolveMoveDirection(Vector3 input)
        {
            if (input.sqrMagnitude < 0.001f)
            {
                return Vector3.zero;
            }

            if (cameraReference == null)
            {
                return transform.TransformDirection(input);
            }

            Vector3 forward = cameraReference.forward;
            Vector3 right = cameraReference.right;
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }
    }
}
