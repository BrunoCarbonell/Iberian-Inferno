using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearController : MonoBehaviour
{
    public float damage = 5;
    public Transform player;
    private bool isStuck = false;
    public ParticleSystem hitParticle;


   public void ChangeSkin(Sprite skin)
    {
        GetComponent<SpriteRenderer>().sprite = skin;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isStuck)
        {
            player.GetComponent<BullController>().Hit(damage);
            transform.SetParent(player.transform, true);
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Collider2D>().enabled = false;
            isStuck = true;
            hitParticle.Play();
        }
    }
}
