using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Health), typeof(Animator))]
public class JumpEnemy : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float jumpRange = 80f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float cooldown = 1f;

    [Header("Jump Movement (sync with animation)")]
    [SerializeField][Range(0f, 1f)] private float moveStartNorm = 0.2f;   // начало рывка (20% анимации)
    [SerializeField][Range(0f, 1f)] private float moveEndNorm = 0.8f;     // конец рывка (80% анимации)

    [Header("Jump Movement")]
    [SerializeField] private float jumpSpeed = 10f;                  // скорость рывка
    [SerializeField][Range(0f, 1f)] private float trackUntilNorm = 0.5f; // до какого момента обновлять цель

    [SerializeField] private AudioClip swooshSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip growlSound;

    public float JumpSpeed => jumpSpeed;
    public float TrackUntilNorm => trackUntilNorm;

    public float MoveStartNorm => moveStartNorm;
    public float MoveEndNorm => moveEndNorm;

    private Animator animator;
    private Health health;
    private bool dead;
    private float lastAttackTime = -Mathf.Infinity;
    private Transform player;
    private Vector3 currentJumpOffset;
    private AudioSource audioSource;

    public float JumpRange => jumpRange;
    public bool CanAttack => Time.time >= lastAttackTime + cooldown && !dead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 60f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void Start()
    {
        health.OnDie += _ => dead = true;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void PerformJumpAttack()
    {
        if (dead || player == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);
        foreach (Collider hit in hits)
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponentInParent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage - (damage / 2), gameObject);
                if (hitSound != null) audioSource.PlayOneShot(hitSound);
            }
        lastAttackTime = Time.time;
    }

    public void PlaySwooshSound()
    {
        if (growlSound != null) audioSource.PlayOneShot(growlSound);
        if (swooshSound == null) return;
        audioSource.PlayOneShot(swooshSound);
    }

    public void GenerateNewJumpOffset()
    {
        // случайное смещение по кругу
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(0.5f, attackRadius * 0.9f); // в пределах радиуса атаки
        currentJumpOffset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
    }

    public Vector3 GetJumpTarget(Vector3 playerPosition)
    {
        return playerPosition + currentJumpOffset;
    }
}