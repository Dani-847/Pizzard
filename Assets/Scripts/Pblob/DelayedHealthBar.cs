using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss health bar: uses existing scene-placed children.
/// - BackHealthBar  (orange, delayed drain animation)
/// - FrontHealthBar (red, instant health tracking)
/// Both must be already set up in the scene with Image.Type=Filled, Horizontal fill.
/// This script only modifies fillAmount — it does NOT build or destroy any children.
/// </summary>
public class DelayedHealthBar : MonoBehaviour
{
    [Header("Scene References (auto-found by name if unassigned)")]
    public Image frontImg;   // "FrontHealthBar" — red, tracks HP instantly
    public Image backImg;    // "BackHealthBar"  — orange, drains slowly

    private Coroutine drainRoutine;

    private void Awake()
    {
        // Auto-find from scene children if not wired in inspector
        if (frontImg == null) frontImg = FindChildImage("FrontHealthBar");
        if (backImg  == null) backImg  = FindChildImage("BackHealthBar");

        if (frontImg == null) Debug.LogError("[DelayedHealthBar] FrontHealthBar child not found!");
        if (backImg  == null) Debug.LogError("[DelayedHealthBar] BackHealthBar child not found!");
    }

    // ──────────────────────────────────── Public API

    /// <summary>
    /// Call with ratio 0–1. Red bar moves instantly, orange drains after a delay.
    /// </summary>
    public void SetHealth(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (frontImg != null)
            frontImg.fillAmount = ratio;

        if (drainRoutine != null) StopCoroutine(drainRoutine);
        if (gameObject.activeInHierarchy)
            drainRoutine = StartCoroutine(DrainBack(ratio));
    }

    public void SetHealth(float current, float max)
    {
        if (max > 0f) SetHealth(current / max);
    }

    // ──────────────────────────────────── Internals

    private Image FindChildImage(string childName)
    {
        Transform t = transform.Find(childName);
        if (t != null) return t.GetComponent<Image>();
        return null;
    }

    private IEnumerator DrainBack(float target)
    {
        yield return new WaitForSeconds(Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainDelay);

        float speed = Pizzard.Core.GameBalance.Bosses.Pblob.HealthBarDrainSpeed;
        while (backImg != null && Mathf.Abs(backImg.fillAmount - target) > 0.001f)
        {
            backImg.fillAmount = Mathf.MoveTowards(backImg.fillAmount, target, speed * Time.deltaTime);
            yield return null;
        }

        if (backImg != null) backImg.fillAmount = target;
    }
}