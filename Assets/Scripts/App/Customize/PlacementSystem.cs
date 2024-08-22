using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] Grid grid;
    [SerializeField] ObjectsDatabaseScriptable objectsDatabase;
    [SerializeField] private int selectedObjectIndex = -1;

    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private AudioSource source;

    [SerializeField] private GridData floorData, furnitureData;

    [SerializeField] private Renderer previewRenderer;

    private List<GameObject> placedGameObjects = new();

    void Start()
    {
        StopPlacement();
        floorData = new();
        furnitureData = new();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();

        // Get all PressableButtons that are children of the grid
        PressableButton[] allButtons = grid.GetComponentsInChildren<PressableButton>();

        // Subscribe to the OnClicked event of each button
        foreach (PressableButton button in allButtons)
        {
            button.OnClicked.AddListener(() => OnGridCellClicked(button));
        }
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = objectsDatabase.objectsData.FindIndex(data => data.ID == ID);

        Debug.Log("Started Placement");

        if (selectedObjectIndex < 0)
        {
            Debug.LogError("No ID found" + ID);
            return;
        }
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);
    }

    private void OnGridCellClicked(PressableButton button)
    {
        // Assuming each button has a reference to its grid position
        Vector3Int gridPosition = button.GetComponent<GridCell>().GridPosition; // Adjust if your setup is different

        if (selectedObjectIndex >= 0 && CheckPlacementValidity(gridPosition, selectedObjectIndex))
        {
            PlaceStructure(gridPosition);
        }
    }

    private void PlaceStructure(Vector3Int gridPosition)
    {
        Debug.Log("Placing structure...");

        if (CheckPlacementValidity(gridPosition, selectedObjectIndex))
        {
            Debug.Log("Placement Valid");
            source.Play();

            GameObject newObject = Instantiate(objectsDatabase.objectsData[selectedObjectIndex].Prefab);
            newObject.transform.position = grid.CellToWorld(gridPosition);

            placedGameObjects.Add(newObject);

            GridData selectedData = objectsDatabase.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
            selectedData.AddObjectAt(gridPosition, objectsDatabase.objectsData[selectedObjectIndex].Size, objectsDatabase.objectsData[selectedObjectIndex].ID, placedGameObjects.Count - 1);
        }
    }

    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = objectsDatabase.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

        return selectedData.CanPlaceObjectAt(gridPosition, objectsDatabase.objectsData[selectedObjectIndex].Size);
    }

    void Update()
    {
        if (selectedObjectIndex < 0) return;

        Vector3 mousePosition = inputManager.GetSelectedPointerPosition(); // You might need to adjust how you get the pointer position
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }
}
