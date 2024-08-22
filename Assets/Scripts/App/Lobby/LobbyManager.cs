using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {


    public static LobbyManager Instance { get; private set; }


    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_TYPE = "PlayerType";
    public const string KEY_SESSION_MODE = "SessionMode";
    public const string KEY_START_SESSION = "Start";



    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbySessionModeChanged;
    public event EventHandler<EventArgs> OnSessionStarted;

    public class LobbyEventArgs : EventArgs {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }


    public enum SessionMode {
        Design,
        Customize
    }

    public enum PlayerType {
        Expert,
        NonExpert
    }


    private System.Random random = new System.Random();
    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private Lobby joinedLobby;
    private int number = 5;//random.Next(1, 10000);
    private string playerName = "Player";
    private PlayerType playerType = PlayerType.NonExpert;
    public SessionMode sessionMode = SessionMode.Design;

    public SessionMode GetSessionMode()
    {
        return sessionMode; // Getter
    }

    public void SetSessionMode(string value)
    {
        Debug.Log("String value: " + value.ToString());

        if (value == SessionMode.Customize.ToString())
        {
            sessionMode = SessionMode.Customize;
        }
        else
        {
            sessionMode = SessionMode.Design;
        }
    }

    private async void Awake() {
        Instance = this;
        playerName="Player" + UnityEngine.Random.Range(1, 1000).ToString();

        LobbyManager.Instance.Authenticate(playerName);
    }

    private void Update() {
        HandleRefreshLobbyList(); // Disabled Auto Refresh for testing with multiple builds
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async void Authenticate(string playerName) {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {
            // do nothing
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            RefreshLobbyList(sessionMode);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void HandleRefreshLobbyList() {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn) {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f) {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList(sessionMode);
            }
        }
    }

    private async void HandleLobbyHeartbeat() {
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling() {
        if (joinedLobby != null) {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f) {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby()) {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }

                if (joinedLobby.Data[KEY_START_SESSION].Value != "0")
                {
                    //Start Session
                    if (!IsLobbyHost())//Lobby Host already joined Relay
                    {
                        TestRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_SESSION].Value);
                    }
                    
                    joinedLobby= null;

                    OnSessionStarted?.Invoke(this, EventArgs.Empty);

                }
            }
        }
    }

    public Lobby GetJoinedLobby() {
        return joinedLobby;
    }

    public bool IsLobbyHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby() {
        if (joinedLobby != null && joinedLobby.Players != null) {
            foreach (Player player in joinedLobby.Players) {
                if (player.Id == AuthenticationService.Instance.PlayerId) {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer() {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_TYPE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerType.ToString()) }
        });
    }

    public void ChangeSessionMode() {
        if (IsLobbyHost()) {
            SessionMode sessionMode =
                Enum.Parse<SessionMode>(joinedLobby.Data[KEY_SESSION_MODE].Value);

            switch (sessionMode) {
                default:
                case SessionMode.Design:
                    sessionMode = SessionMode.Customize;
                    break;
                case SessionMode.Customize:
                    sessionMode = SessionMode.Design;
                    break;
            }

            UpdateLobbySessionMode(sessionMode);
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, SessionMode sessionMode) {
        Player player = GetPlayer();
        Debug.Log("Lobby created with sessionm0de:" + sessionMode);
        CreateLobbyOptions options = new CreateLobbyOptions {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject> {
                { KEY_SESSION_MODE, new DataObject(DataObject.VisibilityOptions.Public, sessionMode.ToString(), DataObject.IndexOptions.S1) },
                { KEY_START_SESSION, new DataObject(DataObject.VisibilityOptions.Member, "0" ) }

            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        Debug.Log("Created Lobby " + lobby.Name);
    }

    public async void RefreshLobbyList(SessionMode? targetSessionMode = null) {

        //Debug.Log("Session mode on Refresh Lobby List"+targetSessionMode);
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            List<QueryFilter> filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
            };

            if (targetSessionMode != null)
            {
                //Debug.Log("target Session mode is: "+targetSessionMode);
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value:targetSessionMode.Value.ToString()
                ));
            }
            
        options.Filters = filters;

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode) {
        Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions {
            Player = player
        });

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void JoinLobby(Lobby lobby) {
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void UpdatePlayerName(string playerName) {
        this.playerName = playerName;

        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_NAME, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerName)
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerCharacter(PlayerType playerType) {
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_TYPE, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerType.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void QuickJoinLobby() {
        try {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby() {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId) {
        if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdateLobbySessionMode(SessionMode sessionMode) {
        try {
            Debug.Log("UpdateLobbySessionMode " + sessionMode);
            
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_SESSION_MODE, new DataObject(DataObject.VisibilityOptions.Public, sessionMode.ToString()) }
                }
            });

            joinedLobby = lobby;

            OnLobbySessionModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void StartSession()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Starting Session");

                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_SESSION, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }

                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

}