using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPole : MonoBehaviour
{
    public Animator animator { get; private set; }

    [SerializeField] FishingPole_Bait m_bait;
    public FishingPole_Bait bait { get { return m_bait; } }

    [SerializeField] Transform m_baitSpawn;

    public float maxFishingDistance = 10.0f;

    public float baitReleaseToBaitHitMaxTime = 10.0f;
    public AnimationCurve baitReleaseSpeedHorizontal;
    public AnimationCurve baitReleaseSpeedVertical;

    public IEnumerator ReleaseBait(ThirdPersonCharacterController player, float distanceInputPercentage)
    {
        m_bait.transform.SetParent(null, true);
        m_bait.characterController.enabled = true;

        float currentBaitThrowTime = 0.0f;
        float totalDistance = Mathf.Max(maxFishingDistance * distanceInputPercentage, 1.0f);
        Vector3 horizontalTargetPos = player.transform.position + player.transform.forward * totalDistance;

        while (currentBaitThrowTime < baitReleaseToBaitHitMaxTime)
        {
            currentBaitThrowTime += Time.deltaTime;
            float currentDistance = Vector3.Distance(Vector3.Scale(horizontalTargetPos, new Vector3(1.0f, 0.0f, 1.0f)), Vector3.Scale(m_bait.transform.position, new Vector3(1.0f, 0.0f, 1.0f)));
            float currentHSpeed = baitReleaseSpeedHorizontal.Evaluate((totalDistance - currentDistance) / totalDistance) * maxFishingDistance * Time.deltaTime;
            float currentVSpeed = -baitReleaseSpeedVertical.Evaluate((totalDistance - currentDistance) / totalDistance) * maxFishingDistance * Time.deltaTime;
            Vector3 dirHorizontal = (horizontalTargetPos - m_bait.transform.position).normalized * currentHSpeed;
            m_bait.characterController.Move(new Vector3(dirHorizontal.x, currentVSpeed, dirHorizontal.z));
            m_bait.transform.rotation = Quaternion.Lerp(m_baitSpawn.rotation, Quaternion.identity, (totalDistance - currentDistance) / totalDistance);

            yield return new WaitForEndOfFrame();

            if (m_bait.currentCollisionState == BaitCollisionState.NotFishableWater || (m_bait.currentCollisionState == BaitCollisionState.FishableWater && currentBaitThrowTime > 1.0f))
                break;
        }

        if(m_bait.currentCollisionState == BaitCollisionState.FishableWater)
        {
            m_bait.transform.rotation = Quaternion.identity;
            m_bait.transform.position = new Vector3 (horizontalTargetPos.x, m_bait.transform.position.y, horizontalTargetPos.z);
        }
    }

    public IEnumerator ResetBait()
    {
        m_bait.characterController.detectCollisions = false;

        float currentBaitResetTime = 0.0f;
        Vector3 baitSpawnStart = m_baitSpawn.position;
        float totalHDistance = Vector3.Distance(Vector3.Scale(baitSpawnStart, new Vector3(1.0f, 0.0f, 1.0f)), Vector3.Scale(m_bait.transform.position, new Vector3(1.0f, 0.0f, 1.0f)));
        float totalVDistance = Vector3.Distance(Vector3.Scale(baitSpawnStart, Vector3.up), Vector3.Scale(m_bait.transform.position, Vector3.up));

        while (currentBaitResetTime < 1.0f)
        {
            currentBaitResetTime += Time.deltaTime;
            float currentHDistance = Vector3.Distance(Vector3.Scale(baitSpawnStart, new Vector3(1.0f, 0.0f, 1.0f)), Vector3.Scale(m_bait.transform.position, new Vector3(1.0f, 0.0f, 1.0f)));
            float currentHSpeed = baitReleaseSpeedHorizontal.Evaluate((totalHDistance - currentHDistance) / totalHDistance) * maxFishingDistance * Time.deltaTime;
            float currentVDistance = Vector3.Distance(Vector3.Scale(baitSpawnStart, Vector3.up), Vector3.Scale(m_bait.transform.position, Vector3.up));
            float currentVSpeed = baitReleaseSpeedVertical.Evaluate(currentVDistance / totalVDistance) * maxFishingDistance * Time.deltaTime;
            Vector3 dirHorizontal = (baitSpawnStart - m_bait.transform.position).normalized * currentHSpeed;
            m_bait.characterController.Move(new Vector3(dirHorizontal.x, currentVSpeed, dirHorizontal.z));

            yield return new WaitForEndOfFrame();

            if (currentHDistance < 0.25f)
                break;
        }

        // detach hook if exist disini?
        m_bait.characterController.detectCollisions = true;
        m_bait.transform.SetParent(m_baitSpawn);
        m_bait.transform.position = m_baitSpawn.position;
        m_bait.transform.rotation = m_baitSpawn.rotation;
        m_bait.transform.localScale = Vector3.one;
        m_bait.characterController.enabled = false;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // animasi nyala
    }

    private void OnDisable()
    {
        // animasi mati
    }
}
