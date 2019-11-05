using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsObject : MonoBehaviour
{
    private Vector2 acceleration;
    private new Rigidbody2D rigidbody2D;

    /// <summary>
    /// The current velocity of this object
    /// </summary>
    public Vector2 Velocity
    {
        get { return rigidbody2D.velocity; }
        set { rigidbody2D.velocity = value; }
    }

    /// <summary>
    /// 2D representation of the position
    /// </summary>
    public Vector2 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    protected virtual void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        Velocity += acceleration * Time.deltaTime;
        acceleration = Vector2.zero;
    }

    /// <summary>
    /// Applies a force affected by mass
    /// </summary>
    /// <param name="force">The force to apply</param>
    public void ApplyForce(Vector2 force)
    {
        acceleration += force / rigidbody2D.mass;
    }
}
