using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCharacterController_AnimatorState : MonoBehaviour
{
    public Animator animator { get; private set; }

    ThirdPersonCharacterController m_player;
    float m_currentDistanceInput = 0.0f;

    [SerializeField] AudioSource m_stepSFX;
    [SerializeField] AudioSource m_currentDistanceInput1SFX;
    [SerializeField] AudioSource m_currentDistanceInput2SFX;
    [SerializeField] AudioSource m_currentDistanceInput3SFX;
    [SerializeField] AudioSource m_throwSFX;

    public void Queue_FishingRelease(ThirdPersonCharacterController player, float currentDistanceInput)
    {
        m_player = player;
        m_currentDistanceInput = currentDistanceInput;
    }

    public void AnimatorState_FishingRelease()
    {
        StartCoroutine(m_player.FishingReleaseCoroutine(m_currentDistanceInput));
    }

    public void AnimatorState_SFX(AudioSource sfxPrefab)
    {
        Destroy(Instantiate(sfxPrefab, transform.position, Quaternion.identity).gameObject, 1.0f);
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
