using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGoal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        print("Win");
        if (collision.gameObject.tag.Equals("Player"))
        {
            CharacterController p = collision.gameObject.GetComponent<CharacterController>();
            p.Kill();
        }
    }
}
