
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using SaintsField;

//using static UnityEditor.VersionControl.Asset;
using static UnityEngine.Rendering.DebugUI;

/*
 * Variable Legend:
 * 
 * 
 * v_ = value
 * c_ = component
 * ch_ = checker
 * m_ = modifier
 * a_ = activatable modifier [used for platformer settings to activate coyote time or jump cueing]
 * chm_ = checker modifier
 * s_ = state (completely defining of movement ex. vaulting)
 * ss_ = substate (helps to find what move but doesnt define ex. moving or grounded)
 * lm_ = layermask
 * 
 * ao_ = add on (usually script)
 * 
 */

public class Platformer2DMovementBase : BaseMovementParent
{
    #region States
    public enum state
    {
        Inactive,
        Active,
        Stun,
        Knockback,
        Dashing,
        WallSliding,
        WallJumping,
        Ledging
    }
    [Header("States + Sub-States")]
    [SerializeField] public state s_playerState = state.Inactive;

    [HideInInspector] public int ss_visual_facing = 1;
    [HideInInspector] public int ss_amountOfJumps;
    [HideInInspector] public Vector2 ss_facing;
    [HideInInspector] public bool ss_canTurn;
    [HideInInspector] public bool ss_attacking;
    [HideInInspector] public bool ss_isGrounded;
    [HideInInspector] public bool ss_canQuickFall;
    //QOL
    public int ss_coyoteTimeActive; // |-1 = disabled|0 = able to change|1 = active|
    [HideInInspector] public int ss_jumpCue;

    [HideInInspector] public Vector2 groundNormal = Vector2.up;
    [HideInInspector] public Vector2 horizontalTangent = Vector2.right;
    [HideInInspector] public float lastTangentialSpeed = 0f;
    //Slopes

    #endregion

    #region Modifiers
    [Header("Movement")]
    [Range(0.0f, 25.0f)][SerializeField] public float m_movementSpeed;
    [Range(0.0f, 25.0f)][SerializeField] public float m_groundAcceleration;
    [Range(0.0f, 25.0f)][SerializeField] public float m_groundDecceleration;
    [Header("Air Movement")]
    [Range(0.0f, 25.0f)][SerializeField] float m_airAcceleration;
    [Range(0.0f, 25.0f)][SerializeField] float m_airDecceleration;
    [Header("Current Movement")]
    public float m_acceleration;
    public float m_decceleration;

    [Header("Jump")]
    [SerializeField] int m_maxAmountOfJumps = 1;
    [Range(0.0f, 25.0f)][SerializeField] float m_jumpForce;
    [SerializeField] bool QUICK_FALL_OPTION; // the player CHOOSES quickfall when releasing jump button
    [Range(0.0f, 25.0f)][SerializeField] float m_quickFallForce;
    [SerializeField] bool QUICK_FALLEN_FORCE; // the player FORCES quickfall at peak velocity
    [Range(-25.0f, 25.0f)][SerializeField] float m_peakJumpVelocity;
    [Header("Jump QOL")]
    [SerializeField] bool COYOTE_TIME;
    [Range(0.0f, 5.0f)][SerializeField] float m_coyoteTimeSeconds;
    [SerializeField] bool JUMP_CUE;
    [Range(0.0f, 5.0f)][SerializeField] float m_jumpCueMax;
    [Range(0.0f, 5.0f)][SerializeField] float m_jumpCueClearTimeSeconds;
    [Header("Gravity Mod")]
    [SerializeField] float m_fallClamp; // clamp the fall speed of the player to make them fall slower

    float knockbackThreshold = 15;
    float knockbackTime;
    #endregion

    #region Inputs
    public Vector2 in_movementInput;
    #endregion

    #region Layermasks
    [Header("Other")]
    [SerializeField] public LayerMask lm_ground;
    #endregion

    #region Components
    Rigidbody2D c_rb;
    Animator c_anim;
    ParticleManager c_particleManager;
    //SoundEffectManager c_sfxManager;
    #endregion

    #region Add Ons
    Platformer2DLedge ao_ledge;
    Platformer2DKnockback ao_knockback;
    Platformer2DWallSlideAndJump ao_wall;
    Platformer2DVaulting ao_vault;
    Platformer2DRunning ao_run;
    Platformer2DRotation ao_rotation;
    #endregion

    // METHODS START!

    #region Basic Functions
    //Get all components 
    void ComponentGrab()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_anim = GetComponent<Animator>();
        c_particleManager = GetComponent<ParticleManager>();
        //c_sfxManager = GetComponent<SoundEffectManager>();

        //Add-Ons
        TryGetComponent<Platformer2DRotation>(out ao_rotation);
        TryGetComponent<Platformer2DLedge>(out ao_ledge);
        TryGetComponent<Platformer2DKnockback>(out ao_knockback);
        TryGetComponent<Platformer2DWallSlideAndJump>(out ao_wall);
        TryGetComponent<Platformer2DVaulting>(out ao_vault);
        TryGetComponent<Platformer2DRunning>(out ao_run);
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
    public override void Launch(Vector2 direction, float force, bool resetForce)
    {
        if (resetForce)
            c_rb.linearVelocity = new Vector2(c_rb.linearVelocity.x, 0);

        c_rb.AddForce(direction * force, ForceMode2D.Impulse);
        // For platform fighter
        if (force > knockbackThreshold)
        {
            s_playerState = state.Knockback;
            knockbackTime = force / 2;
            m_decceleration = 0;
            m_acceleration = 1;
        }
    }
    #endregion

    #region Input Functions
    public void MoveInput(Vector2 value)
    {
        in_movementInput = value;
        if (s_playerState == state.Active && (in_movementInput.x != 0 || in_movementInput.y != 0))
        {
            //ss_facing = in_movementInput;
            ss_facing = new Vector2(ss_visual_facing, 0);
            //body.transform.rotation = Quaternion.Euler(0f, 0f, VectorUtils.VectorToAngle(ss_facing));
        }
        else if (s_playerState == state.Active)
        {
            ss_facing = new Vector2(ss_visual_facing, 0);
        }
    }
    public void JumpInput(float value)
    {
        float t_jumpPressed = value;
        if (JUMP_CUE)
        {
            if (ss_jumpCue < m_jumpCueMax && t_jumpPressed == 1)
            {
                ss_jumpCue += 1;
                StartCoroutine(JumpCueEmptyCache());
            }
            else if(t_jumpPressed == 0)
            {
                if (c_rb.linearVelocity.y < -m_quickFallForce && ss_jumpCue > 0)
                {
                    ss_jumpCue -= 1;
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
                if (ss_canQuickFall && QUICK_FALL_OPTION)
                {
                    c_rb.linearVelocity += Vector2.down * m_quickFallForce;
                    ss_canQuickFall = false;
                }
                break;
            //when the jump button is pressed [everything that relates]
            case 1:
                UpdateStates("jump"); // Updates states to be able to determine what the player wants to do

                if (false)
                {

                }
                // Vaulting
                else if (ao_vault != null && ao_vault.TRIGGER_BY_JUMP_KEY && ao_vault.CheckVaultCondition((int)in_movementInput.x) && ss_isGrounded)
                {
                    print("Vault");
                    ao_vault.Vault((int)in_movementInput.x);
                }
                // Ledge Jump
                else if (s_playerState == state.Ledging)
                {
                    print("LedgeJump");
                    Launch(Vector2.up, 8, true);
                    s_playerState = state.WallJumping;
                }
                //Wall Jumping
                else if (ao_wall != null && ao_wall.CanWallJump() && !ss_isGrounded)
                {
                    print("WallJump");
                    //c_sfxManager.PlaySoundOneShot("jump");
                    ss_amountOfJumps = m_maxAmountOfJumps;
                    ao_wall.HandleWallJump(p_jumpPressed);
                    ss_canQuickFall = true;
                }
                //Double Jumping
                else if (ss_amountOfJumps > 0 && !ss_isGrounded)
                {
                    print("Double Jump");
                    ss_amountOfJumps--;
                    ss_canQuickFall = true;
                    BasicJump();
                    ss_coyoteTimeActive = -1;
                }
                // Basic Jump
                else if(ss_isGrounded || ss_coyoteTimeActive == 1)
                {
                    print("Basic Jump");
                    ss_canQuickFall = true;
                    BasicJump();
                    ss_coyoteTimeActive = -1;
                }

                else if (JUMP_CUE)
                {
                    ss_jumpCue += 1;
                }

                if (JUMP_CUE)
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

        Launch(groundNormal, m_jumpForce,true);
    }
    #endregion

    #region Main Functions
    private void Start()
    {
        ComponentGrab();
        s_playerState = state.Active;
        ss_visual_facing = 1;
        ss_facing = new Vector2(1, 0);
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
        switch (s_playerState)
        {
            case state.Active:
                c_rb.gravityScale = 1;
                LRMovement(); // Left and right movement
                if (!ss_isGrounded && c_rb.linearVelocity.y < m_peakJumpVelocity && QUICK_FALLEN_FORCE) // Force quickfall
                    c_rb.linearVelocity += Vector2.up * Physics2D.gravity.y * m_quickFallForce * Time.deltaTime;
                break;

            case state.WallJumping:
                c_rb.gravityScale = 1;
                if (c_rb.linearVelocityY < m_peakJumpVelocity)
                {
                    print("Wall Jump to Active");
                    s_playerState = state.Active;
                }
                break;

            case state.Ledging:
                c_rb.gravityScale = 0;
                c_rb.linearVelocity = Vector2.zero;
                UpdateStates("wallsliding");

                break;

            case state.Dashing:
                if (ao_vault != null && ao_vault.TRIGGER_BY_DASH && in_movementInput.x != 0 && ao_vault.CheckVaultCondition((int)in_movementInput.x)) { // Dash Vaulting
                    print("Dash Vault");
                    ao_vault.Vault((int)in_movementInput.x);
                }
                break;

            case state.WallSliding:
                LRMovement(); // Left and right movement
                // Slow the player's descent
                c_rb.linearVelocity = new Vector2(c_rb.linearVelocity.x, -ao_wall.m_slideSpeed);
                UpdateStates("wallsliding");
                break;

            case state.Inactive:

                break;
        }
        //Fallclamp Applied
        c_rb.linearVelocity = new Vector2(c_rb.linearVelocity.x,Mathf.Clamp(c_rb.linearVelocity.y,m_fallClamp,20));
    }

    void LRMovement()
    {
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
        float targetSpeed = in_movementInput.x * m_movementSpeed;
        float currentSpeed = Vector2.Dot(c_rb.linearVelocity, tangent);
        lastTangentialSpeed = currentSpeed;
        float speedDif = targetSpeed - currentSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? m_acceleration : m_decceleration;
        float movement = speedDif * accelRate;
        c_rb.AddForce(movement * tangent, ForceMode2D.Force);
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
                if (COYOTE_TIME)
                    CoyoteTime();
                if(JUMP_CUE)
                    JumpCueCheck();
                //Add-on update-----------------------
                if (ao_run != null) // Run
                    ao_run.CheckRun((int)in_movementInput.x);
                if(ao_wall != null) // Wall
                    ao_wall.WallCheck();
                if (ao_knockback != null) //Knockback
                    if (s_playerState == state.Knockback) {
                        c_particleManager. StartPlay("KnockbackWaves");
                        c_particleManager. StartPlay("KnockbackSmoke");
                        ao_knockback.KnockbackEffect();
                    }
                if (ao_ledge != null) // Ledge
                    ao_ledge.CheckLedge(in_movementInput);
                if (ao_rotation != null)
                {
                    ao_rotation.SlopeGrounded();
                    ao_rotation.RotateToGroundOrUpright();
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
        if (s_playerState == state.WallJumping || !ss_isGrounded || ss_attacking)
            ss_canTurn = false;
        else
        {
            ss_canTurn = true;
            if (in_movementInput.x != 0)
            {
                if(in_movementInput.x > 0)
                    ss_visual_facing = 1;
                else
                    ss_visual_facing = -1;
            }
        }


    }
    // call from animation to change attacking state
    public void ChangeAttackingTrue()
    {
        ss_attacking = true;
    }
    public void ChangeAttackingFalse()
    {
        ss_attacking = false;
    }
    void GroundChecker()
    {
        if (ao_rotation != null)
        {
            ao_rotation.UpdateGrounded();
        }
        else
        {
            if (Physics2D.Raycast(transform.position, Vector2.down, 0.7f, lm_ground))
            {
                if (!ss_isGrounded)
                {
                    //c_sfxManager.PlaySoundOneShot("land");
                }
                ss_isGrounded = true;
                c_anim.SetBool("Grounded", true);
            }
            else
            {
                ss_isGrounded = false;
                c_anim.SetBool("Grounded", false);
            }
        }
        if (s_playerState == state.Active)
        {
            if (ss_isGrounded)
            {
                m_acceleration = m_groundAcceleration;
                m_decceleration = m_groundDecceleration;

                //reset amount of jumos
                ss_amountOfJumps = m_maxAmountOfJumps;
            }
            else
            {
                m_acceleration = m_airAcceleration;
                m_decceleration = m_airDecceleration;
            }
        }
    }
    #endregion

    #region QOL Methods
    // COYOTE TIME ----------------------
    void CoyoteTime()
    {
        if(!ss_isGrounded && ss_coyoteTimeActive == 0 && !(c_rb.linearVelocity.y > 0))
        {
            ss_coyoteTimeActive = 1;
            StartCoroutine("CoyoteTimer");
        }
        else if(ss_isGrounded && ss_coyoteTimeActive != 0 && !(c_rb.linearVelocity.y > 0))
        {
            ss_coyoteTimeActive = 0;
        }
    }
    IEnumerator CoyoteTimer()
    {
        yield return new WaitForSeconds(m_coyoteTimeSeconds);
        ss_coyoteTimeActive = -1;
    }

    // JUMP CUE ----------------------------    
    void JumpCueCheck()
    {
        if(ss_jumpCue > 0)
        {
            JumpProcessing(1);
            ss_jumpCue -= 1;
        }
        else
        {
            StopAllCoroutines();
        }
    }
    IEnumerator JumpCueEmptyCache()
    {
        if (ss_jumpCue > 0)
        {
            yield return new WaitForSeconds(m_jumpCueClearTimeSeconds);
            ss_jumpCue -= 1;
        }
    }
    #endregion
}
