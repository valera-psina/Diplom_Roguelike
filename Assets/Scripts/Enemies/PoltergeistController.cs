using UnityEngine;

public class PoltergeistController : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private GameObject damageZoneObject;
    [SerializeField] private AudioClip swooshSound;

    [Tooltip("Компенсация поворота модели. 180, если модель смотрит назад.")]
    [SerializeField] private float modelRotationOffset = 180f;

    private Transform target;
    private AudioSource audioSource;

    public bool IsPlayerDetected { get; private set; }
    public bool IsInAttackRange { get; private set; }
    public bool HasNoticed { get; set; }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) target = playerObj.transform;
        }

        if (damageZoneObject != null)
            damageZoneObject.SetActive(false);
    }

    private void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        IsPlayerDetected = distance <= detectionRange;
        IsInAttackRange = distance <= attackRange;

        if (!IsPlayerDetected)
            HasNoticed = false;

        if (IsPlayerDetected)
        {
            MoveTowardsTarget();
        }

        if (damageZoneObject != null)
            damageZoneObject.SetActive(IsInAttackRange);
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion adjustedRotation = targetRotation * Quaternion.Euler(0, modelRotationOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, adjustedRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    public void PlaySwooshSound()
    {
        if (swooshSound == null) return;
        audioSource.PlayOneShot(swooshSound);
    }
}