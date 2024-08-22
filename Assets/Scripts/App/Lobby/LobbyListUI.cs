using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour {


    public static LobbyListUI Instance { get; private set; }



    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private PressableButton refreshButton;
    [SerializeField] private PressableButton createLobbyButton;

    private string lobbyName = "Lobby";
    private bool isPrivate=false;
    private int maxPlayers = 4;
    private LobbyManager.SessionMode sessionMode= LobbyManager.SessionMode.Customize;
    public LobbyManager.SessionMode GetSessionMode()
    {
        return sessionMode; // Getter
    }

    public void SetSessionMode(string value)
    {
        Debug.Log("String value: " + value.ToString());

        if (value == LobbyManager.SessionMode.Customize.ToString())
        {
            sessionMode = LobbyManager.SessionMode.Customize;
        }
        else
        {
            sessionMode = LobbyManager.SessionMode.Design;
        }
    }

    private void Awake() {
        Instance = this;
        lobbyName = "Lobby "+UnityEngine.Random.Range(1, 1000).ToString();

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.OnClicked.AddListener(RefreshButtonClick);
        createLobbyButton.OnClicked.AddListener(CreateLobbyButtonClick);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Debug.Log("Lobby name :" + lobby.Name);
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick() {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate,
                sessionMode
            );
            Hide();
           // LobbyCreateUI.Instance.Show();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}