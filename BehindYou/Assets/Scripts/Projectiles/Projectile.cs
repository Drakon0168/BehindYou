using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PhysicsObject
{
    [SerializeField]
    private float flightSpeed;
    [SerializeField]
    private float lifeTime;

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

    protected override void Update()
    {
        Velocity = direction * flightSpeed;

        base.Update();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
