using System.Collections;
using UnityEngine;

public class PblobRhythmManager : MonoBehaviour
{
    [Header("Rhythm Configuration")]
    public float bpm = 120f;
    public float beatThreshold = 0.1f;
    public int beatsPerMeasure = 4;
    
    [Header("Visual Feedback")]
    public UnityEngine.UI.Image beatIndicator;
    public Color beatColor = Color.red;
    public Color offBeatColor = Color.white;
    
    private float beatDuration;
    private float nextBeatTime;
    private int currentBeat = 0;
    private int currentMeasure = 0;
    private bool rhythmActive = false;
    
    // Eventos
    public System.Action<int> OnBeat;
    public System.Action<int> OnMeasure;
    public System.Action OnStrongBeat;
    public System.Action OnBeatWindow;

    void Start()
    {
        beatDuration = 60f / bpm;
        Debug.Log($"🎵 Rhythm Manager iniciado - BPM: {bpm}");
    }
    
    void Update()
    {
        if (!rhythmActive || SoundManager.Instance == null || !SoundManager.Instance.GetMusicSource().isPlaying) 
            return;
        
        float currentTime = SoundManager.Instance.GetMusicSource().time;
        
        // Detección de beats
        if (currentTime >= nextBeatTime - beatThreshold)
        {
            OnBeatWindow?.Invoke();
        }
        
        if (currentTime >= nextBeatTime)
        {
            currentBeat++;
            OnBeat?.Invoke(currentBeat);
            
            if (currentBeat % 4 == 0)
            {
                OnStrongBeat?.Invoke();
            }
            
            if (currentBeat % beatsPerMeasure == 0)
            {
                currentMeasure++;
                OnMeasure?.Invoke(currentMeasure);
            }
            
            if (beatIndicator != null)
            {
                StartCoroutine(FlashBeatIndicator());
            }
            
            nextBeatTime += beatDuration;
        }
    }
    
    public void StartRhythm()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("❌ SoundManager no encontrado");
            return;
        }
        
        // Asegurar que la música del boss está sonando
        SoundManager.Instance.PlayBossMusic();
        
        nextBeatTime = 0f;
        currentBeat = 0;
        currentMeasure = 0;
        rhythmActive = true;
        
        Debug.Log("🎵 Ritmo del boss iniciado");
    }
    
    public void StopRhythm()
    {
        rhythmActive = false;
        // NO detenemos la música aquí, solo el ritmo
        Debug.Log("🎵 Ritmo del boss detenido");
    }
    
    private IEnumerator FlashBeatIndicator()
    {
        if (beatIndicator != null)
        {
            beatIndicator.color = beatColor;
            yield return new WaitForSeconds(0.1f);
            beatIndicator.color = offBeatColor;
        }
    }
    
    public float GetBeatDuration() => beatDuration;
    public int GetCurrentBeat() => currentBeat;
    public int GetCurrentMeasure() => currentMeasure;
    public bool IsRhythmActive => rhythmActive;
    
    public bool IsOnBeat(float margin = 0.1f)
    {
        if (!rhythmActive || SoundManager.Instance == null) return false;
        float timeUntilNextBeat = nextBeatTime - SoundManager.Instance.GetMusicSource().time;
        return Mathf.Abs(timeUntilNextBeat) < margin;
    }
}