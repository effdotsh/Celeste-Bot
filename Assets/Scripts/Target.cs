using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    public float value;
    private float _id;

    public bool isWin;

    public bool withDashOnly;

    public bool acceptAny;
    void Start()
    {
        _id = Random.value;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public float GetID()
    {
        return _id;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag.Equals("Player"))
        {
            CharacterController c = col.gameObject.GetComponent<CharacterController>();

            if (!withDashOnly || c.HasDash())
            {
                c.TargetReached(this);
            }
        }
    }
}