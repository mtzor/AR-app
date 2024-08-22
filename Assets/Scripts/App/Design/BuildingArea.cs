using UnityEngine;

public class BuildingArea : MonoBehaviour
{
    [SerializeField] private string areaID; // Unique identifier for the area
    [SerializeField] private bool isOccupied = false;

    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    private Renderer areaRenderer;

    public string AreaID { get => areaID; set => areaID = value; }  // Property for the unique ID

    public void Occupy()
    {
        IsOccupied = true;
    }

    public void Vacate()
    {
        IsOccupied = false;
    }

}
