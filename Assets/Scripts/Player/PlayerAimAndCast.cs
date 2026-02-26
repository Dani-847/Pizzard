using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimAndCast : MonoBehaviour
{
    public Transform bodyToRotate;
    public ElementsCombiner combiner;
    public CombinationDatabase database;

    private Vector2 lookInput;
    private bool isGamepad;

    public Transform castPoint;

    // Referencias a todos los tipos de escudos/muros activos
    private GameObject activeShield;
    private GameObject activeStaticShield;
    private GameObject activeWall;

    void Update()
    {
        if (bodyToRotate == null)
            return;

        if (isGamepad)
        {
            Vector3 dir = new Vector3(lookInput.x, lookInput.y, 0);
            if (dir.sqrMagnitude > 0.1f)
                RotateTowards(bodyToRotate.position + dir);
        }
        else
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            if (mouseWorldPos != Vector3.zero)
                RotateTowards(mouseWorldPos);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    private void RotateTowards(Vector3 point)
    {
        Vector3 direction = point - bodyToRotate.position;
        direction.z = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bodyToRotate.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void OnLookDirection(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        isGamepad = context.control.device is Gamepad;
    }

    public Vector3 GetCurrentAimDirection()
    {
        if (isGamepad)
        {
            Vector3 dir = new Vector3(lookInput.x, lookInput.y, 0);
            if (dir.sqrMagnitude > 0.001f)
                return dir.normalized;
            if (bodyToRotate != null)
                return bodyToRotate.right;
            return Vector3.right;
        }
        else
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (bodyToRotate == null) return (mousePos - transform.position).normalized;
            Vector3 dir = mousePos - bodyToRotate.position;
            dir.z = 0;
            if (dir.sqrMagnitude > 0.001f)
                return dir.normalized;
            return bodyToRotate.right;
        }
    }

    public float baseFatigueCost = 15f;

    public void OnCastSpell(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var elements = combiner.GetSelectedElements();
        if (elements.Count == 0)
            return;

        // 1) Construir key
        string key = BuildKey(elements);

        // 2) Buscar entry
        CombinationEntry entry = database.GetByKey(key);
        if (entry == null)
        {
            Debug.LogWarning($"❌ No existe combinación para clave: {key}");
            combiner.ClearSelectedElements();
            return;
        }

        // --- FATIGUE CALCULATION ---
        float cost = baseFatigueCost * elements.Count * combiner.spamCostMultiplier;
        if (Pizzard.Core.FatigueSystem.Instance != null)
        {
            if (!Pizzard.Core.FatigueSystem.Instance.CanCast(cost))
            {
                Debug.LogWarning($"[Fatigue] Not enough fatigue! Need {cost}, Have {Pizzard.Core.FatigueSystem.Instance.CurrentFatigue}");
                // Optional: Play a "fizzle" sound or UI shake here
                combiner.ClearSelectedElements();
                return;
            }
            // Consume it
            Pizzard.Core.FatigueSystem.Instance.ConsumeFatigue(cost);
        }

        // Register the cast with multiplier system
        combiner.RegisterCast(key);

        // 3) CASO ESPECIAL: escudo de queso móvil (solo "queso")
        if (key == "queso")
        {
            HandleCheeseShield(entry);
            return;
        }

        // 4) CASO ESPECIAL: escudo estático (queso|queso|piña)
        else if (key == "queso|queso|piña")
        {
            HandleStaticCheeseShield(entry);
            return;
        }

        // 5) CASO ESPECIAL: muro de quemado (queso|queso|pepperoni)
        else if (key == "queso|queso|pepperoni")
        {
            HandleCheesePepperoniWall(entry);
            return;
        }

        // 6) CASO ESPECIAL: teletransporte (piña|pepperoni)
        else if (key == "piña|pepperoni")
        {
            HandleTeleportAttack(entry);
            return;
        }

        // 7) CASO ESPECIAL: catapulta con estela de fuego (pepperoni|piña|pepperoni)
        else if (key == "pepperoni|piña|pepperoni")
        {
            HandlePepperoniPineapplePepperoniAttack(entry);
            return;
        }

        // 8) Ataques normales
        HandleNormalAttack(entry);
    }

    private void HandleCheeseShield(CombinationEntry entry)
    {
        if (entry.attackPrefab == null)
        {
            Debug.LogWarning("⚠️ 'queso' no tiene attackPrefab asignado.");
            combiner.ClearSelectedElements();
            return;
        }

        // Destruir escudo anterior si existe
        if (activeShield != null)
        {
            CheeseShield old = activeShield.GetComponent<CheeseShield>();
            if (old != null)
                old.ForceDestroy();
            else
                Destroy(activeShield);
            activeShield = null;
        }

        // Instanciar escudo móvil
        GameObject shieldGo = Instantiate(entry.attackPrefab, transform.position, Quaternion.identity);

        CheeseShield cs = shieldGo.GetComponent<CheeseShield>();
        if (cs != null)
        {
            cs.SetPlayer(transform, this);
        }
        else
        {
            Debug.LogWarning("⚠️ El prefab del escudo no contiene CheeseShield.");
        }

        activeShield = shieldGo;
        combiner.ClearSelectedElements();
    }

    private void HandleStaticCheeseShield(CombinationEntry entry)
    {
        if (entry.attackPrefab == null)
        {
            Debug.LogWarning("⚠️ 'queso|queso|piña' no tiene attackPrefab asignado.");
            combiner.ClearSelectedElements();
            return;
        }

        // Destruir escudo estático anterior si existe
        if (activeStaticShield != null)
        {
            StaticCheeseShield old = activeStaticShield.GetComponent<StaticCheeseShield>();
            if (old != null)
                old.ForceDestroy();
            else
                Destroy(activeStaticShield);
            activeStaticShield = null;
        }

        // Calcular posición de spawn
        Vector3 aimDir = GetCurrentAimDirection();
        float shieldDistance = 2f;
        Vector3 spawnPosition = transform.position + (aimDir.normalized * shieldDistance);

        // Instanciar escudo estático
        GameObject shieldGo = Instantiate(entry.attackPrefab, spawnPosition, Quaternion.identity);

        StaticCheeseShield staticShield = shieldGo.GetComponent<StaticCheeseShield>();
        if (staticShield != null)
        {
            staticShield.InitializeAtPosition(spawnPosition);
            Debug.Log("🧀 Escudo estático creado en posición: " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("⚠️ El prefab del escudo estático no contiene StaticCheeseShield.");
        }

        activeStaticShield = shieldGo;
        combiner.ClearSelectedElements();
    }

    private void HandleCheesePepperoniWall(CombinationEntry entry)
    {
        if (entry.attackPrefab == null)
        {
            Debug.LogWarning("⚠️ 'queso|queso|pepperoni' no tiene attackPrefab asignado.");
            combiner.ClearSelectedElements();
            return;
        }

        // Destruir muro anterior si existe
        if (activeWall != null)
        {
            CheesePepperoniWall old = activeWall.GetComponent<CheesePepperoniWall>();
            if (old != null)
                old.ForceDestroy();
            else
                Destroy(activeWall);
            activeWall = null;
        }

        // Calcular posición de spawn
        Vector3 aimDir = GetCurrentAimDirection();
        float wallDistance = 2.5f;
        Vector3 spawnPosition = transform.position + (aimDir.normalized * wallDistance);

        // Instanciar muro
        GameObject wallGo = Instantiate(entry.attackPrefab, spawnPosition, Quaternion.identity);

        CheesePepperoniWall wall = wallGo.GetComponent<CheesePepperoniWall>();
        if (wall != null)
        {
            wall.InitializeAtPosition(spawnPosition);
            Debug.Log($"🧀🔥 Muro de queso y pepperoni creado en posición: {spawnPosition}");
            Debug.Log($"🔥 Configuración: {wall.burnInitialStacks} stacks de {wall.burnStatusEffect} por {wall.burnEffectDuration}s");
        }
        else
        {
            Debug.LogWarning("⚠️ El prefab del muro no contiene CheesePepperoniWall.");
        }

        activeWall = wallGo;
        combiner.ClearSelectedElements();
    }

    private void HandleTeleportAttack(CombinationEntry entry)
    {
        if (entry.attackPrefab == null)
        {
            Debug.LogWarning("⚠️ 'piña|pepperoni' no tiene attackPrefab asignado.");
            combiner.ClearSelectedElements();
            return;
        }

        GameObject attackObj = Instantiate(entry.attackPrefab, transform.position, Quaternion.identity);
        
        PineapplePepperoniAttack teleportAttack = attackObj.GetComponent<PineapplePepperoniAttack>();
        if (teleportAttack != null)
        {
            teleportAttack.Initialize(transform, this);
            Debug.Log("🌀 Ataque de teletransporte inicializado");
        }
        else
        {
            Debug.LogError("❌ El prefab de piña|pepperoni no tiene el componente PineapplePepperoniAttack");
        }

        combiner.ClearSelectedElements();
    }

    private void HandlePepperoniPineapplePepperoniAttack(CombinationEntry entry)
    {
        if (entry.attackPrefab == null)
        {
            Debug.LogWarning("⚠️ 'pepperoni|piña|pepperoni' no tiene attackPrefab asignado.");
            combiner.ClearSelectedElements();
            return;
        }

        Vector3 target = GetMouseWorldPosition();
        Vector3 direction = (target - castPoint.position).normalized;

        // Distancia mínima
        float minDistance = 2f;
        float distance = Vector3.Distance(castPoint.position, target);
        if (distance < minDistance)
        {
            target = castPoint.position + direction * minDistance;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        // Instanciar
        GameObject projectile = Instantiate(entry.attackPrefab, castPoint.position, rot);
        
        PepperoniPineapplePepperoniAttack attack = projectile.GetComponent<PepperoniPineapplePepperoniAttack>();
        if (attack != null)
        {
            attack.Initialize(direction);
            Debug.Log($"🍕🍍🍕 Catapulta lanzada!");
        }
        else
        {
            Debug.LogError("❌ El prefab no contiene PepperoniPineapplePepperoniAttack");
        }

        // ¡IMPORTANTE! Limpiar elementos
        combiner.ClearSelectedElements();
    }

    private void HandleNormalAttack(CombinationEntry entry)
    {
        if (entry.attackPrefab != null)
        {
            Vector3 target = GetMouseWorldPosition();
            Vector3 direction = (target - castPoint.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject projectile = Instantiate(entry.attackPrefab, castPoint.position, rot);
            
            CharacterProjectile charProjectile = projectile.GetComponent<CharacterProjectile>();
            if (charProjectile != null)
            {
                charProjectile.Initialize(direction);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ La combinación {entry.combinationName} no tiene attackPrefab asignado.");
        }

        combiner.ClearSelectedElements();
    }

    private string BuildKey(List<ElementType> elements)
    {
        List<string> names = new List<string>();
        foreach (var e in elements)
            names.Add(e.ToString().ToLower());

        return string.Join("|", names);
    }
}