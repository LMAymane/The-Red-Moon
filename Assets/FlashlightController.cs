using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight; 
    public bool isOn = false;

    [Header("Audio (Optional)")]
    public AudioSource clickSound;

    void Start()
    {
        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;

            if (flashlight != null)
            {
                flashlight.enabled = isOn;
            }

            if (clickSound != null)
            {
                clickSound.Play();
            }
        }
    }
}