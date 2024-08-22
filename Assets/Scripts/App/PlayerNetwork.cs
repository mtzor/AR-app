using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;

    private Transform spawnedObjectTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (!IsOwner) { return; }

        if (Input.GetKey(KeyCode.P))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }
        if(Input.GetKey(KeyCode.L))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }
        if (Input.GetKey(KeyCode.T))
        {
            moveDir.z= + 1f;
        }
        if (Input.GetKey(KeyCode.G))
        {
            moveDir.z = - 1f;
        }
        if (Input.GetKey(KeyCode.F))
        {
            moveDir.x = - 1f;
        }
        if (Input.GetKey(KeyCode.H))
        {
            moveDir.x = + 1f;
        }

        float moveSpeed = 3f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }
}
