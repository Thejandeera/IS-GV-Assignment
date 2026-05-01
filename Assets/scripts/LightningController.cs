using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    [Header("Lightning Settings")]
    public Light lightningLight;           // Drag your Directional Light (Sun) here
    public float minTimeBetweenFlashes = 8f;
    public float maxTimeBetweenFlashes = 20f;
    public float flashIntensity = 5f;      // How bright the flash is

    [Header("Audio Settings")]
    public AudioSource thunderAudio;       // Drag this object's Audio Source here

    private float originalIntensity;

    // OnEnable runs every time this object is turned on
    void OnEnable()
    {
        if (lightningLight != null)
        {
            originalIntensity = lightningLight.intensity;
        }
        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            // 1. Wait for a random amount of time
            float waitTime = Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes);
            yield return new WaitForSeconds(waitTime);

            // 2. Trigger the flash sequence
            StartCoroutine(FlashLightning());
        }
    }

    IEnumerator FlashLightning()
    {
        if (lightningLight == null) yield break;

        // --- THE VISUAL FLASH ---
        // Flash 1 (Main Strike)
        lightningLight.intensity = flashIntensity;
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f)); // Very fast flash
        lightningLight.intensity = originalIntensity;

        // Tiny dark pause
        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

        // Flash 2 (Smaller secondary flicker that real lightning does)
        lightningLight.intensity = flashIntensity * 0.6f;
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        lightningLight.intensity = originalIntensity;

        // --- THE AUDIO (Delayed realistically) ---
        if (thunderAudio != null)
        {
            // Light travels faster than sound. Wait 0.5 to 2 seconds before the boom hits!
            yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));

            // Randomize the pitch slightly so the thunder sounds different every time
            thunderAudio.pitch = Random.Range(0.85f, 1.15f);
            thunderAudio.Play();
        }
    }
}