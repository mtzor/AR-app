
using MixedReality.Toolkit.UX;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    // Reference to the PressableButton component on this cell
    [SerializeField] private PressableButton pressableButton;
    [SerializeField] private Vector3Int  gridPosition;

    // Stores the grid position of this cell
    public Vector3Int GridPosition {
        get => gridPosition; set => gridPosition = value;
    }

    private void Awake()
    {
        // Ensure the PressableButton is assigned
        if (pressableButton == null)
        {
            Debug.LogError("PressableButton not assigned to GridCell!");
        }
    }
}
