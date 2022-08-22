using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    public Color agentColor;
    private const int NumChoices = 12;
    private const int MinActionTime = 5;
    private const int MaxActionTime = 25;

    public float maxMutationChance;
    public float minMutationChance;
    public float deletionCutoff;
    public float insertionCutoff;

    public int populationSize;
    public int delayStart;
    public GameObject player;

    private float _bestFitness = 0;
    private List<int> _bestActions;
    private int _bestMutStartInd = 0;
    private int _bestRandStartInd = 0;

    private CharacterController _bestReplayer;

    public bool hideAgents;


    public int increaseBy;

    public int increaseEvery;


    private int _deadCounter = 0;


    private bool _won = false;

    private List<CharacterController> _agents = new List<CharacterController>();


    // Start is called before the first frame update
    void Start()
    {
        // Time.timeScale = 2f;
        Transform t = transform;


        GameObject best = Instantiate(player, t.position, t.rotation);
        _bestReplayer = best.GetComponent<CharacterController>();
        _bestReplayer.manager = this;
        _bestReplayer.humanPlayer = false;
        _bestReplayer.SetActions(Mutate(_bestReplayer.GetActions(), 0));

        _bestReplayer.trailEnabled = true;
        SpriteRenderer bs = best.GetComponent<SpriteRenderer>();
        bs.sortingOrder = 5;
        _bestReplayer.normalTrail.sortingOrder = 4;
        _bestReplayer.dashTrail.sortingOrder = 3;

        for (int i = 0; i < populationSize; i++)
        {
            GameObject p = Instantiate(player, t.position, t.rotation);
            CharacterController c = p.GetComponent<CharacterController>();
            _agents.Add(c);
            c.manager = this;
            c.humanPlayer = false;
            c.trailEnabled = false;
            c.dashTrail.enabled = false;
            c.normalTrail.enabled = false;
            SpriteRenderer s = p.GetComponent<SpriteRenderer>();
            s.sortingOrder = 1;
            s.color = agentColor;
            if (hideAgents)
            {
                s.enabled = false;
            }

            c.Kill();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Report(CharacterController c)
    {
        _deadCounter++;
        if (c == _bestReplayer)
        {
            c.SetActions(_bestActions);
            return;
        }


        var fitness = c.GetFitness();
        var actions = c.GetActions();


        if (fitness > _bestFitness)
        {
            _bestFitness = fitness;
            _bestActions = actions;
            _bestMutStartInd = c.GetActionMutationStart();
            _bestRandStartInd = c.GetCompleteRandStart();
            _won = c.won;
            Debug.Log(_bestFitness);
            // print("-----");
            // print(_bestMutStartInd);
            // print(_bestRandStartInd);
        }
        else if (Mathf.Abs(_bestFitness - 0) < 0.001) _bestActions = actions;


        if (Time.realtimeSinceStartup < delayStart)
        {
            //Create a new random list before timer expires
            c.SetActions(new List<int>());
            return;
        }


        List<int> mutActions = Mutate(_bestActions, _bestMutStartInd);
        c.SetActions(mutActions);


        if (_deadCounter >= populationSize)
        {
            ResetGame();
        }
    }

    private void ResetGame()
    {
        _deadCounter = 0;
        foreach (var a in _agents)
        {
            a.Respawn();
            if (_won) a.SetActions(_bestActions);
        }
        _bestReplayer.SetActions(_bestActions);
        _bestReplayer.Respawn();

        GameObject[] balloons = GameObject.FindGameObjectsWithTag("Balloon");
        if (balloons != null)
        {
            foreach (GameObject bal in balloons)
            {
                Balloon b = bal.GetComponent<Balloon>();
                b.TriggerReset();
            }
        }
        
        
        GameObject[] clouds = GameObject.FindGameObjectsWithTag("Cloud");
        if (clouds != null)
        {
            foreach (GameObject cl in clouds)
            {
                Destroy(cl);
            }
        }
        
        GameObject[] cspawner = GameObject.FindGameObjectsWithTag("Cloud Spawner");
        if (cspawner != null)
        {
            foreach (GameObject cs in cspawner)
            {
                CloudSpawner spawner = cs.GetComponent<CloudSpawner>();
                spawner.Spawn();
            }
        }
    }


    public int[] GenerateActionPair()
    {
        int time = (int)(Random.value * (MaxActionTime - MinActionTime)) + MinActionTime;
        int action = (int)(Random.value * NumChoices);
        return new int[] { time, action };
    }

    private List<int> Mutate(List<int> actions, int startInd)
    {
        var mutChance = (maxMutationChance - minMutationChance) * Random.value + minMutationChance;
        var mutActions = new List<int>();

        int targetLen = (int)(Time.realtimeSinceStartup / increaseEvery + 1) * increaseBy * 2;


        for (int i = 0; i < actions.Count; i += 2)
        {
            var roll = Random.value;
            if (i > _bestRandStartInd + 2) roll = 0;

            if (i <= startInd + 2 || roll > mutChance)
            {
                mutActions.Add(actions[i]);
                mutActions.Add(actions[i + 1]);
                continue;
            }


            int[] newSet = GenerateActionPair();
            mutActions.Add(newSet[0]);
            mutActions.Add(newSet[1]);
        }


        while (mutActions.Count < targetLen)
        {
            int[] n = GenerateActionPair();
            mutActions.Add(n[0]);
            mutActions.Add(n[1]);
        }

        return mutActions;
    }
}