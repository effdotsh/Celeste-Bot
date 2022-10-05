using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Balloon : Target {
    private List<float> _ignoreList = new List<float>();
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
            CharacterController p = col.gameObject.GetComponent<CharacterController>();
            if (!_ignoreList.Contains(p.id))
            {
                _ignoreList.Add(p.id);
                p.GiveDash();
            }
            p.TargetReached(this, 1);
        }
    }

    public void TriggerReset()
    {
        // SpriteRenderer s = GetComponent<SpriteRenderer>();
        // s.color = new Color(Random.value , Random.value , Random.value);

        _ignoreList = new List<float>();
    }
}
