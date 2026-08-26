using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float stepInterval = 0.5f;

    private float stepTimer;

    void Update()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
