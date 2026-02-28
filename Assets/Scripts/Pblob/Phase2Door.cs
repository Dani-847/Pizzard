using UnityEngine;

public class Phase2Door : MonoBehaviour
{
    [Header("Door References")]
    public Animator doorAnimator;
    public string openAnimationName = "Open";
    public string closeAnimationName = "Close";
    
    [Header("Door Settings")]
    public bool isLocked = true;
    public bool isOpen = false;
    
    private PblobController bossController;
    
    void Start()
    {
        // Buscar el BossController en la escena
        bossController = FindObjectOfType<PblobController>();
        
        // ✅ Suscribirse al evento de transición (con verificación de null)
        if (bossController != null && bossController.OnPhaseTransition != null)
        {
            bossController.OnPhaseTransition.AddListener(OnPhase2Unlocked);
            Debug.Log("🚪 Phase2Door: Suscrito a evento OnPhaseTransition");
        }
        else
        {
            Debug.LogWarning("⚠️ Phase2Door: PblobController no encontrado o evento no disponible");
        }
        
        // Inicialmente la puerta está cerrada y bloqueada
        if (doorAnimator != null && isLocked)
        {
            doorAnimator.Play(closeAnimationName);
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse del evento al destruirse para evitar memory leaks
        if (bossController != null && bossController.OnPhaseTransition != null)
        {
            bossController.OnPhaseTransition.RemoveListener(OnPhase2Unlocked);
        }
    }
    
    // ✅ Cuando se desbloquea la fase 2
    private void OnPhase2Unlocked()
    {
        isLocked = false;
        OpenDoor();
        Debug.Log("🚪 Puerta desbloqueada - Lista para fase 2");
    }
    
    // ✅ Abrir puerta
    public void OpenDoor()
    {
        if (isLocked) return;
        
        isOpen = true;
        if (doorAnimator != null)
        {
            doorAnimator.Play(openAnimationName);
        }
        Debug.Log("🚪 Puerta abierta");
    }
    
    // ✅ Cerrar puerta
    public void CloseDoor()
    {
        isOpen = false;
        if (doorAnimator != null)
        {
            doorAnimator.Play(closeAnimationName);
        }
        Debug.Log("🚪 Puerta cerrada");
    }
    
    // ✅ Cuando el jugador interactúa con la puerta
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen && !isLocked)
        {
            Debug.Log("🎮 Jugador entrando a Fase 2...");
            StartPhase2();
            CloseDoor();
        }
    }
    
    // ✅ Iniciar fase 2
    private void StartPhase2()
    {
        // Phase 2 now starts automatically via PblobController's MoveToCenterAndTransition coroutine,
        // so we don't need to force the state change here anymore.
    }
    
    // DEBUG
    [ContextMenu("DEBUG - Abrir Puerta")]
    public void DebugOpenDoor()
    {
        isLocked = false;
        OpenDoor();
    }
    
    [ContextMenu("DEBUG - Cerrar Puerta")]
    public void DebugCloseDoor()
    {
        CloseDoor();
    }
}