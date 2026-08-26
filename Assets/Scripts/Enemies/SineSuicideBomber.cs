using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(AudioSource))]
public class MeshSineSuicideBomber : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detonationRange = 2f;

    [Header("Sine wave")]
    [SerializeField] private float sineAmplitude = 0.5f;
    [SerializeField] private float sineFrequency = 2f;

    [Header("Explosion")]
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;

    [Header("Flight sound")]
    [SerializeField] private AudioClip flightSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Health health;                    // ← добавили
    private Transform player;
    private float baseHeight;
    private bool exploded;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<Health>();      // ← получаем

        agent.speed = moveSpeed;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;
        agent.stoppingDistance = 0f;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        baseHeight = transform.position.y;
    }

    private void Update()
    {
        if (exploded || player == null) return;

        agent.SetDestination(player.position);

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float yOffset = Mathf.Sin(Time.time * sineFrequency) * sineAmplitude;
        Vector3 pos = transform.position;
        pos.y = baseHeight + yOffset;
        transform.position = pos;

        if (Vector3.Distance(transform.position, player.position) <= detonationRange)
            Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        agent.isStopped = true;

        Vector3 blastPosition = explosionPoint != null ? explosionPoint.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(blastPosition, explosionRadius, damageLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage, gameObject);
            }
        }

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, blastPosition, Quaternion.identity);
            Destroy(fx, 5f);
        }

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, blastPosition);

        // Вместо Destroy – убиваем через Health, чтобы комната узнала о смерти
        if (health != null)
            health.Kill(gameObject);
        else
            Destroy(gameObject);  // fallback, если вдруг Health нет
    }

    public void PlayFlightSound()
    {
        if (flightSound != null && !exploded)
            audioSource.PlayOneShot(flightSound);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoPos = explosionPoint != null ? explosionPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoPos, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoPos, detonationRange);
    }
}