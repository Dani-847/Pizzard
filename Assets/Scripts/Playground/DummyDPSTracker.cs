// Assets/Scripts/Playground/DummyDPSTracker.cs
//
// Collision routing note:
//   CharacterProjectile.OnTriggerEnter2D checks CompareTag("Boss"), then calls
//   GetComponent<Pizzard.Bosses.BossBase>().TakeDamage(int).
//   DummyDPSTracker extends BossBase to intercept that call without being a real boss.
//   The TrainingDummy GameObject must be tagged "Boss".
//   It overrides TakeDamage so the dummy never dies — all damage is just recorded for DPS.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DummyDPSTracker : Pizzard.Bosses.BossBase
{
    [SerializeField] private TextMeshPro dpsText; // World-space TMP (not UGUI)

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _flashCoroutine;
    private readonly List<(float time, float damage)> _hits = new();

    protected override void Awake()
    {
        base.Awake();
        // CharacterProjectile checks CompareTag("Boss") — ensure this is set regardless of Inspector
        gameObject.tag = "Boss";
        if (dpsText == null)
            dpsText = GetComponentInChildren<TextMeshPro>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    /// <summary>
    /// Called by CharacterProjectile when a player spell hits the training dummy.
    /// Overrides BossBase.TakeDamage so the dummy never dies.
    /// </summary>
    public override void TakeDamage(int damage)
    {
        // Invincible — no HP reduction, no death
        RegisterHit(damage);
        FlashWhite();
        Debug.Log($"[DummyDPSTracker] Registered hit: {damage} damage.");
    }

    private void FlashWhite()
    {
        if (_spriteRenderer == null) return;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        _spriteRenderer.color = _originalColor;
        _flashCoroutine = null;
    }

    /// <summary>
    /// Records a damage hit with the current timestamp for the rolling DPS window.
    /// </summary>
    public void RegisterHit(float damage)
    {
        _hits.Add((Time.time, damage));
    }

    private void Update()
    {
        // Prune hits older than 3 seconds (rolling window)
        float cutoff = Time.time - 3f;
        _hits.RemoveAll(h => h.time < cutoff);

        float dps = _hits.Count > 0 ? _hits.Sum(h => h.damage) / 3f : 0f;
        if (dpsText != null)
            dpsText.text = $"{dps:F1}";
    }
}
