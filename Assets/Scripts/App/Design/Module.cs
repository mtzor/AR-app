using MixedReality.Toolkit.SpatialManipulation;
using Unity.Netcode;
using UnityEngine;

public class Module : NetworkBehaviour
{
    public ModuleScriptable moduleData; // Information about the module, like its name or ID.
    private bool isPlaced = false;
    private Renderer moduleRenderer;

    [SerializeField] private Material fitMaterial;       // Material for the module when it fits
    [SerializeField] private Material nofitMaterial;     // Material for the module when it doesn't fit

    private BuildingArea lastValidArea;      // To store the last valid area entered
    private Vector3 initialPosition;         // To store the initial position in case no valid area is found
    private bool isInsideValidArea = false;  // Flag to determine if the module is in a valid area

    [SerializeField] private bool isRectangular = false; // Flag to determine if the module is rectangular
    private void Start()
    {
        moduleRenderer = GetComponent<Renderer>();
        initialPosition = transform.position;  // Store the initial position
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlaced) return;

        BuildingArea area = other.GetComponent<BuildingArea>();

        if (area != null && !area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area))
        {
            //Debug.Log("Entered fitting area Encapsulated: " + area.AreaID);
            area.OccupyServerRpc();

        }

        if (area != null && !area.IsOccupied && area.AreaID == this.moduleData.Name && IsRotationValid(area))
        {
            DesignManager.Instance.AddArea(moduleData.Area);
            SetLastValidArea(area);
            ChangeColor(true);
            //Debug.Log("Entered fitting area: " + area.AreaID);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPlaced) return;

        BuildingArea area = other.GetComponent<BuildingArea>();

       // Debug.Log("Trigger stay: " + area.AreaID+" Area occupied"+area.IsOccupied);
        if (area != null && !area.IsOccupied && area.AreaID == this.moduleData.Name && IsRotationValid(area))
        {
            if (area != lastValidArea)
            {
                SetLastValidArea(area);
            }
            ChangeColor(true);
            isInsideValidArea = true;
        }
        else if (area == lastValidArea)
        {
            // If the area was the last valid but is no longer valid, revert it
            RevertLastValidArea();
            isInsideValidArea = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BuildingArea area = other.GetComponent<BuildingArea>();

        if (area != null && area.IsOccupied && area.AreaID != this.moduleData.Name && IsRotationValid(area))
        {
            //Debug.Log("Vacated fitting area encaapsulated: " + area.AreaID);
            area.VacateServerRpc();

        }

        if (isPlaced) return;

        if (area != null && area == lastValidArea)
        {
            DesignManager.Instance.SubtractArea(moduleData.Area);
            // If exiting the last valid area, revert its material and flag
            RevertLastValidArea();
            ChangeColor(false); // Ensure the module's material changes to nofitMaterial
           // Debug.Log("Exited fitting area: " + area.AreaID);
            isInsideValidArea = false;
        }
    }

    public void OnManipulationEnded()
    {
        // When manipulation ends, only snap to the last valid area if still inside it and rotation is valid
        if (isInsideValidArea && lastValidArea != null)
        {
            if (IsRotationValid(lastValidArea))
            {
                SnapToArea(lastValidArea);
            }
            else
            {
                // If rotation is not valid, reset color and do not snap
                ChangeColor(false);
               // Debug.Log("Rotation not valid, module not snapped.");
            }
        }
        else
        {
            // If no valid area or exited, do not snap and revert to nofitMaterial
            ChangeColor(false);
        }
    }

    private void SetLastValidArea(BuildingArea area)
    {
        // Revert the material of the previous valid area if there was one
        if (lastValidArea != null && lastValidArea != area)
        {
            RevertLastValidArea();
        }

        // Set the new last valid area and highlight it
        lastValidArea = area;
        area.HighlightAreaMaterialClientRpc();
        isInsideValidArea = true;
    }

    private void RevertLastValidArea()
    {
        if (lastValidArea != null)
        {
            lastValidArea.RevertAreaMaterialClientRpc();
            lastValidArea = null;
            isInsideValidArea = false;
        }
    }

    private void SnapToArea(BuildingArea area)
    {
        Debug.Log("Snapping to area: " + area.AreaID);
        // Snap the module to the exact position and rotation of the BuildingArea
        transform.position = area.transform.position;
        transform.rotation = area.transform.rotation;

        // Mark the module as placed and the area as occupied
        isPlaced = true;
        area.OccupyServerRpc();

        // Revert the area to its original material after snapping
        area.RevertAreaMaterialClientRpc();

        // Make the module stationary
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ObjectManipulator>().enabled = false;
    }

    private void ChangeColor(bool fits)
    {
        // Change the material of the module based on whether it fits or not
        moduleRenderer.material = fits ? fitMaterial : nofitMaterial;
    }

    private bool IsRotationValid(BuildingArea area)
    {
        // Check if the Y rotation of the module matches the area's Y rotation within 90-degree increments
        float moduleYRotation = Mathf.Round(transform.eulerAngles.y / 90) * 90;
        float areaYRotation = Mathf.Round(area.transform.eulerAngles.y / 90) * 90;

        if (!isRectangular)
        {
            //Debug.Log("Not rectangular rotation" + Mathf.Abs(moduleYRotation - areaYRotation));
            return Mathf.Abs(moduleYRotation - areaYRotation) < 1f; // Allow small tolerance due to floating point errors
        }
        else { 
           // Debug.Log("Module Y Rotation: " + moduleYRotation + " Area Y Rotation: " + areaYRotation+"The result is "+ Mathf.Abs(moduleYRotation - areaYRotation));
            return (Mathf.Abs(moduleYRotation - areaYRotation) < 1f || (Mathf.Abs(moduleYRotation - areaYRotation)> 90f && Mathf.Abs(moduleYRotation - areaYRotation) <181f));
        } // Allow small tolerance due to floating point errors
    }


}
