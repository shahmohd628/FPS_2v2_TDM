using Unity.Services.Multiplayer;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    private ISession _session;

    // Action — no parameters (Changed event on ISession is Action, not Action<T>)
    public event System.Action OnSessionUpdated;
    public event System.Action OnSessionLeft;

    // All properties verified against ISession interface docs
    public string   JoinCode       => _session?.Code ?? "";
    public string   SessionName    => _session?.Name ?? "";
    public int      PlayerCount    => _session?.PlayerCount ?? 0; // was CurrentPlayers
    public int      MaxPlayers     => _session?.MaxPlayers ?? 0;
    public bool     IsHost         => _session?.IsHost ?? false;
    public ISession CurrentSession => _session;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── HOST: Create session ─────────────────────────────────────────────
    // WithRelayNetwork() handles: Relay allocation + NGO StartHost internally
    // DO NOT call NetworkManager.StartHost() yourself
    public async Task<bool> CreateSession(string sessionName, int maxPlayers = 4)
    {
        try
        {
            var options = new SessionOptions
            {
                Name       = sessionName,
                MaxPlayers = maxPlayers,
                IsPrivate  = false,
            }.WithRelayNetwork();

            _session = await MultiplayerService.Instance
                .CreateSessionAsync(options);

            // Changed is event Action — handler takes NO parameters
            _session.Changed += HandleSessionChanged;

            await SetMyUsername(PlayerProfile.Username);

            Debug.Log($"[SESSION] Created '{_session.Name}'" +
                      $" | Code: {_session.Code}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SESSION] Create failed: {e.Message}");
            return false;
        }
    }

    // ── CLIENT: Join session by code ─────────────────────────────────────
    // JoinSessionByCodeAsync handles: Relay join + NGO StartClient internally
    // DO NOT call NetworkManager.StartClient() yourself
    public async Task<bool> JoinSession(string joinCode)
    {
        try
        {
            _session = await MultiplayerService.Instance
                .JoinSessionByCodeAsync(joinCode.ToUpper().Trim());

            _session.Changed += HandleSessionChanged;

            await SetMyUsername(PlayerProfile.Username);

            Debug.Log($"[SESSION] Joined '{_session.Name}'");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SESSION] Join failed: {e.Message}");
            return false;
        }
    }

    // ── Set my username as a session player property ─────────────────────
    public async Task SetMyUsername(string username)
    {
        if (_session == null) return;
        try
        {
            _session.CurrentPlayer.SetProperty("username",
                new PlayerProperty(username,
                    VisibilityPropertyOptions.Member));

            await _session.SaveCurrentPlayerDataAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SESSION] SetUsername failed: {e.Message}");
        }
    }

    // ── Read username from any player ────────────────────────────────────
    // IReadOnlyPlayer is the correct type for items in ISession.Players
    public string GetUsername(IReadOnlyPlayer player)
    {
        if (player?.Properties == null) return "Unknown";
        return player.Properties.TryGetValue("username", out var prop)
            ? prop.Value : "Unknown";
    }

    // ── Session change handler — Action with NO parameters ───────────────
    private void HandleSessionChanged()
    {
        OnSessionUpdated?.Invoke();
    }

    // ── Leave / cleanup ───────────────────────────────────────────────────
    public async void LeaveSession()
    {
        if (_session == null) return;
        try
        {
            _session.Changed -= HandleSessionChanged;
            await _session.LeaveAsync();
            _session = null;
            OnSessionLeft?.Invoke();
            Debug.Log("[SESSION] Left session.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SESSION] Leave failed: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (_session != null)
            _session.Changed -= HandleSessionChanged;
    }
}