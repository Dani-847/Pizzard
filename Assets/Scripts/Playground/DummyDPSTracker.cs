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

    private readonly List<(float time, float damage)> _hits = new();

    /// <summary>
    /// Called by CharacterProjectile when a player spell hits the training dummy.
    /// Overrides BossBase.TakeDamage so the dummy never dies.
    /// </summary>
    public override void TakeDamage(int damage)
    {
        // Invincible — no HP reduction, no death
        RegisterHit(damage);
        Debug.Log($"[DummyDPSTracker] Registered hit: {damage} damage.");
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
