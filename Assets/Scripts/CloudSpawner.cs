using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject cloud;

    public float spawnEvery;
    public int speed;

    private float _lastSpawn;

    private Vector2 _spawnPos;
    private Quaternion _spawnRot;

    // Start is called before the first frame update
    public void Spawn()
    {
        _lastSpawn = Time.realtimeSinceStartup;
        GameObject c = Instantiate(cloud, _spawnPos, _spawnRot);
        Cloud cscript = c.GetComponent<Cloud>();
        cscript.speed = speed;
    }

    void Start()
    {
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.realtimeSinceStartup - _lastSpawn > spawnEvery) Spawn();
    }
}
