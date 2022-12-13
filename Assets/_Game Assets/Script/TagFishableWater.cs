using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagFishableWater : MonoBehaviour
{
    [SerializeField] TagFishable[] m_fishPrefabs;
    [SerializeField] AnimationCurve m_probabilityCurve;

    public TagFishable GenerateRandomFishPrefab()
    {
        // warning kalo probability curve y-nya lebih / kurang dari fishprefab

        float rand = Random.Range(0.0f, 1.0f);
        float resultY = m_probabilityCurve.Evaluate(rand);
        return m_fishPrefabs[Mathf.FloorToInt(resultY)];
    }
}
