using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class NetworkRoomPlayer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameBtn = null;
    [SerializeField] private GameObject RoomPlayersLayout = null;
    [SerializeField] private GameObject RoomPlayerPrefab = null;

    [Header("SyncVars")]
    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Cargando...";

    [SyncVar(hook = nameof(HandleReadyChanged))]
    public bool IsReady = false;

    private bool isFirstPlayer;
    public bool IsFirstPlayer
    {
        set
        {
            isFirstPlayer = value;
            startGameBtn.gameObject.SetActive(value);
            HandleReadyToStart(false);
        }
    }

    private MainNetworkManager room;
    private MainNetworkManager Room
    {
        get
        {
            if (room != null) return room;
            return room = NetworkManager.singleton as MainNetworkManager;
        }
    }

    #region Personal Methods

    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();
    public void HandleReadyChanged(bool oldValue, bool newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < RoomPlayersLayout.transform.childCount; i++)
        {
            Destroy(RoomPlayersLayout.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            GameObject actual = Instantiate(RoomPlayerPrefab, RoomPlayersLayout.transform);
            actual.transform.GetChild(0).GetComponent<Text>().text = Room.RoomPlayers[i].DisplayName;
            actual.transform.GetChild(1).GetComponent<Text>().text = Room.RoomPlayers[i].IsReady ? "Listo" : "No listo";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isFirstPlayer) return;

        startGameBtn.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;
        Room.SendReadyState();
    }

    [Command]
    public void CmdUnreadyUp()
    {
        IsReady = false;
        Room.SendReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;

        Room.ServerStartGame();
    }

    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() {
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() {
        Room.RoomPlayers.Remove(this);

        UpdateDisplay();
    }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() {
        CmdSetDisplayName(UIPlayMenu.DisplayName);

        lobbyUI.SetActive(true);
    }

    #endregion
}
