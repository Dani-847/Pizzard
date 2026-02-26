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
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                Debug.LogWarning("[PlayerController] Added missing Rigidbody2D to player at runtime.");
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
            // Gather standard movement
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(moveX, moveY).normalized;

            // Gather dash input (e.g. Spacebar)
            if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownRemaining <= 0f && movementInput != Vector2.zero)
            {
                StartDash();
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