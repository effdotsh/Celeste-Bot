using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    private List<Collider2D> _ignoreList = new List<Collider2D>();
    public Collider2D collider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            _ignoreList.Add(col);
            CharacterController p = col.gameObject.GetComponent<CharacterController>();
            p.GiveDash();
            Physics2D.IgnoreCollision(col, collider, true);
        }
    }

    public void TriggerReset()
    {
        // Debug.Log("BALLOON RESET");
        foreach (Collider2D col in _ignoreList)
        {
            Physics2D.IgnoreCollision(col, collider, false);
        }

        _ignoreList = new List<Collider2D>();
    }
}
