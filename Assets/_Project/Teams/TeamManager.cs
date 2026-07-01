using Unity.Netcode;
using UnityEngine;

public enum TeamType { Red, Blue }

public struct TeamAssignment : INetworkSerializable,
    System.IEquatable<TeamAssignment>
{
    public ulong clientId;
    public TeamType team;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref team);
    }

    public bool Equals(TeamAssignment other) =>
        clientId == other.clientId && team == other.team;
}

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }

    private NetworkList<TeamAssignment> _assignments;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _assignments = new NetworkList<TeamAssignment>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        // Only subscribe to disconnect — we no longer assign on connect
        // Teams are assigned all at once in AssignAllTeams()
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
    }

    // Called by NetworkGameManager.RequestStartMatchServerRpc()
    // before the countdown begins — full player roster guaranteed
    public void AssignAllTeams()
    {
        if (!IsServer) return;
        _assignments.Clear();

        var clients = NetworkManager.Singleton.ConnectedClientsList;
        int total = clients.Count;

        for (int i = 0; i < total; i++)
        {
            ulong clientId = clients[i].ClientId;
            TeamType team;

            if (total <= 2)
            {
                // 2 players: first (host) = Red, second = Blue
                team = (i == 0) ? TeamType.Red : TeamType.Blue;
            }
            else
            {
                // 3-4 players: first half = Red, second half = Blue
                team = (i < total / 2) ? TeamType.Red : TeamType.Blue;
            }

            _assignments.Add(new TeamAssignment
                { clientId = clientId, team = team });

            Debug.Log($"[TEAM] Slot {i} | Client {clientId} → {team}");
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        for (int i = 0; i < _assignments.Count; i++)
        {
            if (_assignments[i].clientId == clientId)
            {
                _assignments.RemoveAt(i);
                break;
            }
        }
        PlayerSpawner.Instance?.RemovePlayer(clientId);
    }

    public TeamType GetTeam(ulong clientId)
    {
        foreach (var a in _assignments)
            if (a.clientId == clientId) return a.team;

        Debug.LogWarning($"[TEAM] No assignment for client {clientId}." +
                         $" Defaulting Red.");
        return TeamType.Red;
    }

    public int GetTeamCount(TeamType team)
    {
        int count = 0;
        foreach (var a in _assignments)
            if (a.team == team) count++;
        return count;
    }
}