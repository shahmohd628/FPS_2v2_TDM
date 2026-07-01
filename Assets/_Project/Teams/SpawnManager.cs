using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Team Spawn Points")]
    [SerializeField] private Transform[] redTeamSpawns;
    [SerializeField] private Transform[] blueTeamSpawns;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Transform GetSpawnPoint(TeamType team)
    {
        Transform[] pool = team == TeamType.Red ? redTeamSpawns : blueTeamSpawns;

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning($"No spawn points assigned for {team}! Using origin.");
            return transform;
        }

        int index = Random.Range(0, pool.Length);
        return pool[index];
    }

    // Useful for debugging in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (redTeamSpawns != null)
            foreach (var t in redTeamSpawns)
                if (t != null) Gizmos.DrawWireSphere(t.position, 0.5f);

        Gizmos.color = Color.blue;
        if (blueTeamSpawns != null)
            foreach (var t in blueTeamSpawns)
                if (t != null) Gizmos.DrawWireSphere(t.position, 0.5f);
    }
}