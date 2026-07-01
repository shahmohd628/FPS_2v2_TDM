using UnityEngine;

[RequireComponent(typeof(Health))]
public class RespawnSystem : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private GameObject[] visualObjects; // Body mesh, weapon model etc.
    [SerializeField] private Collider[] hitboxColliders;

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDied -= HandleDied;
    }

    private void HandleDied(ulong killerClientId)
    {
        // Disable input scripts
        if (playerController != null) playerController.enabled = false;
        if (weaponController != null) weaponController.enabled = false;

        // Hide visuals so the corpse doesn't block view/shots
        foreach (var obj in visualObjects)
            if (obj != null) obj.SetActive(false);

        // Disable hit detection on a dead body
        foreach (var col in hitboxColliders)
            if (col != null) col.enabled = false;
    }

    // Called automatically because PlayerSpawner destroys this whole
    // GameObject and spawns a fresh one — so no "revive" method is needed.
    // This script's job is purely to make death LOOK right before despawn.
}