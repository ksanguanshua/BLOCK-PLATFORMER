using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using SaintsField;

public class MovementPlatformer2D : Movement
{
    [System.Serializable]
    public struct AddOns
    {
        [SerializeField][ReadOnly] public Platformer2DRotation rotation;
        [SerializeField][ReadOnly] public Platformer2DLedge ledge;
        [SerializeField][ReadOnly] public Platformer2DKnockback knockback;
        [SerializeField][ReadOnly] public Platformer2DWallSlideAndJump wall;
        [SerializeField][ReadOnly] public Platformer2DVaulting vault;
        [SerializeField][ReadOnly] public Platformer2DRunning run;
    }

    [SerializeField][SaintsRow][RichLabel("Add-Ons")] public AddOns AO;

    // METHODS START!

    #region Basic Functions
    //Get all components 
    override protected void ComponentGrab()
    {
        base.ComponentGrab();

        //Add-Ons
        TryGetComponent<Platformer2DRotation>(out AO.rotation);
        TryGetComponent<Platformer2DLedge>(out AO.ledge);
        TryGetComponent<Platformer2DKnockback>(out AO.knockback);
        TryGetComponent<Platformer2DWallSlideAndJump>(out AO.wall);
        TryGetComponent<Platformer2DVaulting>(out AO.vault);
        TryGetComponent<Platformer2DRunning>(out AO.run);
    }
    //Finds the X and Y component of the given angle
    Vector2 CalculateAngleComponents(float angle)
    {
        float t_y = Mathf.Sin(Mathf.Deg2Rad * angle);
        float t_x = Mathf.Cos(Mathf.Deg2Rad * angle);
        print("Angle Components Found: " + new Vector2(t_x, t_y));
        return new Vector2(t_x, t_y);
    }
    //Launch the object
    public void Launch(Vector2 direction, float force, bool resetForce)
    {
        if (resetForce)
            R.rb.linearVelocity = new Vector2(R.rb.linearVelocity.x, 0);

        R.rb.AddForce(direction * force, ForceMode2D.Impulse);
    }
    public void Launch(Vector2 direction, float force, bool resetForce, float knockbackThreshold)
    {
        Launch(direction, force, resetForce);

        if (force > knockbackThreshold)
        {
            S.state = State.Knockback;
            //knockbackTime = force / 2;
            S.decceleration = 0;
            S.acceleration = 1;
        }
    }
    #endregion

    #region Input Functions
    public override void MoveInput(Vector2 value)
    {
        S.movementInput = value;
    }
    public override void JumpInput(float value)
    {
        float t_jumpPressed = value;
        if (M.JUMP_CUE)
        {
            if (S.jumpQueue < M.jumpQueueMax && t_jumpPressed == 1)
            {
                S.jumpQueue += 1;
                StartCoroutine(JumpCueEmptyCache());
            }
            else if (t_jumpPressed == 0)
            {
                if (R.rb.linearVelocity.y < -M.quickFallForce && S.jumpQueue > 0)
                {
                    S.jumpQueue -= 1;
                }
                else
                {
                    JumpProcessing(t_jumpPressed);
                }
            }
        }
        else
        {
            JumpProcessing(t_jumpPressed);
        }
    }
    #endregion

    #region Jumping Functions
    void JumpProcessing(float p_jumpPressed)
    {
        switch (p_jumpPressed)
        {
            //when the jump button is released [mm_quickFallOption]
            case 0:
                if (S.canQuickFall && M.QUICK_FALL_OPTION && M.quickFallForce < R.rb.linearVelocityY)
                {
                    //R.rb.linearVelocityY = -1 * M.quickFallForce;
                    R.rb.linearVelocityY += -1 * M.quickFallForce;
                    S.canQuickFall = false;
                }
                break;
            //when the jump button is pressed [everything that relates]
            case 1:
                UpdateStates("jump"); // Updates states to be able to determine what the player wants to do

                if (false)
                {

                }
                // Vaulting
                else if (AO.vault != null && AO.vault.TRIGGER_BY_JUMP_KEY && AO.vault.CheckVaultCondition((int)S.movementInput.x) && S.isGrounded)
                {
                    print("Vault");
                    AO.vault.Vault((int)S.movementInput.x);
                }
                // Ledge Jump
                else if (S.state == State.Ledging)
                {
                    print("LedgeJump");
                    Launch(Vector2.up, 8, true);
                    S.state = State.WallJumping;
                }
                //Wall Jumping
                else if (AO.wall != null && AO.wall.CanWallJump() && !S.isGrounded)
                {
                    print("WallJump");
                    //c_sfxManager.PlaySoundOneShot("jump");
                    S.amountOfJumps = M.maxAmountOfJumpsInAir;
                    AO.wall.HandleWallJump(p_jumpPressed);
                    S.canQuickFall = true;
                }
                //Double Jumping
                else if (S.amountOfJumps > 0 && !S.isGrounded)
                {
                    print("Double Jump");
                    S.amountOfJumps--;
                    S.canQuickFall = true;
                    BasicJump();
                    S.coyoteTimeActive = -1;
                }
                // Basic Jump
                else if (S.isGrounded || S.coyoteTimeActive == 1)
                {
                    //print("Basic Jump");
                    S.canQuickFall = true;
                    BasicJump();
                    S.coyoteTimeActive = -1;

                    AudioManager.instance.PlayJump();
                }

                else if (M.JUMP_CUE)
                {
                    S.jumpQueue += 1;
                }

                if (M.JUMP_CUE)
                {
                    StopCoroutine(JumpCueEmptyCache());
                }

                break;
        }
    }
    //Basic Hop [with quickfall]
    void BasicJump()
    {
        //c_sfxManager.PlaySoundOneShot("jump");
        R.particleManager.PlayParticle("PSFloorDustJump");
        R.particleManager.PlayParticle("PSDustDrag");
        Launch(M.baseGroundNormal, M.jumpForce, true);
    }
    #endregion

    #region Main Functions
    private void Start()
    {
        if (SaveData.instance != null)
        {
            M = SaveData.instance.playerStats.movementModifiers;
        }

        ComponentGrab();
        ModtoState();
        S.state = State.Base;
        S.facing = Vector2.right;
        S.facing = new Vector2(1, 0);
    }
    private void Update()
    {
        UpdateStates("general");
    }
    private void FixedUpdate()
    {
        //turns input into movement
        UpdateMovement();
    }
    #endregion

    #region Movement Methods
    void UpdateMovement() //Based on FixedUpdate
    {
        switch (S.state)
        {
            case State.Base:
                R.rb.gravityScale = 1;
                LRMovement(); // Left and right movement
                if (!S.isGrounded && R.rb.linearVelocity.y < M.m_peakJumpVelocity && M.QUICK_FALLEN_FORCE) // Force quickfall
                {
                    R.rb.linearVelocity += Vector2.down * M.quickFallForceForced;
                }
                if (R.rb.linearVelocity.y < 0 && M.GRAV_MOD)
                {
                    R.rb.linearVelocityY -= Time.deltaTime * M.gravityFallingAccel;
                }
                break;

            case State.WallJumping:
                R.rb.gravityScale = 1;
                if (R.rb.linearVelocityY < M.m_peakJumpVelocity)
                {
                    print("Wall Jump to Active");
                    S.state = State.Base;
                }
                break;

            case State.Ledging:
                R.rb.gravityScale = 0;
                R.rb.linearVelocity = Vector2.zero;
                UpdateStates("wallsliding");

                break;

            case State.Dashing:
                if (AO.vault != null && AO.vault.TRIGGER_BY_DASH && S.movementInput.x != 0 && AO.vault.CheckVaultCondition((int)S.movementInput.x))
                { // Dash Vaulting
                    print("Dash Vault");
                    AO.vault.Vault((int)S.movementInput.x);
                }
                break;

            case State.WallSliding:
                LRMovement(); // Left and right movement
                // Slow the player's descent
                R.rb.linearVelocity = new Vector2(R.rb.linearVelocity.x, -AO.wall.m_slideSpeed);
                UpdateStates("wallsliding");
                break;

            case State.Inactive:

                break;
        }
        //Fallclamp Applied
        R.rb.linearVelocity = new Vector2(R.rb.linearVelocity.x, Mathf.Clamp(R.rb.linearVelocity.y, M.fallClamp, 20));
    }

    void LRMovement()
    {
        Vector2 tangent = new Vector2(M.baseGroundNormal.y, -M.baseGroundNormal.x).normalized;
        float targetSpeed = S.movementInput.x * S.movementSpeed;
        float currentSpeed = Vector2.Dot(R.rb.linearVelocity, tangent);
        //lastSpeed = currentSpeed;
        float speedDif = targetSpeed - currentSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? S.acceleration : S.decceleration;
        float movement = speedDif * accelRate;
        R.rb.AddForce(movement * tangent, ForceMode2D.Force);
    }
    #endregion

    #region Update States + Check Functions
    //Called in Update -> goes through checks to update ss [groundcheck, wallcheck]
    public void UpdateStates(string type)
    {
        switch (type)
        {
            //Called everyframe
            case "general":
                CanTurnChecker(); //changes if the player can turn or not
                GroundChecker(); //checks for ground
                //QOL Features------------------------
                if (M.COYOTE_TIME)
                    CoyoteTime();
                if (M.JUMP_CUE)
                    JumpCueCheck();
                //Add-on update-----------------------
                if (AO.run != null) // Run
                    AO.run.CheckRun((int)S.movementInput.x);

                if (AO.wall != null) // Wall
                    AO.wall.WallCheck();
                if (AO.knockback != null) //Knockback
                    if (S.state == State.Knockback)
                    {
                        R.particleManager.StartPlay("KnockbackWaves");
                        R.particleManager.StartPlay("KnockbackSmoke");
                        AO.knockback.KnockbackEffect();
                    }

                if (AO.ledge != null) // Ledge
                    AO.ledge.CheckLedge(S.movementInput);

                if (AO.rotation != null)
                {
                    AO.rotation.SlopeGrounded();
                    AO.rotation.RotateToGroundOrUpright();
                }
                break;
            //Called whenever the player jumps to limit the amount the states are updated
            case "jump":

                break;
            //Called whenever the player is wallsliding
            case "wallsliding":

                break;
        }
    }

    void CanTurnChecker()
    {
        if (S.state == State.WallJumping || !S.isGrounded)
            S.canTurn = false;
        else
        {
            S.canTurn = true;
            if (S.movementInput.x != 0)
            {
                if (S.movementInput.x > 0)
                    S.facing = Vector2.right;
                else
                    S.facing = Vector2.left;
            }
        }


    }
    void GroundChecker()
    {
        if (AO.rotation != null)
        {
            AO.rotation.UpdateGrounded();
        }
        else
        {
            //if (Physics2D.Raycast(transform.position, Vector2.down, 0.7f, R.layerGround))
            if (Physics2D.OverlapBox((Vector3)M.groundCheckLocation + transform.position, M.groundCheckSize, 0f, R.layerGround))
            {
                if (!S.isGrounded)
                {
                    //c_sfxManager.PlaySoundOneShot("land");
                    R.particleManager.PlayParticle("PSFloorDust");
                }
                S.isGrounded = true;
                R.anim.SetBool("Grounded", true);
            }
            else
            {
                if (S.isGrounded)
                {
                    S.prevFrameGrounded = true;
                }
                else
                {
                    S.prevFrameGrounded = false;
                }
                S.isGrounded = false;
                R.anim.SetBool("Grounded", false);
            }
        }

        // change acceleration based on ground state
        if (S.state == State.Base && M.DIFF_AIR_VALS)
        {
            if (S.movementInput.y < 0 && S.isGrounded)
            {
                S.movementSpeed = M.movementSpeedCrouch;
                S.acceleration = M.accelerationCrouch;
                S.decceleration = M.deccelerationCrouch;
            }
            else if (S.isGrounded)
            {
                S.movementSpeed = M.movementSpeed;
                S.acceleration = M.acceleration;
                S.decceleration = M.decceleration;
            }
            else
            {
                S.movementSpeed = M.movementSpeedAir;
                S.acceleration = M.accelerationAir;
                S.decceleration = M.deccelerationAir;
            }
        }

        // reset mutli jump on landing
        if (S.isGrounded && M.MULT_JUMP && S.state == State.Base)
        {
            S.amountOfJumps = M.maxAmountOfJumpsInAir;
        }
    }
    #endregion

    #region QOL Methods
    // COYOTE TIME ----------------------
    void CoyoteTime()
    {
        if (!S.isGrounded && S.coyoteTimeActive == 0 && !(R.rb.linearVelocity.y > 0))
        {
            S.coyoteTimeActive = 1;
            StartCoroutine("CoyoteTimer");
        }
        else if (S.isGrounded && S.coyoteTimeActive != 0 && !(R.rb.linearVelocity.y > 0))
        {
            S.coyoteTimeActive = 0;
        }
    }
    IEnumerator CoyoteTimer()
    {
        yield return new WaitForSeconds(M.coyoteTimeSeconds);
        S.coyoteTimeActive = -1;
    }

    // JUMP CUE ----------------------------    
    void JumpCueCheck()
    {
        if (S.jumpQueue > 0)
        {
            JumpProcessing(1);
            S.jumpQueue -= 1;
        }
        else
        {
            StopAllCoroutines();
        }
    }
    IEnumerator JumpCueEmptyCache()
    {
        if (S.jumpQueue > 0)
        {
            yield return new WaitForSeconds(M.jumpQueueClearTimeSeconds);
            S.jumpQueue -= 1;
        }
    }
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector3)M.groundCheckLocation + transform.position, M.groundCheckSize);
    }
}