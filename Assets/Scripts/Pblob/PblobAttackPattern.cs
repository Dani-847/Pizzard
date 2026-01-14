using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PblobAttackPattern : MonoBehaviour
{
    protected PblobController bossController;

    protected virtual void Start()
    {
        bossController = GetComponentInParent<PblobController>();
    }

    public abstract void StartPattern();
    public abstract void StopPattern();

    protected void DebugPattern(string message)
    {
        Debug.Log($"[{GetType().Name}] {message}");
    }
    
    protected bool IsBossVulnerable()
    {
        return bossController != null && bossController.IsVulnerable();
    }
}