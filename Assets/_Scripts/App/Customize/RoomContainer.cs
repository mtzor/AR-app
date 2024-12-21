using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomContainer : MonoBehaviour
{
    [SerializeField] private bool isDouble;
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int rotation;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

  public void onRoomContainerBtnPressed()
    {
        //ASK THE LAYOUT MANAGER TO CLOSE PREVIOUS OPEN MENUS
        //ASK LAYOUT MANAGER TO OPEN THIS MENU PASS THE IS DOUBLE PARAMETER AND THE POSITION PARAMETER

        CustomizeManager.Instance.CurrentLayoutManager.OpenMenu(isDouble, spawnPos,rotation);
    }
}
