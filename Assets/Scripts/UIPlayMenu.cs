using UnityEngine;
using UnityEngine.UI;

public class UIPlayMenu : MonoBehaviour
{
    [SerializeField] private MainNetworkManager networkManager = null;

    [Header("Buttons")]
    [SerializeField] private Button hostBtn = null;
    [SerializeField] private Button joinBtn = null;

    [Header("Text Inputs")]
    [SerializeField] private InputField nameInput = null;
    [SerializeField] private InputField ipAddressInput = null;
    public static string DisplayName { get; private set; } 
    private readonly string prefsNameKey = "PlayerName";

    private void OnEnable()
    {
        MainNetworkManager.OnClientConnected += HandleClientConnected;
        MainNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        MainNetworkManager.OnClientConnected -= HandleClientConnected;
        MainNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }
    private void Awake()
    {
        string savedName = PlayerPrefs.GetString(prefsNameKey, "");
        nameInput.text = savedName;
        EvaluateName(savedName);
    }

    public void EvaluateName(string name)
    {
        hostBtn.interactable = !string.IsNullOrEmpty(name);
        joinBtn.interactable = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ipAddressInput.text);
    }

    public void EvaluateIP(string name)
    {
        joinBtn.interactable = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(nameInput.text);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInput.text;
        PlayerPrefs.SetString(prefsNameKey, DisplayName);
    }

    public void HostLobby()
    {
        networkManager.StartHost();
    }

    public void JoinLobby()
    {
        networkManager.networkAddress = ipAddressInput.text;
        networkManager.StartClient();

        joinBtn.interactable = false;
        hostBtn.interactable = false;
        joinBtn.GetComponentInChildren<Text>().text = "Uniendose";
    }

    private void HandleClientConnected()
    {
        joinBtn.interactable = true;
        hostBtn.interactable = true;
        this.gameObject.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinBtn.interactable = true;
        hostBtn.interactable = true;
        joinBtn.GetComponentInChildren<Text>().text = "Unirse:";
    }
}
