using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Player player;
    private float lethality;
    [SerializeField]
    private float LethalSpeed;

    /// <summary>
    /// Wether or not this weapon is capable of killing anything
    /// </summary>
    public bool IsLethal { get { return lethality >= 1; } }

    /// <summary>
    /// A float representing how lethal the weapon is from 0 to 1, it can go beyond 1 but it doesn't mean anything
    /// </summary>
    public float Lethality { get { return lethality; } }

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        float speed = player.Velocity.magnitude;

        lethality = speed / LethalSpeed;
    }
}
