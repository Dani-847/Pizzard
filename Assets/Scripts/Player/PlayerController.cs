using UnityEngine;

namespace Pizzard.Player
{
    /// <summary>
    /// Handles 2D movement and dash mechanics for the player using Rigidbody2D.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeedMultiplier = 3f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;

        private Rigidbody2D rb;
        private Vector2 movementInput;
        
        private bool isDashing = false;
        private float dashTimeRemaining;
        private float dashCooldownRemaining;

        private void Awake()
        {
            // Aggressively remove 3D CharacterController that fights 2D physics and freezes movement
            var charController = GetComponent<UnityEngine.CharacterController>();
            if (charController != null)
            {
                DestroyImmediate(charController);
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                try 
                {
                    rb = gameObject.AddComponent<Rigidbody2D>();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PlayerController] Suppressed Unity internal exception when adding Rigidbody2D: {e.Message}");
                }
            }
            // Ensure no gravity affects the top-down player
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }

        private void Update()
        {
            HandleInput();
            HandleDashTimers();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void HandleInput()
        {
            // Use New Input System Keyboard fallback directly since older Input.GetAxis is disabled.
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                float moveX = 0f;
                float moveY = 0f;
                if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed) moveY += 1f;
                if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed) moveY -= 1f;
                if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed) moveX += 1f;
                if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed) moveX -= 1f;
                
                movementInput = new Vector2(moveX, moveY).normalized;

                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame && !isDashing && dashCooldownRemaining <= 0f && movementInput != Vector2.zero)
                {
                    StartDash();
                }
            }
        }

        // Support for Unity Events from PlayerInput component
        public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
            {
                movementInput = context.ReadValue<Vector2>().normalized;
            }
        }

        private void HandleDashTimers()
        {
            if (isDashing)
            {
                dashTimeRemaining -= Time.deltaTime;
                if (dashTimeRemaining <= 0)
                {
                    isDashing = false;
                    dashCooldownRemaining = dashCooldown;
                }
            }
            else if (dashCooldownRemaining > 0)
            {
                dashCooldownRemaining -= Time.deltaTime;
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimeRemaining = dashDuration;
        }

        private void ApplyMovement()
        {
            if (rb == null) return;
            
            float currentSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
            rb.velocity = movementInput * currentSpeed;
        }
    }
}