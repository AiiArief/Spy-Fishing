using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonCharacterController : MonoBehaviour
{
    public CharacterController controller { get; private set; }
    [SerializeField] ThirdPersonCharacterController_AnimatorState m_animState;
    [SerializeField] ThirdPersonCharacterController_UI m_ui;

    [SerializeField] FishingPole m_fishingPole;
    public float maxFishingDistanceInput = 3.0f;
    float m_currentFishingDistanceInput = 0.0f;
    [SerializeField] CinemachineVirtualCamera m_releasedBaitCamera;
    bool m_fishingHasReleased = false;
    bool m_isFishingInWater = false;
    [SerializeField] CinemachineVirtualCamera m_showcaseCamera;
    bool m_isShowCaseFish = false;
    float m_fishingInWaterTimer = 0.0f;

    [SerializeField] Transform m_inventory;
    public Transform inventory { get { return m_inventory; } }

    public float speed = 3f;

    public float turnSmoothTime = 0.0f;
    float m_turnSmoothVelocity;

    public IEnumerator FishingReleaseCoroutine(float fishingDistanceInput)
    {
        yield return StartCoroutine(m_fishingPole.ReleaseBait(this, fishingDistanceInput / maxFishingDistanceInput));

        if (m_fishingPole.bait.currentCollisionState != BaitCollisionState.FishableWater)
        {
            StartCoroutine(CancelFishingCoroutine());
            yield break;
        }

        m_isFishingInWater = true;
        m_fishingInWaterTimer = 0.0f;
        m_animState.animator.SetBool("isFishingInWater", true);
        m_fishingPole.bait.SetFishingInWater(true);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if(!m_fishingHasReleased && !m_isFishingInWater && !m_isShowCaseFish)
        {
            bool fishingDown = Input.GetButtonDown("Fire1");
            bool fishing = Input.GetButton("Fire1");
            bool fishingUp = Input.GetButtonUp("Fire1");
            if(fishingDown)
            {
                m_fishingPole.gameObject.SetActive(true);
                m_fishingHasReleased = false;
                m_animState.animator.SetInteger("moveRange", 0);
                m_animState.animator.SetTrigger("startFishing");
            }
            else if (fishing)
            {
                m_ui.ActivateUI(true, m_currentFishingDistanceInput);
                m_currentFishingDistanceInput = Mathf.Min(maxFishingDistanceInput, m_currentFishingDistanceInput + Time.deltaTime);
                m_animState.animator.SetFloat("currentFishingDistance", m_currentFishingDistanceInput);
                m_fishingPole.animator.SetFloat("currentDistanceInput", m_currentFishingDistanceInput);
            } 
            else if (fishingUp)
            {
                m_ui.ActivateUI(false);
                m_animState.Queue_FishingRelease(this, m_currentFishingDistanceInput);
                m_fishingHasReleased = true;
                m_animState.animator.SetTrigger("fishingRelease");
                m_releasedBaitCamera.gameObject.SetActive(true);
                m_currentFishingDistanceInput = 0.0f;
                m_animState.animator.SetFloat("currentFishingDistance", 0.0f);
                m_fishingPole.animator.SetFloat("currentDistanceInput", 0.0f);
            }
        } 
        
        if(m_isFishingInWater) 
        {
            bool pullFishing = Input.GetButtonUp("Fire1");
            if(pullFishing)
            {
                m_isFishingInWater = false;
                StartCoroutine(PullFishingCoroutine());
            } else
            {
                m_fishingInWaterTimer += Time.deltaTime;
                if (m_fishingInWaterTimer > 3.0f)
                    m_fishingPole.bait.Hooked();
            }
        }

        if (m_currentFishingDistanceInput > 0 || m_fishingHasReleased || m_isShowCaseFish)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetButton("Sprint");

        Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;
        int sprintMultiplier = (sprint) ? 2 : 1;

        m_animState.animator.SetInteger("moveRange", Mathf.RoundToInt(dir.magnitude) * sprintMultiplier);

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = 0.0f;
            transform.rotation = SmoothRotateToward(dir, out targetAngle);

            Vector3 movedir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(movedir.normalized * speed * sprintMultiplier * Time.deltaTime);
        }
    }

    private IEnumerator CancelFishingCoroutine()
    {
        m_animState.animator.SetTrigger("cancelFishing");
        m_animState.animator.SetBool("isFishingInWater", false);
        m_releasedBaitCamera.gameObject.SetActive(false);
        m_fishingPole.bait.SetFishingInWater(false);
        StartCoroutine(m_fishingPole.bait.DetachFishInHookIfExist(this));
        yield return StartCoroutine(m_fishingPole.ResetBait());

        m_animState.animator.SetTrigger("exitFishing");
        m_fishingHasReleased = false;
        m_fishingInWaterTimer = 0.0f;
        m_fishingPole.gameObject.SetActive(false);
    }

    private IEnumerator PullFishingCoroutine()
    {
        if (m_fishingInWaterTimer > 3.0f)
        {
            m_isShowCaseFish = true;
            m_fishingPole.bait.AttachRandomFishFromFishableWater();
        }

        yield return StartCoroutine(CancelFishingCoroutine());

        if(m_isShowCaseFish)
        {
            Vector3 targetDir = Camera.main.transform.position - transform.position;
            targetDir = new Vector3(targetDir.x, 0.0f, targetDir.z).normalized;
            float targetAngle = 0.0f;
            do
            {
                transform.rotation = SmoothRotateToward(targetDir, out targetAngle);

                yield return new WaitForEndOfFrame();
            } 
            while (Vector3.Angle(transform.forward, targetDir) > 10.0f);

            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            m_showcaseCamera.gameObject.SetActive(true);
            m_animState.animator.SetInteger("expression", 3);
            m_inventory.gameObject.SetActive(true);
            m_inventory.GetChild(m_inventory.childCount-1).gameObject.SetActive(true);

            yield return new WaitForSeconds(5.0f);

            m_showcaseCamera.gameObject.SetActive(false);
            m_animState.animator.SetInteger("expression", 0);
            m_inventory.gameObject.SetActive(false);
            m_inventory.GetChild(m_inventory.childCount-1).gameObject.SetActive(false);

            yield return new WaitForSeconds(1.0f);

            m_isShowCaseFish = false;
        }
    }

    private Quaternion SmoothRotateToward(Vector3 dir, out float targetAngle)
    {
        targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref m_turnSmoothVelocity, turnSmoothTime);
        return Quaternion.Euler(0f, angle, 0f);
    }
}
