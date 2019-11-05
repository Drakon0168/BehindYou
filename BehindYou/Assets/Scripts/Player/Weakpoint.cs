using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weakpoint : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Weapon other = collision.GetComponent<Weapon>();

        if(other != null && other.IsLethal)
        {
            player.Die();
        }
    }
}