using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject hitSparkPrefab;
    [SerializeField] private LayerMask hitMask = ~0;

    private int _currentMag;
    private int _reserveAmmo;
    private float _nextFireTime;
    private bool _isReloading;

    private int _recoilIndex;
    private float _recoilResetTimer;
    private const float RecoilResetDelay = 0.4f;

    public event System.Action<int, int> OnAmmoChanged;
    public event System.Action OnReloadStarted;

    public override void OnNetworkSpawn()
    {
        _currentMag = weaponData.magazineSize;
        _reserveAmmo = weaponData.reserveAmmo;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (NetworkGameManager.Instance != null &&
            NetworkGameManager.Instance.CurrentState != MatchState.Playing) return;

        if (_recoilIndex > 0)
        {
            _recoilResetTimer -= Time.deltaTime;
            if (_recoilResetTimer <= 0f) _recoilIndex = 0;
        }

        if (Input.GetButton("Fire1") && CanFire())
            Fire();

        if (Input.GetKeyDown(KeyCode.R) && !_isReloading
            && _currentMag < weaponData.magazineSize
            && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }

    bool CanFire() =>
        !_isReloading && _currentMag > 0 && Time.time >= _nextFireTime;

    void Fire()
    {
        _nextFireTime = Time.time + 60f / weaponData.fireRateRPM;
        _currentMag--;
        OnAmmoChanged?.Invoke(_currentMag, _reserveAmmo);

        ApplyRecoilPattern();
        SpawnMuzzleFlash();

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, hitMask))
        {
            FireServerRpc(hit.point, hit.normal);
        }
    }

    void ApplyRecoilPattern()
    {
        if (weaponData.recoilPattern == null || weaponData.recoilPattern.Length == 0)
            return;

        int idx = Mathf.Min(_recoilIndex, weaponData.recoilPattern.Length - 1);
        Vector2 recoil = weaponData.recoilPattern[idx];

        GetComponent<PlayerController>().AddRecoil(recoil);

        _recoilIndex++;
        _recoilResetTimer = RecoilResetDelay;
    }

    void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || firePoint == null) return;
        GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        Destroy(flash, 0.15f);
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        Collider[] cols = Physics.OverlapSphere(hitPoint, 0.3f);
        foreach (var col in cols)
        {
            var health = col.GetComponentInParent<Health>();
            var targetTeam = col.GetComponentInParent<TeamMember>();
            var myTeam = GetComponent<TeamMember>();

            if (health != null && col.transform.root != transform.root)
            {
                // Skip damage if same team
                if (targetTeam != null && myTeam != null && targetTeam.Team == myTeam.Team)
                    continue;

                health.TakeDamage(weaponData.damage, OwnerClientId);
                break;
            }
        }
        SpawnHitSparkClientRpc(hitPoint, hitNormal);
    }
    [ClientRpc]
    private void SpawnHitSparkClientRpc(Vector3 point, Vector3 normal)
    {
        if (hitSparkPrefab == null) return;
        GameObject spark = Instantiate(hitSparkPrefab, point, Quaternion.LookRotation(normal));
        Destroy(spark, 0.5f);
    }

    private IEnumerator Reload()
    {
        _isReloading = true;
        OnReloadStarted?.Invoke();
        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = weaponData.magazineSize - _currentMag;
        int toLoad = Mathf.Min(needed, _reserveAmmo);
        _currentMag += toLoad;
        _reserveAmmo -= toLoad;

        _isReloading = false;
        OnAmmoChanged?.Invoke(_currentMag, _reserveAmmo);
    }

    public int CurrentMag => _currentMag;
    public int ReserveAmmo => _reserveAmmo;
}