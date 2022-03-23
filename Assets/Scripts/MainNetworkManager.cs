using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;
using System.Collections.Generic;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class MainNetworkManager : NetworkManager
{
    public int minPlayers { get; } = 4; //Minimo de 4 jugadores

    [Scene] [SerializeField] private string menuScene = "";

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;
    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    [SerializeField] private GameManager gameManager;

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    float explainTime = 2f;
    float explainEndTime = 0f;
    bool explaining = false;

    bool showingAnswers = false;

    #region Personal Methods
    private void Update()
    {
        if (explaining && SceneManager.GetActiveScene().name.StartsWith("Game") && Time.time >= explainEndTime)
        {
            //foreach (var player in GamePlayers)
            //{
            //    player.ToggleAllPlayerPanels(1);
            //}
            GamePlayers[0].ToggleAllPlayerPanels(1);

            explaining = false;

        }
    }

    public void SendTextInput(string input, string playerName) => gameManager.SaveTextInput(input, playerName);
    public void SendMarkReady(string playerName) => gameManager.MarkReady(playerName);
    public void AddPlayerResults(string playerName, bool isPrompt)
    {
        GamePlayers[0].AddAllListPlayer(playerName, isPrompt);
    }
    public void ChangeColumnTitles(string leftTitle, string rightTitle)
    {
        GamePlayers[0].ChangeColumnTitles(leftTitle, rightTitle);
    }


    public bool ShowingAnswers => showingAnswers;
    public void PanelForInput()
    {
        showingAnswers = false;
        int panel = gameManager.IsQuestioning() ? 1 : 2;
        GamePlayers[0].ToggleAllPlayerPanels(panel);
        //foreach (var player in GamePlayers)
        //{
        //    player.ToggleAllPlayerPanels(panel);//SERRA POSIBLE HACERLO SOLO DESDE EL PLAYER 0 ????
        //}
    }
    public void PanelAnswers()
    {
        showingAnswers = true;
        GamePlayers[0].ToggleAllPlayerPanels(3);
    }
    public void PanelFinal()
    {
        showingAnswers = false;
        GamePlayers[0].ToggleAllPlayerPanels(4);
    }
    public void FeedGeneralMatches(string[] matches)
    {
        GamePlayers[0].RequestGeneralResults(matches);
    }
    internal (string[], CategoriesList[]) PersonalMatches(string playerName)
    {
        int[] indexes = gameManager.PersonalBestMatches(playerName);

        string[] names = new string[indexes.Length];
        CategoriesList[] lists = new CategoriesList[indexes.Length];

        for (int i = 0; i < indexes.Length; i++)
        {
            names[i] = GamePlayers[indexes[i]].GetDisplayName();
            lists[i] = GamePlayers[indexes[i]].cl;
        }

        return (names, lists);
    }

    public void UpdatePrompt(string prompt)
    {
        GamePlayers[0].UpdateAllPrompts(prompt);
        //foreach(var player in GamePlayers)
        //{
        //    player.UpdateAllPrompts(prompt);
        //}
    }
    // ROOM MANAGEMENT
    public void SendReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) return false;

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) return false;
        }

        return true;
    }

    [Server]
    public void ServerStartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) return;

            ServerChangeScene("Game00");
        }
    }

    #endregion

    #region Scene Management

    [Server]
    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        // De menu al juego:
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("Game"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                NetworkConnection conn = RoomPlayers[i].connectionToClient;
                NetworkGamePlayer gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);

                explainEndTime = Time.time + explainTime;
                explaining = true;
            }
        }
        
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName)
    {
        if (!gameManager.initiated)
        {
            gameManager.InitGameManager(this);
            int game = Int32.Parse(sceneName.Substring(sceneName.Length - 2));
            gameManager.SelectGame(game);
        }
    }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    /// <param name="conn">The network connection that the scene change message arrived on.</param>
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        if (SceneManager.GetActiveScene().name.StartsWith("Game"))
        {
            conn.identity.gameObject.GetComponent<NetworkGamePlayer>().FindCanvas();
        }
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnection conn) {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
                //string sceneSubstring = menuScene.Substring(menuScene.LastIndexOf('/') + 1, menuScene.LastIndexOf('.') - menuScene.LastIndexOf('/') - 1);

        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            bool isFirstPlayer = RoomPlayers.Count == 0;

            NetworkRoomPlayer roomPlayer = Instantiate(roomPlayerPrefab);
            roomPlayer.IsFirstPlayer = isFirstPlayer;

            GameObject roomPlayerGO = roomPlayer.gameObject;
            roomPlayerGO.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

            NetworkServer.AddPlayerForConnection(conn, roomPlayerGO);
        }
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayer>();
            RoomPlayers.Remove(player);

            SendReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
        if (SceneManager.GetActiveScene().name.StartsWith("Game"))
        {
            SceneManager.LoadScene("MainMenu");
            Destroy(this.gameObject);
        }
    }

    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() {
        RoomPlayers.Clear();
    }

    #endregion
}
