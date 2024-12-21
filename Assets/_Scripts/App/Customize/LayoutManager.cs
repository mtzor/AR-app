using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LayoutManager : NetworkBehaviour
{
    [SerializeField] Transform viewPos;
    public string name;
    [SerializeField] private Transform layout;
    [SerializeField] private Transform roomLayout;

    [SerializeField] bool isShared;

    [SerializeField] GameObject roomMenuS;
    [SerializeField] GameObject roomMenuL;

    [SerializeField] Transform[] roomPrefabs;
    [SerializeField] Transform[] furniturePrefabs;
    // Start is called before the first frame update

    private GameObject openMenu;
    public Transform spawnPos;//debug ???
    public int spawnRotY;
    private bool isSpawning = false;

    private List<GameObject> _spawnedRooms;
    private List<GameObject> _spawnedFurniture;
    void Start()
    {
        _spawnedFurniture = new List<GameObject>();
        _spawnedRooms = new List<GameObject>();

    }

    public Transform Layout { get; set; }
    public Transform RoomLayout { get; set; }
    public void SetRoomLayout(Transform layout)
    {
        roomLayout= layout;
    }

    // Update is called once per frame
    void Update()
    {
     
    }
    public void DisplayLayoutModel()
    {
        if (roomLayout == null)
        {
            Debug.LogError("RoomLayout is not assigned!");
            return;
        }

        // Instantiate the layout
        Transform instantiatedLayout = Instantiate(roomLayout);

        // Preserve the world position
        instantiatedLayout.position = roomLayout.position;

        // Preserve the world rotation
        instantiatedLayout.rotation = roomLayout.rotation;

        // Scale the instantiated layout to double its size
        instantiatedLayout.localScale = layout.localScale * 2;

        // Optionally, parent it to roomLayout if you want it in the hierarchy
        instantiatedLayout.SetParent(viewPos, true); // true ensures world position/rotation stays intact
    }
    public void OpenMenu(bool isDouble, Transform pos,int rotation)
    {
        
        //closing already opened menu
        if (openMenu != null)
        {
            openMenu.SetActive(false);
            isSpawning = false;
        }

        spawnPos = pos;//setting spawn position
        spawnRotY = rotation;

        isSpawning = true;

        GameObject roomMenu = (isDouble) ? roomMenuL : roomMenuS;//setting the correct room menu

        Transform parent = roomMenu.transform.parent;

        Debug.Log("cHANGING ROOM MENU POS");
        Vector3 offset = new Vector3(0.245f, 0.1235f, -0.254f);
        roomMenu.transform.localPosition = parent.InverseTransformPoint(pos.position+offset);

        roomMenu.SetActive(true);

        openMenu = roomMenu;

    }

    public void SpawnRoom(int id)
    {
        if (id >= roomPrefabs.Count() || id < 0)
        {
            Debug.Log("Invalid room ID to spawn");
            return;
        }

        if (!isShared)
        {
            Debug.Log("Instantiating");
            // Instantiate the prefab without any parent to avoid inheriting scale/rotation
            Transform spawnedRoom = Instantiate(roomPrefabs[id]);

            // Move the instantiated object to the position of spawnPos
            spawnedRoom.position = spawnPos.position;

            // Ensure the rotation and scale of the prefab remain unchanged
            spawnedRoom.rotation = Quaternion.Euler(roomPrefabs[id].transform.rotation.x, spawnRotY, roomPrefabs[id].transform.rotation.z);
            spawnedRoom.localScale = roomPrefabs[id].localScale;//??

            // Add the spawned room to the list
            _spawnedRooms.Add(spawnedRoom.gameObject);
        }
        else
        {
            SpawnRoomServerRPC(id);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnRoomServerRPC(int id)
    {
        // Instantiate the room and set its parent to `spawnPos`
        Transform spawnedRoom = Instantiate(roomPrefabs[id], spawnPos);

        // Ensure the spawned room has a NetworkObject and spawn it across the network
        NetworkObject networkObject = spawnedRoom.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        // Pass the NetworkObjectId to clients
        AddSpawnedRoomClientRPC(networkObject.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void AddSpawnedRoomClientRPC(ulong networkObjectId)
    {
        // Resolve the NetworkObject from its ID
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObject))
        {
            _spawnedRooms.Add(netObject.gameObject); // Add the GameObject to your local list
        }
        else
        {
            Debug.LogError($"Failed to find NetworkObject with ID: {networkObjectId}");
        }
    }
}

