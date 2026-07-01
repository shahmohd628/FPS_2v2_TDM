using Unity.Netcode;
using UnityEngine;
using System.Collections;

public enum MatchState { Lobby, Countdown, Playing, MatchEnd }

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [SerializeField] private float countdownDuration = 3f;

    private NetworkVariable<MatchState> _matchState =
        new NetworkVariable<MatchState>(MatchState.Lobby,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public MatchState CurrentState => _matchState.Value;
    public event System.Action<MatchState> OnStateChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        _matchState.OnValueChanged += (oldState, newState) =>
            OnStateChanged?.Invoke(newState);

        // Fire immediately so any already-subscribed listeners get current state
        OnStateChanged?.Invoke(_matchState.Value);

        // Server auto-starts the match shortly after Game scene loads
        if (IsServer)
            StartCoroutine(AutoStartWhenReady());
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStartMatchServerRpc()
    {
        if (_matchState.Value != MatchState.Lobby) return;

        // Assign all teams at match-start when full roster is guaranteed
        // AssignAllTeams() uses connection order: 2p=1v1, 4p=2v2
        TeamManager.Instance.AssignAllTeams();

        StartCoroutine(MatchCountdownRoutine());
    }

    private IEnumerator MatchCountdownRoutine()
    {
        _matchState.Value = MatchState.Countdown;
        yield return new WaitForSeconds(countdownDuration);
        _matchState.Value = MatchState.Playing;
        PlayerSpawner.Instance.SpawnAllPlayers();
    }

    // Waits 1 second after scene load for all NetworkObjects to spawn,
    // then auto-starts the match — no manual Start button needed in Game scene
    private IEnumerator AutoStartWhenReady()
    {
        yield return new WaitForSeconds(1f);
        if (_matchState.Value == MatchState.Lobby)
            RequestStartMatchServerRpc();
    }

    public void EndMatch()
    {
        if (!IsServer) return;
        _matchState.Value = MatchState.MatchEnd;
    }
}