using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// Controlador del jugador que gestiona el movimiento y la entrada.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Collision Settings")]
    public LayerMask wallLayerMask = 1;
    public float collisionCheckDistance = 0.1f;

    private CharacterController controller;
    private Vector3 moveInput;
    private Tilemap wallTilemap;
    private bool inputEnabled = true;

    public MenuUI menuUI;
    public OptionsUI optionsUI;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Buscar el tilemap de paredes automáticamente
        wallTilemap = FindObjectOfType<Tilemap>();
        if (wallTilemap == null)
        {
            Debug.LogError("❌ No se encontró Tilemap en la escena");
        }

        // Configurar la layer mask para paredes
        wallLayerMask = LayerMask.GetMask("Wall");
    }

    void Update()
    {
        // Verificar si el input está habilitado
        if (!inputEnabled)
        {
            return;
        }

        // Verificar si hay menús abiertos
        if ((menuUI != null && menuUI.gameObject.activeSelf) || 
            (optionsUI != null && optionsUI.gameObject.activeSelf))
        {
            return;
        }

        Vector3 inputDir = new Vector3(moveInput.x, moveInput.y, 0);

        if (inputDir.sqrMagnitude > 0.01f)
        {
            // Verificar colisiones antes de mover
            Vector3 moveDirection = inputDir * speed * Time.deltaTime;
            
            if (!WillCollideWithWall(moveDirection))
            {
                controller.Move(moveDirection);
            }
            else
            {
                // Intentar mover solo en X o solo en Y
                Vector3 moveX = new Vector3(moveDirection.x, 0, 0);
                Vector3 moveY = new Vector3(0, moveDirection.y, 0);
                
                if (!WillCollideWithWall(moveX))
                {
                    controller.Move(moveX);
                }
                if (!WillCollideWithWall(moveY))
                {
                    controller.Move(moveY);
                }
            }
        }
    }

    private bool WillCollideWithWall(Vector3 movement)
    {
        if (wallTilemap == null) return false;

        // Calcular posición futura
        Vector3 futurePosition = transform.position + movement;
        
        // Convertir a posición de celda en el tilemap
        Vector3Int cellPosition = wallTilemap.WorldToCell(futurePosition);
        
        // Verificar si hay un tile de pared en esa posición
        if (wallTilemap.HasTile(cellPosition))
        {
            return true;
        }

        // Verificación adicional con raycast (opcional)
        float checkDistance = controller.radius + collisionCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, movement.normalized, checkDistance, wallLayerMask);
        
        return hit.collider != null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (menuUI != null && menuUI.gameObject.activeSelf) return;

        if (UIManager.Instance != null && UIManager.Instance.optionsUI != null)
        {
            if (UIManager.Instance.optionsUI.gameObject.activeSelf)
                UIManager.Instance.CloseOptions();
            else
                UIManager.Instance.OpenOptions(UIContext.BossFight);
        }
    }

    /// <summary>
    /// Habilita o deshabilita el input del jugador.
    /// Usado por GameFlowManager para controlar cuándo el jugador puede moverse.
    /// </summary>
    /// <param name="enabled">True para habilitar, false para deshabilitar.</param>
    public void EnableInput(bool enabled)
    {
        inputEnabled = enabled;
        
        // Resetear el input cuando se deshabilita
        if (!enabled)
        {
            moveInput = Vector3.zero;
        }
    }

    // Para debugging visual
    void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, controller.radius);
            
            if (Application.isPlaying && moveInput.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, moveInput.normalized * (controller.radius + collisionCheckDistance));
            }
        }
    }
}