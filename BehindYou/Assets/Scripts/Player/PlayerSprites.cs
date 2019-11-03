using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSprites : MonoBehaviour
{
    private Player player;

    [Header("Sprites")]
    [SerializeField]
    private SpriteRenderer coreLights;
    [SerializeField]
    private SpriteRenderer weaponLights;
    [SerializeField]
    private SpriteRenderer boosterLights;

    [Space]
    [Header("UI")]
    [SerializeField]
    private Image shootImage;
    [SerializeField]
    private Image boostImage;
    [SerializeField]
    private Image grappleImage;
    [SerializeField]
    private Image stunImage;
    [SerializeField]
    private Image lethalityImage;

    [Space]
    [Header("Colors")]
    [SerializeField]
    private Gradient powerGradient;
    [SerializeField]
    private Gradient weaponGradient;
    [SerializeField]
    private Gradient UIGradient;
    [SerializeField]
    private Gradient statusGradient;
    [SerializeField]
    private Gradient lethalGradient;

    private void Awake()
    {
        player = GetComponent<Player>();

        coreLights.color = player.Color;
    }

    private void Update()
    {
        weaponLights.color = weaponGradient.Evaluate(player.Weapon.Lethality);
        boosterLights.color = powerGradient.Evaluate(player.BoosterCharge);

        shootImage.color = UIGradient.Evaluate(player.StunCharge);
        boostImage.color = UIGradient.Evaluate(player.BoosterCharge);

        stunImage.color = statusGradient.Evaluate(player.StunProgress);
        lethalityImage.color = lethalGradient.Evaluate(player.Weapon.Lethality);
    }
}
