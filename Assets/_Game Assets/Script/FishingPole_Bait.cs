using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BaitCollisionState
{
    None, FishableWater, NotFishableWater
}

public class FishingPole_Bait : MonoBehaviour
{
    CharacterController m_characterController;
    public CharacterController characterController { get { return m_characterController; } }

    Animator m_animator;
    public Animator animator { get { return m_animator; } }

    [SerializeField] Transform m_fxParent;
    [SerializeField] GameObject m_splashFXPrefab;
    Coroutine m_hookedCoroutine;

    public BaitCollisionState currentCollisionState { get; private set; } = BaitCollisionState.None;

    [SerializeField] Transform m_hook;
    public TagFishableWater currentFishableWater { get; private set; }
    public TagFishable currentHookedFish { get; private set; }

    public void SetFishingInWater(bool isFishingInWater)
    {
        if(currentCollisionState == BaitCollisionState.FishableWater)
            Destroy(Instantiate(m_splashFXPrefab, m_fxParent.position, Quaternion.identity), 1.0f);

        m_animator.SetBool("isFishingInWater", isFishingInWater);
    }

    public void Hooked()
    {
        if(m_hookedCoroutine == null)
            m_hookedCoroutine = StartCoroutine(HookedCoroutine());
    }

    public void AttachRandomFishFromFishableWater()
    {
        currentHookedFish = Instantiate(currentFishableWater.GenerateRandomFishPrefab());
        currentHookedFish.transform.SetParent(m_hook, false);

        StopCoroutine(m_hookedCoroutine);
        m_hookedCoroutine = null;
    }

    public IEnumerator DetachFishInHookIfExist(ThirdPersonCharacterController player)
    {
        if (!currentHookedFish)
            yield break;

        while (Vector3.Distance(currentHookedFish.transform.position, player.transform.position) > 1.5f)
            yield return new WaitForEndOfFrame();

        currentHookedFish.transform.SetParent(player.inventory, false);
        currentHookedFish.gameObject.SetActive(false);
        currentHookedFish = null;
    }

    private IEnumerator HookedCoroutine()
    {
        m_animator.SetTrigger("hooked");
        while(true)
        {
            Destroy(Instantiate(m_splashFXPrefab, m_fxParent.position, Quaternion.identity), 1.0f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        currentFishableWater = hit.gameObject.GetComponent<TagFishableWater>();
        if(currentFishableWater)
        {
            currentCollisionState = BaitCollisionState.FishableWater;
            return;
        }

        currentCollisionState = BaitCollisionState.NotFishableWater;
    }

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        currentCollisionState = BaitCollisionState.None;
    }
}
