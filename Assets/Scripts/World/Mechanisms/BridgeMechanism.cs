using UnityEngine;

namespace GameProgramming.World.Mechanisms
{
    public class BridgeMechanism : MechanismReceiver
    {
        [SerializeField] private GameObject bridgeRoot;

        protected override void ApplyState(bool isActive)
        {
            if (bridgeRoot != null)
            {
                bridgeRoot.SetActive(isActive);
            }
        }
    }
}
