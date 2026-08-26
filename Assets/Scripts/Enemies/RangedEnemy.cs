using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

[RequireComponent(typeof(NavMeshAgent), typeof(Health), typeof(Animator))]
public class RangedEnemy : MonoBehaviour
{
    [SerializeField] private float attackRange = 100f;
    [SerializeField] private Vector3 muzzleOffset = Vector3.zero;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Animator animator;
    private Health health;
    private Transform attackPoint;
    private Transform target;
    private bool dead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 80f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        attackPoint = new GameObject("AttackPoint").transform;
        attackPoint.SetParent(transform);
        attackPoint.localPosition = muzzleOffset;

        agent.stoppingDistance = attackRange * 0.9f;
    }

    private void Start()
    {
        if (health != null)
            health.OnDie += OnDeath;
    }

    public void PerformAttack()
    {
        if (dead) return;
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!", this);
            return;
        }

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
        if (target == null) return;

        float distance = Vector3.Distance(attackPoint.position, target.position);
        if (distance > attackRange) return;

        Vector3 direction = (target.position - attackPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject projectileInstance = Instantiate(projectilePrefab, attackPoint.position, rotation);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    private void OnDeath(GameObject killer)
    {
        dead = true;
        agent.isStopped = true;
    }
}