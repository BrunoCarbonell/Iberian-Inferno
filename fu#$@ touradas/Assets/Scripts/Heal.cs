using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    public int HealAmount = 15;
    public ParticleSystem healEffect;
    public SpriteRenderer image;
    public GameObject effect;
    private BoxCollider2D col;

    private void Start()
    {
        col = GetComponentInChildren<BoxCollider2D>();
    }


    public int Use()
    {
        effect.SetActive(false);
        image.enabled = false;
        healEffect.Play(true);
        col.enabled = false;
        Destroy(gameObject, 2f);
        return HealAmount;
        
    }
}
