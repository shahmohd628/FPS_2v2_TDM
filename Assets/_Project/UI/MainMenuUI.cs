using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Threading.Tasks;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Screen")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Play Panel")]
    [SerializeField] private GameObject playPanel;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button hostGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button backButton;

    [Header("Join Panel")]
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button backFromJoinButton;
    [SerializeField] private TMP_Text joinStatusText;


    private bool _updatingJoinCode = false;

    void Start()
    {
        if (playButton == null) { Debug.LogError("[MainMenuUI] playButton NULL"); return; }
        if (playPanel == null) { Debug.LogError("[MainMenuUI] playPanel NULL"); return; }
        if (joinPanel == null) { Debug.LogError("[MainMenuUI] joinPanel NULL"); return; }
        if (joinCodeInput == null) { Debug.LogError("[MainMenuUI] joinCodeInput NULL"); return; }

        // Username input — safe, no recursion possible
        usernameInput.text = PlayerProfile.Username;
        usernameInput.onValueChanged.AddListener(val =>
            PlayerProfile.Username = val);

        // Join code uppercase — guarded against recursive callback
        joinCodeInput.onValueChanged.AddListener(val =>
        {
            if (_updatingJoinCode) return;
            _updatingJoinCode = true;
            joinCodeInput.text = val.ToUpper();
            _updatingJoinCode = false;
        });

        // Wire buttons
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        hostGameButton.onClick.AddListener(OnHostGameClicked);
        joinGameButton.onClick.AddListener(OnJoinGameClicked);
        backButton.onClick.AddListener(OnBackClicked);
        confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
        backFromJoinButton.onClick.AddListener(OnBackFromJoinClicked);

        playPanel.SetActive(false);
        joinPanel.SetActive(false);

        SetStatus("Ready.");
        Debug.Log("[MainMenuUI] Start() completed.");
    }

    // ── Panel navigation ──────────────────────────────────────────────────
    void OnPlayClicked()
    {
        playPanel.SetActive(true);
        // Clear EventSystem selection to prevent BackButton auto-firing
        UnityEngine.EventSystems.EventSystem.current
            .SetSelectedGameObject(null);
}

    void OnBackClicked()
    {
        playPanel.SetActive(false);
        joinPanel.SetActive(false);
    }

    void OnJoinGameClicked()
    {
        playPanel.SetActive(false);
        joinPanel.SetActive(true);
        joinStatusText.text = "";
    }

    void OnBackFromJoinClicked()
    {
        joinPanel.SetActive(false);
        playPanel.SetActive(true);
    }

    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── HOST: Create session then load WaitingRoom ────────────────────────
    async void OnHostGameClicked()
    {
        hostGameButton.interactable = false;
        SetStatus("Signing in...");

        // Wait up to 5 seconds for UGS sign-in to complete
        await UGSManager.Instance.WaitForSignIn(5f);

        if (!UGSManager.Instance.IsSignedIn)
        {
            SetStatus("Sign-in failed. Check internet connection.");
            hostGameButton.interactable = true;
            return;
        }

        SetStatus("Creating session...");

        string name = string.IsNullOrWhiteSpace(lobbyNameInput?.text)
            ? $"{PlayerProfile.Username}'s Game"
            : lobbyNameInput.text.Trim();

        // CreateSession handles Relay + NGO StartHost via WithRelayNetwork()
        // DO NOT call NetworkManager.Singleton.StartHost() here
        bool created = await SessionManager.Instance.CreateSession(name, 4);

        if (!created)
        {
            SetStatus("Failed to create session. Try again.");
            hostGameButton.interactable = true;
            return;
        }

        SetStatus($"Session created! Code: {SessionManager.Instance.JoinCode}");

        // Host loads WaitingRoom via NGO SceneManager
        // This automatically syncs the scene load to all connected clients
        NetworkManager.Singleton.SceneManager.LoadScene(
            "WaitingRoom",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    // ── CLIENT: Join session and wait for host to load WaitingRoom ────────
    async void OnConfirmJoinClicked()
    {
        string code = joinCodeInput.text.ToUpper().Trim();
        if (string.IsNullOrEmpty(code))
        {
            joinStatusText.text = "Enter a join code first.";
            return;
        }

        confirmJoinButton.interactable = false;
        joinStatusText.text = "Signing in...";

        await UGSManager.Instance.WaitForSignIn(5f);

        if (!UGSManager.Instance.IsSignedIn)
        {
            joinStatusText.text = "Sign-in failed. Check connection.";
            confirmJoinButton.interactable = true;
            return;
        }

        joinStatusText.text = "Joining session...";

        // JoinSession handles Relay join + NGO StartClient via SDK internally
        // DO NOT call NetworkManager.Singleton.StartClient() here
        bool joined = await SessionManager.Instance.JoinSession(code);

        if (!joined)
        {
            joinStatusText.text = "Session not found. Check the code.";
            confirmJoinButton.interactable = true;
            return;
        }

        // Client does NOT call LoadScene here.
        // When host calls NetworkManager.SceneManager.LoadScene("WaitingRoom"),
        // NGO automatically syncs ALL connected clients into that scene.
        joinStatusText.text = "Joined! Waiting for host...";
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[UI] {msg}");
    }
}