using UnityEngine;

public class GridCellGenerator : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Vector2Int gridSize;

    // New field to specify the desired leftmost cell position
    [SerializeField] private Transform targetLeftmostCellPosition;

    private void Start()
    {
        // ... (your existing checks for Grid and prefab)

        // Get the grid's cell size
        Vector3 cellSize = grid.cellSize;

        // Iterate through each cell in the grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                // Calculate the grid cell position
                Vector3Int cellPosition = new Vector3Int(x, 0, z);

                // Get the world position of the cell center
                Vector3 worldPosition = grid.CellToWorld(cellPosition) + cellSize / 2;

                // Instantiate the GridCell prefab
                GameObject newCell = Instantiate(gridCellPrefab, worldPosition, Quaternion.identity, transform);

                // Get the GridCell component and set its GridPosition
                GridCell gridCellComponent = newCell.GetComponent<GridCell>();
                if (gridCellComponent != null)
                {
                    gridCellComponent.GridPosition = cellPosition;
                }
                else
                {
                    Debug.LogError("GridCell prefab is missing the GridCell component!");
                }

                // Calculate the offset to move the cell down and to the left
                Vector3 offset = targetLeftmostCellPosition.position - grid.CellToWorld(new Vector3Int(0, 0, 0)) - cellSize / 2;

                // Apply the offset to the newCell's position
                newCell.transform.position += offset;
            }
        }
    }
}
