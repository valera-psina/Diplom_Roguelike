using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(AudioSource))]
public class SuicideBomber : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detonationRange = 2f;   // расстояние до игрока для взрыва

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private GameObject explosionEffect;   // префаб эффекта взрыва
    [SerializeField] private AudioClip explosionSound;

    [Header("Flight sound")]
    [SerializeField] private AudioClip flightSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private bool exploded;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;   // не останавливается, пока не взорвётся
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (exploded || player == null) return;

        // Всегда летим к игроку
        agent.SetDestination(player.position);

        // Поворачиваемся лицом к игроку
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            // Если модель смотрит боком, поправьте угол в инспекторе:
            // lookRotation * Quaternion.Euler(0, 90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        // Проверка на взрыв
        if (Vector3.Distance(transform.position, player.position) <= detonationRange)
            Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        // Урон
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage, gameObject);
            }
        }

        // Эффект взрыва — спавним отдельно
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // Если эффект был дочерним – теперь он в корне сцены и не удалится вместе с врагом
            Destroy(fx, 5f); // даём время на проигрыш, подбери нужное
        }

        // Звук
        if (explosionSound != null)
            audioSource.PlayOneShot(explosionSound);

        Destroy(gameObject);
    }

    // Вызывается из анимации через AnimationEvent
    public void PlayFlightSound()
    {
        if (flightSound != null && !exploded)
            audioSource.PlayOneShot(flightSound);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detonationRange);
    }
}