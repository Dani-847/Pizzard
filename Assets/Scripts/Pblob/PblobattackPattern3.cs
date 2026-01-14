using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PblobAttackPattern3 : PblobAttackPattern
{
    [Header("Pattern 3 - Placeholder")]
    public bool isEnabled = false;
    
    private bool isPatternActive = false;

    public override void StartPattern()
    {
        if (!isEnabled) return;
        
        isPatternActive = true;
        DebugPattern("Patrón 3 iniciado (placeholder)");
        // TODO: Implementar lógica del patrón 3 en el futuro
    }
    
    public override void StopPattern()
    {
        if (!isPatternActive) return;
        
        isPatternActive = false;
        DebugPattern("Patrón 3 detenido");
    }
    
    [ContextMenu("TEST - Activar Patrón 3")]
    public void TestActivatePattern()
    {
        if (isEnabled)
            StartPattern();
    }
}