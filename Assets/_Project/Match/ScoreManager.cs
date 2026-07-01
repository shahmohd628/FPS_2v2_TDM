using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public const int KillsToWin = 20;

    private NetworkVariable<int> _redScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _blueScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int RedScore => _redScore.Value;
    public int BlueScore => _blueScore.Value;

    public event System.Action<int, int> OnScoreChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        _redScore.OnValueChanged += (_, __) => OnScoreChanged?.Invoke(_redScore.Value, _blueScore.Value);
        _blueScore.OnValueChanged += (_, __) => OnScoreChanged?.Invoke(_redScore.Value, _blueScore.Value);
    }

    public void RecordKill(ulong killerClientId, ulong victimClientId)
    {
        if (!IsServer) return;

        TeamType killerTeam = TeamManager.Instance.GetTeam(killerClientId);
        if (killerTeam == TeamType.Red) _redScore.Value++;
        else _blueScore.Value++;

        BroadcastKillClientRpc(killerClientId, victimClientId, killerTeam);

        if (_redScore.Value >= KillsToWin || _blueScore.Value >= KillsToWin)
            NetworkGameManager.Instance.EndMatch();
    }

    [ClientRpc]
    private void BroadcastKillClientRpc(ulong killer, ulong victim, TeamType killerTeam)
    {
        KillFeedUI.Instance?.AddKill(killer, victim, killerTeam);
    }

    public void ResetScores()
    {
        if (!IsServer) return;
        _redScore.Value = 0;
        _blueScore.Value = 0;
    }
}