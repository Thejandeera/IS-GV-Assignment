using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    [Header("Lightning Settings")]
    public Light lightningLight;           
    
    // FIXED: Lowered these numbers so lightning strikes much more often! (Every 2 to 7 seconds)
    public float minTimeBetweenFlashes = 2f;
    public float maxTimeBetweenFlashes = 7f;
    public float flashIntensity = 5f;      

    [Header("Audio Settings")]
    public AudioSource thunderAudio;       

    private float originalIntensity;

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
            // 1. Wait for a random amount of time (now much shorter)
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
        lightningLight.intensity = flashIntensity;
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f)); 
        lightningLight.intensity = originalIntensity;

        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

        lightningLight.intensity = flashIntensity * 0.6f; 
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        lightningLight.intensity = originalIntensity;

        // --- THE AUDIO ---
        if (thunderAudio != null)
        {
            // FIXED: Drastically reduced the delay so the thunder cracks almost immediately!
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            
            thunderAudio.pitch = Random.Range(0.85f, 1.15f); 
            thunderAudio.Play();
        }
    }
}
