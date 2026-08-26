using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Vector3 overlapCenter = Vector3.zero;
    [SerializeField] private float overlapRadius = 1.2f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip swooshSound;
    [SerializeField] private AudioClip hitSound;
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Animator animator;
    private Health health;
    private Transform attackPoint;
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
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        attackPoint = new GameObject("AttackPoint").transform;
        attackPoint.SetParent(transform);
        attackPoint.localPosition = overlapCenter;

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
        Vector3 center = attackPoint.position;
        Collider[] hits = Physics.OverlapSphere(center, overlapRadius, targetLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage - (damage / 2), gameObject);
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    public void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        if (agent.velocity.magnitude < 0.1f) return;
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(clip);
    }

    public void PlaySwooshSound()
    {
        if (swooshSound == null) return;
        audioSource.PlayOneShot(swooshSound);
    }

    private void OnDeath(GameObject killer)
    {
        dead = true;
        agent.isStopped = true;
        Destroy(gameObject, 3f);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= OnDeath;
    }
}