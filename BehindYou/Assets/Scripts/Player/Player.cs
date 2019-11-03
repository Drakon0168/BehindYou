using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    [HideInInspector]
    public playerControls controls;

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

        if (InputManager.Instance.GetInput(controls.shoot) && StunCharge >= 1)
        {
            Shoot(stunProjectile);
        }

        if (InputManager.Instance.GetInput(controls.boost) && BoosterCharge >= 1)
        {
            boosting = true;
            boostTrail.emitting = true;
            timers[(int)PlayerTimers.BoostDuration] = 0;
        }

        MoveInput();
        AimInput();

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg - 90);

        base.Update();
    }

    /// <summary>
    /// Handles input for player movement
    /// </summary>
    private void MoveInput()
    {
        Vector2 targetDirection = Vector2.zero;

        if (!stunned && !dying)
        {
            if (controls.axisMovement)
            {
                targetDirection = InputManager.Instance.GetDirection(controls.moveAxis, this);
            }
            else
            {
                if (InputManager.Instance.GetInput(controls.moveUp))
                {
                    targetDirection += Vector2.up;
                }

                if (InputManager.Instance.GetInput(controls.moveDown))
                {
                    targetDirection += Vector2.down;
                }
                if (InputManager.Instance.GetInput(controls.moveLeft))
                {
                    targetDirection += Vector2.left;
                }
                if (InputManager.Instance.GetInput(controls.moveRight))
                {
                    targetDirection += Vector2.right;
                }
            }
        }
        
        Vector2 moveDirection = (targetDirection.normalized * CurrentSpeed) - Velocity;
        float moveDistance = moveDirection.magnitude;

        if(moveDistance > 0)
        {
            moveDirection = (moveDirection / moveDistance) * (moveDistance / (2 * CurrentSpeed)) * maxMoveForce;
        }
        
        ApplyForce(moveDirection);
    }

    /// <summary>
    /// Handles input for player aiming
    /// </summary>
    private void AimInput()
    {
        Vector2 aimDirection = InputManager.Instance.GetDirection(controls.aim, this);

        if(aimDirection == Vector2.zero)
        {
            aimDirection = Velocity;
        }

        aimDirection = aimDirection.normalized;

        cursor.transform.position = Position + (aimDirection * RETICLE_RANGE);
    }

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
}
