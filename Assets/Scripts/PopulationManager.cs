using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
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

    private float _bestFitness = -999999;
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

        for (int i = 0; i < populationSize; i++)
        {
            GameObject p = Instantiate(player, t.position, t.rotation);
            CharacterController c = p.GetComponent<CharacterController>();
            _agents.Add(c);
            c.manager = this;
            c.humanPlayer = false;

            if (hideAgents)
            {
                SpriteRenderer s = p.GetComponent<SpriteRenderer>();
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

        if (fitness >= _bestFitness)
        {
            _bestFitness = fitness;
            _bestActions = actions;
            _bestMutStartInd = c.GetActionMutationStart();
            _bestRandStartInd = c.GetCompleteRandStart();
            _won = c.won;
            // print("-----");
            print(_bestFitness);
            // print(_bestMutStartInd);
            // print(_bestRandStartInd);
        }

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
            _deadCounter = 0;
            foreach (var a in _agents)
            {
                a.Respawn();
                if (_won) a.SetActions(_bestActions);
            }
            _bestReplayer.Respawn();
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