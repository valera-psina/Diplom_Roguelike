using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileData data;

    [Header("Damage Over Time")]
    [SerializeField] private float baseDotDamagePerTick = 5f;
    [SerializeField] private float dotTickInterval = 0.5f;
    [SerializeField] private float dotDuration = 3f;

    private bool hasHit = false;
    private Vector3 previousPosition;
    private PlayerUI cachedPlayerUI;
    private PlayerUpgrades playerUpgrades;
    private GameObject damageSource;
    private float overrideDamage = -1f;
    private bool useOverrideDamage = false;

    public void Init(ProjectileData projectileData)
    {
        data = projectileData;
    }

    public void SetPlayerUpgrades(PlayerUpgrades upgrades, GameObject owner)
    {
        playerUpgrades = upgrades;
        damageSource = owner;
    }

    void Start()
    {
        if (data == null)
        {
            Destroy(gameObject);
            return;
        }

        previousPosition = transform.position;
        cachedPlayerUI = FindFirstObjectByType<PlayerUI>();
        Destroy(gameObject, data.maxLifeTime);
    }

    void Update()
    {
        if (hasHit || data == null) return;

        Vector3 movement = transform.forward * (data.speed * Time.deltaTime);
        Vector3 newPosition = previousPosition + movement;

        if (Physics.SphereCast(previousPosition, data.sphereCastRadius, movement.normalized,
                               out RaycastHit hitInfo, movement.magnitude, data.collisionMask))
        {
            OnHit(hitInfo);
            return;
        }

        transform.position = newPosition;
        previousPosition = newPosition;
    }

    private void OnHit(RaycastHit hit)
    {
        if (hasHit) return;
        hasHit = true;

        Health targetHealth = hit.collider.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsGhost && (data == null || !data.isGhostDamage))
        {
            Destroy(gameObject);
            return;
        }

        if (targetHealth != null)
        {
            float dmg = useOverrideDamage ? overrideDamage : data.damage;
            GameObject source = damageSource != null ? damageSource : gameObject;
            targetHealth.TakeDamage(dmg, source);
            if (cachedPlayerUI != null)
                cachedPlayerUI.ShowHitMarker();

            if (playerUpgrades != null && playerUpgrades.DotsMultiplier > 1f)
            {
                float scaledDotDamage = baseDotDamagePerTick * playerUpgrades.DotsMultiplier;
                targetHealth.ApplyDot(scaledDotDamage, dotTickInterval, dotDuration, damageSource);
            }
        }

        if (data.impactForce > 0f && hit.rigidbody != null)
        {
            hit.rigidbody.AddForceAtPosition(transform.forward * data.impactForce,
                                             hit.point, ForceMode.Impulse);
        }

        bool isNotEnemy = !hit.collider.CompareTag("Enemy")
                          && !hit.collider.transform.root.CompareTag("Enemy")
                          && !hit.collider.CompareTag("Boss")
                          && !hit.collider.transform.root.CompareTag("Boss");

        if (isNotEnemy && data.impactEffect != null)
        {
            GameObject effect = Instantiate(data.impactEffect, hit.point,
                                            Quaternion.LookRotation(hit.normal));
            if (effect.TryGetComponent<ParticleSystem>(out var ps))
                Destroy(effect, ps.main.duration);
            else
                Destroy(effect, 3f);
        }

        if (isNotEnemy && data.bulletHolePrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(hit.normal)
                                  * Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            GameObject decal = Instantiate(data.bulletHolePrefab,
                                           hit.point + hit.normal * data.decalOffset, rotation);
            Destroy(decal, data.decalLifetime);
        }

        // Скрываем модель пули и удаляем объект
        if (TryGetComponent<MeshRenderer>(out var mesh))
            mesh.enabled = false;

        Destroy(gameObject, 0.1f);
    }
}