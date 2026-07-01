using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System.Collections.Generic;

public class WaitingRoomUI : MonoBehaviour
{
    [Header("Lobby Info")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyCodeText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text waitingText;

    [Header("Player Slots — drag all 4 here")]
    [SerializeField] private PlayerSlotUI[] playerSlots;

    [Header("Buttons")]
    [SerializeField] private Button startMatchButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button copyCodeButton;

    [Header("Settings")]
    [SerializeField] private int minPlayersToStart = 2;

    void Start()
    {
        // OnSessionUpdated is Action (no parameters)
        // Wrap RefreshUI call in a lambda that gets session from SessionManager
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnSessionUpdated +=
                () => RefreshUI(SessionManager.Instance.CurrentSession);

        // Wire buttons
        leaveButton.onClick.AddListener(OnLeaveClicked);
        startMatchButton.onClick.AddListener(OnStartMatchClicked);

        if (copyCodeButton != null)
            copyCodeButton.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = SessionManager.Instance.JoinCode;
                Debug.Log("[COPY] Code copied: " +
                          SessionManager.Instance.JoinCode);
            });

        // Only host sees Start Match button
        bool isHost = SessionManager.Instance?.IsHost ?? false;
        startMatchButton.gameObject.SetActive(isHost);
        startMatchButton.interactable = false;

        // Show current session data immediately if already available
        var session = SessionManager.Instance?.CurrentSession;
        if (session != null)
            RefreshUI(session);
        else
            lobbyCodeText.text = $"Code: {SessionManager.Instance?.JoinCode}";
    }

    void OnDestroy()
    {
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnSessionUpdated -=
                () => RefreshUI(SessionManager.Instance.CurrentSession);
    }

    void RefreshUI(ISession session)
    {
        if (session == null) return;

        // Header info
        lobbyNameText.text = session.Name;
        lobbyCodeText.text = $"Code: {session.Code}";

        // PlayerCount is the correct ISession property (not CurrentPlayers)
        playerCountText.text =
            $"Players: {session.PlayerCount} / {session.MaxPlayers}";

        // Players is IReadOnlyList<IReadOnlyPlayer>
        var players = new List<IReadOnlyPlayer>(session.Players);

        // Update each player slot
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < players.Count)
            {
                var player = players[i];
                string username = SessionManager.Instance.GetUsername(player);
                // session.Host is a string (player ID of the host)
                bool isHost = player.Id == session.Host;
                playerSlots[i].SetPlayer(username, isHost);
            }
            else
            {
                playerSlots[i].SetEmpty();
            }
        }

        // Start button state — only for host, only when enough players
        bool amHost = SessionManager.Instance?.IsHost ?? false;
        bool canStart = amHost && session.PlayerCount >= minPlayersToStart;

        startMatchButton.gameObject.SetActive(amHost);
        startMatchButton.interactable = canStart;

        waitingText.text = canStart
            ? "All players ready — click START MATCH!"
            : $"Waiting for players..." +
              $" ({session.PlayerCount}/{minPlayersToStart} minimum)";
    }

    void OnStartMatchClicked()
    {
        startMatchButton.interactable = false;
        waitingText.text = "Starting match...";

        // Load Game scene via NGO SceneManager
        // Automatically syncs ALL clients into Game scene
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    void OnLeaveClicked()
    {
        SessionManager.Instance?.LeaveSession();
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}