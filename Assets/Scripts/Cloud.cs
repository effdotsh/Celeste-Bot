using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;
    public Collider2D collider;
    public float speed;
    void Start()
    {
        IgnoreOthers("floor");
        IgnoreOthers("Cloud");

        IgnoreOthers("Border");

    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(speed, 0);
    }
    private void IgnoreOthers(String t)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(t);

        if (players == null)
            return;

        foreach (GameObject player in players)
        {
            Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), collider);
        }
    }
}
