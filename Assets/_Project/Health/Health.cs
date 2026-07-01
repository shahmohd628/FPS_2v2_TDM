using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private NetworkVariable<int> _currentHealth =
        new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public int CurrentHealth => _currentHealth.Value;
    public int MaxHealth => maxHealth;
    public bool IsDead => _currentHealth.Value <= 0;

    public event System.Action<int> OnHealthChanged;
    public event System.Action<ulong> OnDied;

    public override void OnNetworkSpawn()
    {
        _currentHealth.OnValueChanged += (oldVal, newVal) =>
            OnHealthChanged?.Invoke(newVal);

        if (IsServer)
            _currentHealth.Value = maxHealth;

        // Ensure UI gets the initial value too (handles late joiners)
        OnHealthChanged?.Invoke(_currentHealth.Value);
    }

    public void TakeDamage(int amount, ulong killerClientId)
    {
        if (!IsServer || IsDead) return;

        _currentHealth.Value = Mathf.Max(0, _currentHealth.Value - amount);

        if (_currentHealth.Value <= 0)
            HandleDeath(killerClientId);
    }

    private void HandleDeath(ulong killerClientId)
    {
        ScoreManager.Instance.RecordKill(killerClientId, OwnerClientId);
        OnDied?.Invoke(killerClientId);
        PlayerSpawner.Instance.RequestRespawn(OwnerClientId);
    }
}