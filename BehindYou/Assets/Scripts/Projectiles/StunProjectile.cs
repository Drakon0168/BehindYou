using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunProjectile : Projectile
{
    [SerializeField]
    public float stunDuration;

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Player other = collision.GetComponent<Player>();

        if(other != null)
        {
            other.Stun(stunDuration);
        }
        else
        {
            other = collision.GetComponentInParent<Player>();

            if(other != null)
            {
                other.Stun(stunDuration);
            }
        }

        base.OnTriggerEnter2D(collision);
    }
}
