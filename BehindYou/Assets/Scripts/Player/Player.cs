using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : PhysicsObject
{
    private enum PlayerTimers
    {
        StunTimer,
        ShootTimer,
        BoosterCooldown,
        BoostDuration,
    }

    const float RETICLE_RANGE = 2.5f;

    private Weapon weapon;
    private Weakpoint weakpoint;
    private Animator animator;
    private TrailRenderer boostTrail;
    private List<Collider2D> colliders;
    private bool stunned = false;
    private bool dying = false;
    private bool boosting = false;
    private float[] timers;

    private Vector2 moveDirection;
    private Vector2 aimDirection;
    private bool boostInput;
    private bool shootInput;

    [Header("Stats")]
    [SerializeField]
    private float maxMoveSpeed;
    [SerializeField]
    private float maxMoveForce;
    [SerializeField]
    private float boostSpeedMultiplier;
    [SerializeField]
    private Color playerColor;
    [SerializeField]
    private float[] timerDurations;
    [Space]
    [Header("Objects")]
    [SerializeField]
    private GameObject cursor;
    [SerializeField]
    private GameObject stunProjectile;
    [Space]
    [Header("Particles")]
    [SerializeField]
    private ParticleSystem smokeParticles;
    [SerializeField]
    private ParticleSystem sparkParticles;
    [SerializeField]
    private ParticleSystem stunParticles;

    #region Properties

    /// <summary>
    /// The color relating to the players team
    /// </summary>
    public Color Color { get { return playerColor; } }

    /// <summary>
    /// The player's equipped weapon
    /// </summary>
    public Weapon Weapon { get { return weapon; } }

    /// <summary>
    /// Whether or not the player is currently boosting
    /// </summary>
    public bool IsBoosting { get { return boosting; } }

    /// <summary>
    /// The charge progress of the booster
    /// </summary>
    public float BoosterCharge
    {
        get
        {
            if (boosting)
            {
                return 0;
            }

            float charge = timers[(int)PlayerTimers.BoosterCooldown] / timerDurations[(int)PlayerTimers.BoosterCooldown];

            if(charge > 1)
            {
                charge = 1;
            }

            return charge;
        }
    }

    /// <summary>
    /// The charge progress of the stun gun
    /// </summary>
    public float StunCharge
    {
        get
        {
            float charge = timers[(int)PlayerTimers.ShootTimer] / timerDurations[(int)PlayerTimers.ShootTimer];

            if (charge > 1)
            {
                charge = 1;
            }

            return charge;
        }
    }

    /// <summary>
    /// The progress of the stun status from 0 to 1
    /// </summary>
    public float StunProgress
    {
        get
        {
            if (stunned)
            {
                float charge = timers[(int)PlayerTimers.StunTimer] / timerDurations[(int)PlayerTimers.StunTimer];

                if (charge > 1)
                {
                    charge = 1;
                }

                return charge;
            }
            else
            {
                return 1;
            }
        }
    }

    /// <summary>
    /// The current move speed of the player
    /// </summary>
    public float CurrentSpeed
    {
        get
        {
            if (boosting)
            {
                return maxMoveSpeed * boostSpeedMultiplier;
            }

            return maxMoveSpeed;
        }
    }

    #endregion

    #region Unity Functions

    protected override void Awake()
    {
        base.Awake();
        weapon = GetComponentInChildren<Weapon>();
        weakpoint = GetComponentInChildren<Weakpoint>();
        animator = GetComponent<Animator>();
        boostTrail = GetComponent<TrailRenderer>();

        colliders = new List<Collider2D>();

        colliders.Add(GetComponent<Collider2D>());

        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();

        for(int i = 0; i < childColliders.Length; i++)
        {
            colliders.Add(childColliders[i]);
        }

        timers = new float[timerDurations.Length];

        for(int i = 0; i < timers.Length; i++)
        {
            timers[i] = 0.0f;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        for (int i = 0; i < timers.Length; i++)
        {
            timers[i] += Time.deltaTime;
        }

        if(stunned && timers[(int)PlayerTimers.StunTimer] >= timerDurations[(int)PlayerTimers.StunTimer])
        {
            stunned = false;
            stunParticles.Stop();
        }

        if(boosting && timers[(int)PlayerTimers.BoostDuration] >= timerDurations[(int)PlayerTimers.BoostDuration])
        {
            boosting = false;
            boostTrail.emitting = false;
            timers[(int)PlayerTimers.BoosterCooldown] = 0;
        }

        if(BoosterCharge >= 1 && boostInput)
        {
            Boost();
        }

        if(StunCharge >= 1 && shootInput)
        {
            Shoot(stunProjectile);
        }

        UpdateMovement();
        UpdateAim();

        base.Update();

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg - 90);
    }

    #endregion

    #region Input Management

    /// <summary>
    /// Applies movement force based on the input direction
    /// </summary>
    private void UpdateMovement()
    {
        Vector2 move = (moveDirection * CurrentSpeed) - Velocity;
        float moveDistance = move.magnitude;

        if (moveDistance > 0)
        {
            move = (move / moveDistance) * (moveDistance / (2 * CurrentSpeed)) * maxMoveForce;
        }

        ApplyForce(move);
    }

    /// <summary>
    /// Moves the aim reticle based on the aim direction
    /// </summary>
    private void UpdateAim()
    {
        Vector2 target = aimDirection;

        if (target == Vector2.zero)
        {
            target = Velocity.normalized;
        }

        cursor.transform.position = Position + (target * RETICLE_RANGE);
    }

    /// <summary>
    /// Reads input for player movement
    /// </summary>
    public void MoveInput(InputAction.CallbackContext value)
    {
        moveDirection = value.ReadValue<Vector2>();
    }

    /// <summary>
    /// Reads shoot button input
    /// </summary>
    public void ShootInput(InputAction.CallbackContext value)
    {
        if(value.ReadValue<float>() != 0)
        {
            shootInput = true;
        }
        else
        {
            shootInput = false;
        }
    }

    /// <summary>
    /// Reads boost button input
    /// </summary>
    public void BoostInput(InputAction.CallbackContext value)
    {
        if(value.ReadValue<float>() != 0)
        {
            boostInput = true;
        }
        else
        {
            boostInput = false;
        }
    }

    /// <summary>
    /// Reads grapple button input
    /// </summary>
    public void GrappleInput(InputAction.CallbackContext value)
    {
        Debug.Log("Shooting grappling hook");
    }

    /// <summary>
    /// Handles input for player aiming
    /// </summary>
    public void AimInput(InputAction.CallbackContext value)
    {
        aimDirection = value.ReadValue<Vector2>();

        //Debug.Log("Controller: " + value.control.device.displayName);

        if(value.control.device.name == "Mouse")
        {
            aimDirection = (aimDirection - (Vector2)Camera.main.WorldToScreenPoint(Position)).normalized;
        }
    }

    #endregion

    #region Status

    /// <summary>
    /// Stuns the player for a set amount of time
    /// </summary>
    /// <param name="duration">The length of the stun</param>
    public void Stun(float duration)
    {
        timerDurations[(int)PlayerTimers.StunTimer] = duration;
        timers[(int)PlayerTimers.StunTimer] = 0;
        stunned = true;
        stunParticles.Play();
    }

    /// <summary>
    /// Playes the death animation and then respawns the player
    /// </summary>
    public void Die()
    {
        //Disable colliders
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Time.timeScale = 0.5f;

        //Play death animation
        animator.SetTrigger("Death");

        dying = true;
    }

    public void Respawn()
    {
        //Move to a spawn location away from the other player
        Time.timeScale = 1.0f;

        //Enable colliders
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }

        dying = false;
    }

    #endregion

    #region Actions

    /// <summary>
    /// Shoots a bullet in the specified direction
    /// </summary>
    /// <param name="projectile">The bullet to shoot</param>
    public void Shoot(GameObject projectile)
    {
        Projectile bullet = Instantiate(projectile, transform.position + ((cursor.transform.position - transform.position) / RETICLE_RANGE), Quaternion.identity).GetComponent<Projectile>();
        bullet.Direction = (cursor.transform.position - transform.position) / RETICLE_RANGE;

        TrailRenderer bulletTrail = bullet.GetComponent<TrailRenderer>();

        if (bulletTrail != null)
        {
            bulletTrail.colorGradient = boostTrail.colorGradient;
        }

        timers[(int)PlayerTimers.ShootTimer] = 0;
    }

    /// <summary>
    /// Initiates a boost
    /// </summary>
    public void Boost()
    {
        boosting = true;
        boostTrail.emitting = true;
        timers[(int)PlayerTimers.BoostDuration] = 0;
    }

    #endregion

    #region Helper Functions

    /// <summary>
    /// Turns the particle systems on
    /// </summary>
    public void StartDeathParticles()
    {
        smokeParticles.Play();
        sparkParticles.Play();
    }

    /// <summary>
    /// Turns the particle systems off
    /// </summary>
    public void StopDeathParticles()
    {
        smokeParticles.Stop();
        sparkParticles.Stop();
    }

    #endregion
}