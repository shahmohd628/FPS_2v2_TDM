using Unity.Netcode;
using UnityEngine;

public class TeamMember : NetworkBehaviour
{
    [SerializeField] private Renderer teamIndicatorRenderer;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;

    private NetworkVariable<TeamType> _team =
        new NetworkVariable<TeamType>(TeamType.Red,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public TeamType Team => _team.Value;

    public override void OnNetworkSpawn()
    {
        _team.OnValueChanged += (_, newTeam) => ApplyTeamColor(newTeam);

        if (IsServer)
        {
            _team.Value = TeamManager.Instance.GetTeam(OwnerClientId);
            Debug.Log($"[TEAM MEMBER] OwnerClientId={OwnerClientId} assigned Team={_team.Value}");

        }

        // Apply immediately — handles late joiners and host's own player
        ApplyTeamColor(_team.Value);
    }

    private void ApplyTeamColor(TeamType team)
    {
        if (teamIndicatorRenderer == null) return;
        teamIndicatorRenderer.material =
            team == TeamType.Red ? redMaterial : blueMaterial;
    }
}