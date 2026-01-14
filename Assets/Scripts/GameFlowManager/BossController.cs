using System;
using UnityEngine;

public abstract class BossController : MonoBehaviour
{
    public event Action OnBossDefeated;

    public virtual void StartBossFight()
    {
        enabled = true;
    }

    // Llama a esto cuando el boss muere
    protected void BossDefeated()
    {
        OnBossDefeated?.Invoke();
    }
}