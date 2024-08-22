using UnityEngine;
using UnityEngine.Events;


public class InputManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    [SerializeField] private LayerMask placementLayerMask;

    private Vector3 lastPosition;

    private void Update()
    {
        // Update lastPosition only if hitting placementLayer
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && (((1 << hit.collider.gameObject.layer) & placementLayerMask) != 0))
        {
            lastPosition = hit.point;
        }
    }

    public Vector3 GetSelectedPointerPosition()
    {
        return lastPosition;
    }
}