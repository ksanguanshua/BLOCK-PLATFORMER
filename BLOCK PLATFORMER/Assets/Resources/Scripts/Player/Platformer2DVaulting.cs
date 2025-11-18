using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Platformer2DMovementBase))]
public class Platformer2DVaulting : MonoBehaviour
{
    #region Vaulting Settings
    [Header("Vaulting Settings")]
    [SerializeField] public bool RESET_XVEL = true; // does vaulting reset xvel
    [SerializeField] private float ledgeCheckDistance = 1f; // Distance to detect a ledge
    [SerializeField] private float ledgeTopOffset = 0.5f;   // Offset to place the player on top of the ledge
    [SerializeField] private LayerMask lm_ledgeLayer;       // Layer for ledges
    [SerializeField] private float vaultForce = 20f;        // Force applied during the vault
    [SerializeField] private float upwardForceMultiplier = 0.7f; // Multiplier for the upward component of the vault force
    [SerializeField] private float vaultCooldown = 0.2f;    // Cooldown before another vault can occur

    [Header("Input Options")]
    [SerializeField] public bool TRIGGER_BY_JUMP_KEY = true; // Spacebar
    [SerializeField] public bool TRIGGER_BY_DASH = true;    // Dash
    [SerializeField] public bool TRIGGER_AUTO_BY_RUN = true;    // Automaticly vaults when running
    [SerializeField] public KeyCode VAULT_KEY = KeyCode.None; // Custom key for vaulting

    [Header("Collider Adjustment")]
    [SerializeField] private Vector2 colliderShrinkFactor = new Vector2(0.5f, 0.7f); // Scale factor for collider resizing
    [SerializeField] private Vector2 colliderOffsetDuringVault = new Vector2(0f, -0.2f); // Offset during vault

    [Header("Debug")]
    [SerializeField] private bool debugLedgeDetection = false;
    #endregion

    #region Components
    private Rigidbody2D c_rb;
    private Collider2D c_collider;
    private Platformer2DMovementBase c_movementBase;

    private Vector2 originalColliderSize;   // Original size of the collider
    private Vector2 originalColliderOffset; // Original offset of the collider
    #endregion

    #region State
    private bool ch_canVault = true; // Determines if the player can vault
    #endregion

    #region Initialization
    private void Start()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_movementBase = GetComponent<Platformer2DMovementBase>();
        c_collider = GetComponent<Collider2D>();

        // Cache the original collider size and offset
        CacheOriginalColliderSettings();
    }

    private void CacheOriginalColliderSettings()
    {
        switch (c_collider)
        {
            case BoxCollider2D boxCollider:
                originalColliderSize = boxCollider.size;
                originalColliderOffset = boxCollider.offset;
                break;
            case CapsuleCollider2D capsuleCollider:
                originalColliderSize = capsuleCollider.size;
                originalColliderOffset = capsuleCollider.offset;
                break;
            case CircleCollider2D circleCollider:
                originalColliderSize = Vector2.one * circleCollider.radius;
                originalColliderOffset = circleCollider.offset;
                break;
        }
    }
    #endregion

    #region Vaulting Logic
    /// <summary>
    /// Checks if the player is near a ledge and can vault.
    /// </summary>
    /// <param name="in_moveInput">The directional input indicating movement toward the ledge (-1 or 1).</param>
    /// <returns>True if vaulting is possible, otherwise false.</returns>
    public bool CheckVaultCondition(int in_moveInput)
    {
        if (!ch_canVault) return false;

        // Detect ledges based on movement input direction
        Vector2 direction = new Vector2(in_moveInput, 0);
        RaycastHit2D lowerHit = Physics2D.Raycast(transform.position, direction, ledgeCheckDistance, lm_ledgeLayer);
        RaycastHit2D upperHit = Physics2D.Raycast(transform.position + Vector3.up * 0.9f, direction, ledgeCheckDistance, lm_ledgeLayer);

        bool ledgeDetected = lowerHit.collider != null && upperHit.collider == null;

        if (debugLedgeDetection)
        {
            Debug.DrawRay(transform.position, direction * ledgeCheckDistance, lowerHit.collider ? Color.green : Color.red);
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, direction * ledgeCheckDistance, upperHit.collider ? Color.green : Color.red);
        }

        return ledgeDetected;
    }

    /// <summary>
    /// Initiates the vaulting process if the player is near a valid ledge.
    /// </summary>
    /// <param name="ledgeSide">The direction of the ledge (-1 for left, 1 for right).</param>
    public void Vault(int ledgeSide)
    {
        if (!ch_canVault) return;

        StartCoroutine(CoVault(ledgeSide));
    }

    private IEnumerator CoVault(int ledgeSide)
    {
        ch_canVault = false;

        // Stop any existing vertical velocity
        c_rb.linearVelocity = new Vector2(c_rb.linearVelocity.x * (RESET_XVEL ? 0f : 1f), 0);

        // Adjust collider for the vault
        AdjustCollider(true);

        // Calculate force vector
        Vector2 forceDirection = new Vector2(ledgeSide, upwardForceMultiplier).normalized;
        Vector2 vaultForceVector = forceDirection * vaultForce;

        // Apply vault force
        c_rb.AddForce(vaultForceVector, ForceMode2D.Impulse);

        // Wait for cooldown to finish
        yield return new WaitForSeconds(vaultCooldown);

        // Reset collider to original settings
        AdjustCollider(false);

        ch_canVault = true;
    }

    private void AdjustCollider(bool isVaulting)
    {
        switch (c_collider)
        {
            case BoxCollider2D boxCollider:
                boxCollider.size = isVaulting
                    ? Vector2.Scale(originalColliderSize, colliderShrinkFactor)
                    : originalColliderSize;
                boxCollider.offset = isVaulting
                    ? originalColliderOffset + colliderOffsetDuringVault
                    : originalColliderOffset;
                break;

            case CapsuleCollider2D capsuleCollider:
                capsuleCollider.size = isVaulting
                    ? Vector2.Scale(originalColliderSize, colliderShrinkFactor)
                    : originalColliderSize;
                capsuleCollider.offset = isVaulting
                    ? originalColliderOffset + colliderOffsetDuringVault
                    : originalColliderOffset;
                break;

            case CircleCollider2D circleCollider:
                circleCollider.radius = isVaulting
                    ? originalColliderSize.x * colliderShrinkFactor.x
                    : originalColliderSize.x;
                circleCollider.offset = isVaulting
                    ? originalColliderOffset + colliderOffsetDuringVault
                    : originalColliderOffset;
                break;
        }
    }
    #endregion
}
