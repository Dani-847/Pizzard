using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PblobAttackPattern2 : PblobAttackPattern
{
    [Header("Pattern 2 - Movement Circles")]
    public bool isEnabled = true;
    
    [Header("Circle Settings")]
    public GameObject movementCirclePrefab;
    public int numberOfCircles = 3;
    public float circleMovementSpeed = 2f;
    
    private bool isPatternActive = false;
    private List<GameObject> activeCircles = new List<GameObject>();
    private Coroutine circleCoroutine;

    public override void StartPattern()
    {
        if (!isEnabled || isPatternActive) return;
        
        isPatternActive = true;
        DebugPattern("🎵 FASE 2 INICIADA - Patrón de círculos de movimiento activado");
        
        // Iniciar lógica de círculos
        circleCoroutine = StartCoroutine(CirclePatternRoutine());
        
        Debug.Log("🔵 Círculos de movimiento: Sigue el círculo que aparece en el beat correcto!");
        Debug.Log("🎯 Pista: Los círculos falsos aparecen en momentos aleatorios");
    }
    
    public override void StopPattern()
    {
        if (!isPatternActive) return;
        
        isPatternActive = false;
        
        // Detener corrutina si está activa
        if (circleCoroutine != null)
        {
            StopCoroutine(circleCoroutine);
            circleCoroutine = null;
        }
        
        // Limpiar círculos
        foreach (var circle in activeCircles)
        {
            if (circle != null)
                Destroy(circle);
        }
        activeCircles.Clear();
        
        DebugPattern("Fase 2 detenida - Círculos eliminados");
    }
    
    // Corrutina para el patrón de círculos
    private IEnumerator CirclePatternRoutine()
    {
        Debug.Log("🔄 Iniciando rutina de círculos...");
        
        while (isPatternActive)
        {
            // Aquí irá la lógica de spawn de círculos
            // Por ahora solo mostramos mensajes de debug
            yield return new WaitForSeconds(5f);
            
            // Verificación redundante removida - el while ya verifica isPatternActive
            Debug.Log("⭕ Círculo debería aparecer aquí (sincronizado con beat)");
        }
    }
    
    [ContextMenu("TEST - Activar Patrón 2")]
    public void TestActivatePattern()
    {
        if (isEnabled)
            StartPattern();
    }
    
    [ContextMenu("TEST - Detener Patrón 2")]
    public void TestStopPattern()
    {
        StopPattern();
    }
}