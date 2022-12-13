using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdPersonCharacterController_UI : MonoBehaviour
{
    Quaternion originalCamRotation;

    [SerializeField] Slider m_slider;

    public void ActivateUI(bool active, float currentDistanceInput = 0.0f)
    {
        if (active && !gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        if (!active && gameObject.activeInHierarchy) gameObject.SetActive(false);

        if(active)
            m_slider.value = currentDistanceInput;
        else
            m_slider.value = 0.0f;

    }

    private void Start()
    {
        originalCamRotation = transform.rotation;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation * originalCamRotation;
    }
}
