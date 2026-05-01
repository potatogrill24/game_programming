using UnityEngine;

namespace GameProgramming.CameraSystem
{
    public class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -7f);
        [SerializeField] private bool useTargetSpaceOffset;
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothSpeed = 8f;
        [SerializeField] private bool lookAtTarget = true;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredOffset = useTargetSpaceOffset ? target.TransformDirection(offset) : offset;
            Vector3 desiredPosition = target.position + desiredOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

            if (lookAtTarget)
            {
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
                }
            }
        }
    }
}
