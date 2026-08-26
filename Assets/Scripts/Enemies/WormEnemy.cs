using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class WormEnemy : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private float yOffset = -1f;

    [Header("Attack settings")]
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private Vector3 muzzleOffset = Vector3.up;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioClip shootSound;

    [Header("Hitbox & UI")]
    [SerializeField] private Collider wormCollider;
    [SerializeField] private GameObject healthBarObject;
    [SerializeField] private float hitboxDuration = 1f;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 3f;

    private Transform attackPoint;
    private Animator anim;
    private Health health;
    private Transform player;
    private AudioSource audioSource;
    private bool dead;
    private Coroutine hitboxCoroutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        transform.position += new Vector3(0f, yOffset, 0f);

        attackPoint = new GameObject("AttackPoint").transform;
        attackPoint.SetParent(transform);
        attackPoint.localPosition = muzzleOffset;

        if (wormCollider != null) wormCollider.enabled = false;
        if (healthBarObject != null) healthBarObject.SetActive(false);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (health != null)
            health.OnDie += OnDeath;
    }

    public void PerformAttack()
    {
        if (dead || projectilePrefab == null || player == null) return;

        Vector3 direction = (player.position - attackPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        Instantiate(projectilePrefab, attackPoint.position, rotation);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    public void EnableHitbox()
    {
        if (dead) return;

        if (wormCollider != null) wormCollider.enabled = true;
        if (healthBarObject != null) healthBarObject.SetActive(true);

        if (hitboxCoroutine != null) StopCoroutine(hitboxCoroutine);
        hitboxCoroutine = StartCoroutine(DisableHitboxAfterDelay(hitboxDuration));
    }

    private IEnumerator DisableHitboxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!dead)
        {
            if (wormCollider != null) wormCollider.enabled = false;
            if (healthBarObject != null) healthBarObject.SetActive(false);
        }
        hitboxCoroutine = null;
    }

    private void ForceDisableHitbox()
    {
        if (wormCollider != null) wormCollider.enabled = false;
        if (healthBarObject != null) healthBarObject.SetActive(false);
        if (hitboxCoroutine != null)
        {
            StopCoroutine(hitboxCoroutine);
            hitboxCoroutine = null;
        }
    }

    private void OnDeath(GameObject killer)
    {
        if (dead) return;
        dead = true;
        anim.SetBool("isShooting", false);
        ForceDisableHitbox();
        Destroy(gameObject, destroyDelay);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= OnDeath;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}