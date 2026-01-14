using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTransitionManager : MonoBehaviour
{
    [Header("Room Settings")]
    public string nextRoomName;
    public GameObject hairWall; // Pared de pelo que bloquea el paso
    public Transform bossNextPosition; // Posición del boss en la siguiente habitación
    
    [Header("Transition Effects")]
    public Animator transitionAnimator;
    public string transitionTrigger = "FadeOut";
    public float transitionDuration = 2f;

    private PblobController bossController;

    private void Start()
    {
        bossController = FindObjectOfType<PblobController>();
        
        // Suscribirse a eventos del boss
        if (bossController != null)
        {
            bossController.OnPhaseTransition.AddListener(StartRoomTransition);
        }
    }

    // ✅ Iniciar transición de habitación
    public void StartRoomTransition()
    {
        Debug.Log("🚀 Iniciando transición de habitación...");
        StartCoroutine(RoomTransitionCoroutine());
    }

    private IEnumerator RoomTransitionCoroutine()
    {
        // 1. Animación de transición
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(transitionTrigger);
        }

        Debug.Log("🎬 Reproduciendo animación de transición...");
        yield return new WaitForSeconds(transitionDuration * 0.5f);

        // 2. Mover boss a nueva posición (simulación)
        if (bossController != null && bossNextPosition != null)
        {
            bossController.transform.position = bossNextPosition.position;
            Debug.Log("👑 Boss movido a nueva posición");
        }

        // 3. Activar pared de pelo (para simular cambio de room)
        if (hairWall != null)
        {
            hairWall.SetActive(true);
            Debug.Log("🧱 Pared de pelo activada");
        }

        yield return new WaitForSeconds(transitionDuration * 0.5f);

        // 4. Iniciar siguiente fase del boss
        if (bossController != null)
        {
            // Aquí se iniciaría el patrón 2 en la siguiente fase
            Debug.Log("✅ Transición completada - Listo para Patrón 2");
        }
    }

    // ✅ Método para cuando el jugador rompe la pared de pelo
    public void OnHairWallDestroyed()
    {
        if (hairWall != null)
        {
            hairWall.SetActive(false);
            Debug.Log("💥 Pared de pelo destruida - Pasando a siguiente habitación");
            
            // Iniciar batalla del boss en la nueva habitación
            if (bossController != null)
            {
                bossController.StartBossBattle();
            }
        }
    }

    // DEBUG
    [ContextMenu("DEBUG - Forzar Transición")]
    public void DebugForceTransition()
    {
        StartRoomTransition();
    }
}