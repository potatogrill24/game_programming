using UnityEngine;

namespace GameProgramming.World.Mechanisms
{
    public class PassageMechanism : MechanismReceiver
    {
        [SerializeField] private GameObject passageRoot;

        protected override void ApplyState(bool isActive)
        {
            if (passageRoot != null)
            {
                passageRoot.SetActive(isActive);
            }
        }
    }
}
