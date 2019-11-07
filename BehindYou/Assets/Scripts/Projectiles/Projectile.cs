using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PhysicsObject
{
    [SerializeField]
    private float flightSpeed;
    [SerializeField]
    private float lifeTime;
    [SerializeField, Tooltip("How fast the projectile can turn in degrees per second")]
    private float seekForce;
    [SerializeField]
    private float seekDistance;

    [HideInInspector]
    public Transform target;

    private Vector2 direction;

    //The direction that this projectile should move in
    public Vector2 Direction
    {
        get { return direction; }
        set
        {
            direction = value.normalized;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        Destroy(gameObject, lifeTime);
    }

    private void Start()
    {
        Velocity = direction * flightSpeed;
    }

    protected override void Update()
    {
        Vector2 distance = target.position - transform.position;

        if(distance.magnitude <= seekDistance)
        {
            SeekTarget(distance.normalized);
        }
        else if(Velocity.magnitude != flightSpeed)
        {
            Vector2 targetVelocity = Velocity.normalized * flightSpeed;
            ApplyForce(targetVelocity - Velocity);
        }

        base.Update();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

    private void SeekTarget(Vector2 targetDirection)
    {
        Vector2 targetVelocity = targetDirection * flightSpeed;

        Vector2 force = targetVelocity - Velocity;
        float forceDistance = force.magnitude;

        if (forceDistance > 0)
        {
            force = (force / forceDistance) * (forceDistance / (2 * flightSpeed)) * seekForce;
        }

        ApplyForce(force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, seekDistance);
    }
}
