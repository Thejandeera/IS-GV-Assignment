# LightningController.cs - Explanation

## Role Alignment
**Graphics and Visualization (GV): The World Builder**
This script contributes heavily to the visual fidelity, lighting, and environmental design of the 3D world, specifically during the "Storm" weather state.

## Code Explanation & Mechanism

### 1. The Lightning Loop
```csharp
IEnumerator LightningLoop()
{
    while (true)
    {
        float waitTime = Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes);
        yield return new WaitForSeconds(waitTime);

        StartCoroutine(FlashLightning());
    }
}
```
**Mechanism & Theory:**
- **Coroutines:** Instead of using the `Update()` method which runs every frame (and can be computationally expensive to track timers), this uses Unity's `IEnumerator` coroutines. Coroutines allow execution to yield (pause) for a set amount of time without blocking the main thread.
- **Randomization:** Uses `Random.Range` to make the lightning unpredictable, mimicking natural weather patterns rather than a robotic loop.

### 2. Flashing and Audio Sync
```csharp
IEnumerator FlashLightning()
{
    // First flash
    lightningLight.intensity = flashIntensity;
    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f)); 
    lightningLight.intensity = originalIntensity;

    yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

    // Second smaller flash
    lightningLight.intensity = flashIntensity * 0.6f; 
    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
    lightningLight.intensity = originalIntensity;

    // Audio Sync
    if (thunderAudio != null)
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.4f)); // Delay for sound travel
        thunderAudio.pitch = Random.Range(0.85f, 1.15f); 
        thunderAudio.Play();
    }
}
```
**Mechanism & Theory:**
- **Visual Impact:** Manipulates the `intensity` of a Light component rapidly to create a strobe effect. It does a strong flash followed by a slightly weaker secondary flash to emulate multi-stroke lightning.
- **Physical Simulation (Audio Sync):** In the real world, light travels faster than sound. The script simulates this by yielding for a small random delay before playing the thunder audio.
- **Audio Variation:** It slightly randomized the `pitch` of the thunder audio clip. Pitch shifting prevents the sound from becoming repetitive and artificial, maintaining the player's immersion in the designed environment.
