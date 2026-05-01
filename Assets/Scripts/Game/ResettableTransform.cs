using GameProgramming.World.Cubes;
using UnityEngine;

namespace GameProgramming.Game
{
    public class ResettableTransform : MonoBehaviour
    {
        [SerializeField] private bool captureParentOnAwake = true;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Transform initialParent;
        private Rigidbody attachedBody;
        private ColoredCube coloredCube;

        private void Awake()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialParent = captureParentOnAwake ? transform.parent : null;
            attachedBody = GetComponent<Rigidbody>();
            coloredCube = GetComponent<ColoredCube>();
        }

        public void ResetState()
        {
            if (coloredCube != null && coloredCube.IsCarried)
            {
                coloredCube.SetCarried(false);
            }

            transform.SetParent(initialParent);
            transform.SetPositionAndRotation(initialPosition, initialRotation);

            if (attachedBody != null)
            {
                attachedBody.linearVelocity = Vector3.zero;
                attachedBody.angularVelocity = Vector3.zero;
            }
        }
    }
}
