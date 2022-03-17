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

public class NetworkGamePlayer : NetworkBehaviour
{
    // SyncVars
    [SyncVar]
    private string displayName = "Cargando...";

    private MainNetworkManager room;
    private MainNetworkManager Room
    {
        get
        {
            if (room != null) return room;
            return room = NetworkManager.singleton as MainNetworkManager;
        }
    }

    private GameUI myUI = null;

    public CategoriesList cl;

    private ResumeUI ResumeCanvas;

    #region Personal Methods

    [Server]
    public void SetCategoryList() => this.cl = JsonUtility.FromJson<CategoriesList>(PlayerPrefs.GetString(UISelectMenu.prefsKey, "{}"));

    [Client]
    public CategoriesList GetCL() { return this.cl; }

    [Server]
    public void SetDisplayName(string displayName) => this.displayName = displayName;

    [Client]
    public string GetDisplayName() { return this.displayName; }

    public void FindCanvas()
    {
        if (isClient)
        {
            myUI = GameObject.Find("Canvas").GetComponent<GameUI>();
            myUI.SetPlayer(this);
        }
    }

    [ClientRpc]
    public void ToggleAllPlayerPanels(int panelIndex)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.ToggleMyPanel(panelIndex);
                    break;
                }
            }
            return;
        }

        ToggleMyPanel(panelIndex);
    }

    void ToggleMyPanel(int panel)
    {
        myUI.TogglePanels(panel);
    }

    [Command]
    void CmdRequestPersonalResults(string name)
    {
        (string[], CategoriesList[]) result = Room.PersonalMatches(name);

        CategoriesList[] scores = result.Item2;

        CategoriesList[] commons = new CategoriesList[scores.Length];
        for (int i = 0; i < scores.Length; i++)
            commons[i] = scores[i].CommonList(cl);

        SettingOwnMatches(commons, result.Item1);
    }

    [TargetRpc]
    void SettingOwnMatches(CategoriesList[] commons, string[] names)
    {
        AddMyMatches(commons, names);
    }

    void AddMyMatches(CategoriesList[] commons, string[] names)
    {
        if (!isLocalPlayer)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.isLocalPlayer)
                {
                    Debug.Log("Paso <color=cyan>4-1</color>: " + player.GetDisplayName() + " está en jugador buscando en los cliente");
                    player.AddMyMatches(commons, names);
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < commons.Length; i++)
        {
            ResumeCanvas.AddOwnMatch(names[i], 2 - i, commons[i]);
        }
    }

    [ClientRpc]
    public void RequestGeneralResults(string[] matches)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.PrintBestMatches(matches);
                    break;
                }
            }
            return;
        }

        PrintBestMatches(matches);
    }

    void PrintBestMatches(string[] matches)
    {
        AssignResumeCanvas();

        for (int i = 0; i < matches.Length; i++)
        {
            char[] separators = new char[] { GameManager.Separator };
            string[] split = matches[i].Split(separators, 3);
            string playerA, playerB;
            try
            {
                playerA = split[0];
                playerB = split[1];
            }
            catch (IndexOutOfRangeException)
            {
                Debug.Log("<color=red> FALLÓ EN ENCONTRAR NOMBRES</color>");
                playerA = "Error inesperado";
                playerB = "Error inesperado";
            }
            ResumeCanvas.ChangeGeneralCouple(playerA, playerB, 3 - i);
        }

        CmdRequestPersonalResults(GetDisplayName());
    }

    private void AssignResumeCanvas()
    {
        if (ResumeCanvas == null)
        {
            ResumeCanvas = Instantiate(Resources.Load<ResumeUI>("Prefabs/ResumeCanvas"));
            ResumeCanvas.SetManager(Room);
            if (isServer) ResumeCanvas.SetBtnHost();
        }
    }

    [ClientRpc]
    public void AddAllListPlayer(string playerName, bool isPrompt)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.AddListPlayer(playerName, isPrompt);
                    break;
                }
            }
            return;
        }

        AddListPlayer(playerName, isPrompt);
    }

    void AddListPlayer(string playerName, bool isPrompt)
    {
        myUI.AddPlayerToColumn(playerName, isPrompt);
    }
    
    [ClientRpc]
    public void ChangeColumnTitles(string leftTitle, string rightTitle)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.ChangeMyColumns(leftTitle, rightTitle);
                    break;
                }
            }
            return;
        }

        ChangeMyColumns(leftTitle, rightTitle);
    }
    void ChangeMyColumns(string leftTitle, string rightTitle)
    {
        myUI.ChangeColumnsNames(leftTitle, rightTitle);
    }
    public void SendAnswer(string input)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.SendAnswer(input);
                    break;
                }
            }
            return;
        }

        CmdSendTextInput(input);
    }
    public void SendReady()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    player.SendReady();
                    break;
                }
            }
            return;
        }

        CmdSendReady();
    }

    [Command]
    public void CmdSendTextInput(string input)
    {
        Room.SendTextInput(input, GetDisplayName());
    }

    [Command]
    public void CmdSendReady()
    {
        Room.SendMarkReady(GetDisplayName());
    }

    [ClientRpc]
    public void UpdateAllPrompts(string prompt)
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (!player.Equals(this) && player.hasAuthority)
                {
                    SetMyPrompt(prompt);
                    break;
                }
            }
            return;
        }

        SetMyPrompt(prompt);
    }

    void SetMyPrompt(string prompt)
    {
        if (myUI == null)
            FindCanvas();

        myUI.SetPrompt(prompt);
    }
    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    public override void OnStartServer()
    {
        SetCategoryList();
    }
    #endregion
}
