using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    [Header("Lightning Settings")]
    public Light lightningLight;           
    
   
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
            
            float waitTime = Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes);
            yield return new WaitForSeconds(waitTime);

            
            StartCoroutine(FlashLightning());
        }
    }

    IEnumerator FlashLightning()
    {
        if (lightningLight == null) yield break;

       
        lightningLight.intensity = flashIntensity;
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f)); 
        lightningLight.intensity = originalIntensity;

        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

        lightningLight.intensity = flashIntensity * 0.6f; 
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        lightningLight.intensity = originalIntensity;

        
        if (thunderAudio != null)
        {
           
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            
            thunderAudio.pitch = Random.Range(0.85f, 1.15f); 
            thunderAudio.Play();
        }
    }
}
