using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    private Dictionary<StatusType, ActiveEffect> activeEffects = new Dictionary<StatusType, ActiveEffect>();
    
    [System.Serializable]
    private class ActiveEffect
    {
        public StatusType type;
        public int stacks;
        public float duration;
        public GameObject source;
        public Coroutine coroutine;
    }

    public void ApplyEffect(StatusType type, float duration, GameObject source, int initialStacks = 1)
    {
        if (activeEffects.ContainsKey(type))
        {
            // Si el efecto ya está activo, aumentar las cargas
            ActiveEffect existingEffect = activeEffects[type];
            existingEffect.stacks += initialStacks;
            existingEffect.duration = duration; // Reiniciar duración
            existingEffect.source = source;
            
            Debug.Log($"Efecto {type} actualizado: {existingEffect.stacks} cargas, {duration} segundos");
        }
        else
        {
            // Crear nuevo efecto
            ActiveEffect newEffect = new ActiveEffect
            {
                type = type,
                stacks = initialStacks,
                duration = duration,
                source = source
            };
            
            newEffect.coroutine = StartCoroutine(HandleEffect(newEffect));
            activeEffects[type] = newEffect;
            
            Debug.Log($"Nuevo efecto aplicado: {type} con {initialStacks} cargas, {duration} segundos");
        }
    }

    private IEnumerator HandleEffect(ActiveEffect effect)
    {
        Debug.Log($"Iniciando efecto: {effect.type} con {effect.stacks} cargas");
        
        float tickInterval = 1f; // Daño cada segundo
        float elapsed = 0f;
        
        while (elapsed < effect.duration && effect.stacks > 0)
        {
            // Aplicar daño basado en las cargas actuales
            ApplyEffectDamage(effect.type, effect.stacks, effect.source);
            
            // Esperar hasta el próximo tick
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            
            // Reducir cargas a la mitad (redondeando hacia abajo) después de cada ciclo
            if (elapsed < effect.duration && effect.stacks > 0)
            {
                int newStacks = Mathf.FloorToInt(effect.stacks / 2f);
                Debug.Log($"Reduciendo cargas de {effect.stacks} a {newStacks}");
                effect.stacks = newStacks;
                
                // Si las cargas llegan a 0, terminar el efecto
                if (effect.stacks <= 0)
                {
                    Debug.Log($"Efecto {effect.type} terminado: sin cargas");
                    break;
                }
            }
        }
        
        // Limpiar efecto
        activeEffects.Remove(effect.type);
        Debug.Log($"Efecto {effect.type} finalizado después de {elapsed} segundos");
    }

    private void ApplyEffectDamage(StatusType type, int stacks, GameObject source)
    {
        if (type == StatusType.picante)
        {
            // Calcular daño basado en las cargas
            float damage = stacks * 2f; // 2 de daño por carga
            
            // Aplicar daño al objetivo según su tipo
            if (gameObject.CompareTag("Boss"))
            {
                PblobController boss = GetComponent<PblobController>();
                if (boss != null && boss.IsVulnerable())
                {
                    boss.TakeDamage(damage);
                    Debug.Log($"{gameObject.name} recibe {damage} de daño por picante ({stacks} cargas)");
                }
            }
            else if (gameObject.CompareTag("Player"))
            {
                PlayerHPController player = GetComponent<PlayerHPController>();
                if (player != null)
                {
                    player.RecibirDaño((int)damage);
                    Debug.Log($"{gameObject.name} recibe {damage} de daño por picante ({stacks} cargas)");
                }
            }
        }
        // Puedes añadir más tipos de efectos aquí
    }

    public bool HasActiveEffect(StatusType type)
    {
        return activeEffects.ContainsKey(type);
    }

    public int GetEffectStacks(StatusType type)
    {
        if (activeEffects.ContainsKey(type))
        {
            return activeEffects[type].stacks;
        }
        return 0;
    }

    public void RemoveEffect(StatusType type)
    {
        if (activeEffects.ContainsKey(type))
        {
            StopCoroutine(activeEffects[type].coroutine);
            activeEffects.Remove(type);
            Debug.Log($"Efecto {type} removido manualmente");
        }
    }
}