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
    public int MinActionTime;
    public int MaxActionTime;

    public float maxMutationChance;
    public float minMutationChance;

    public int populationSize;
    public int delayStart;
    public GameObject player;

    private float _bestFitness = -999;
    private List<int> _bestActions;
    private int _bestMutStartInd = 0;
    private int _bestRandStartInd = 0;

    private CharacterController _bestReplayer;

    public bool hideAgents;


    public float startMaxTime;
    public float increaseMaxTimeBy;
    private float _maxTime;
    private float _roundStart;

    private int _deadCounter = 0;


    private bool _won = false;

    private List<CharacterController> _agents = new List<CharacterController>();


    private bool _resetLock = false;

    // Start is called before the first frame update
    void Start()
    {
        // Time.timeScale = 2f;
        Transform t = transform;


        GameObject best = Instantiate(player, t.position, t.rotation);
        _bestReplayer = best.GetComponent<CharacterController>();
        _bestReplayer.manager = this;
        _bestReplayer.humanPlayer = false;
        _bestReplayer.SetActions(Mutate(_bestReplayer.GetActions(), 0, 0));

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
        _maxTime = startMaxTime + increaseMaxTimeBy * _bestFitness;
        if (Time.realtimeSinceStartup - _roundStart > _maxTime)
        {
            foreach (var a in _agents)
            {
                if (!a.dead) a.Kill();
            }
        }
    }

    public void Report(CharacterController c)
    { 
        if (_won && c != _bestReplayer) return; // stupid patch for reset bug that I can't figure out the cause of
        else if (_won)
        {        
            _deadCounter++;

            // Debug.Log("Triggering win reset");
            ResetGame();
        }
        
        _deadCounter++;
        if (c == _bestReplayer)
        {
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
            if (_won)
            {
                Debug.Log("OUI OUI");
                ResetGame();
                return;
            }
            // print("-----");
            // print(_bestMutStartInd);
            // print(_bestRandStartInd);
        }


        if (Time.realtimeSinceStartup < delayStart)
        {
            //Create a new random list before timer expires
            c.SetActions(new List<int>());
            return;
        }

        // if (_won || _deadCounter >= populationSize)

        if (_deadCounter == populationSize)
        {
            if(!_resetLock) Debug.Log("hevy iz ded");
            
            ResetGame();
        }
    }

    private void ResetGame()
    {
        if (_resetLock) return;
        

        _resetLock = true;
        _deadCounter = 0;

        
        



        if (!_won)
        {
            List<int> ba = Mutate(_bestActions, _bestMutStartInd, _bestRandStartInd);
            _bestReplayer.SetActions(ba);
            _bestReplayer.Respawn();
            foreach (var a in _agents)
            {
                if (!a.dead) a.Kill();
            }

            foreach (var a in _agents)
            {
                List<int> mutActions = Mutate(_bestActions, _bestRandStartInd, _bestRandStartInd);
                a.SetActions(mutActions);
                a.Respawn();
            }
        }
        else
        {
            foreach (var a in _agents)
            {
                a.sprite.enabled = false;
                // a.Respawn();
                a.dead = true;
            }
            _bestReplayer.SetActions(_bestActions);
            _bestReplayer.Respawn();
        }


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


        _roundStart = Time.realtimeSinceStartup;
        _resetLock = false;
    }


    public int[] GenerateActionPair()
    {
        int time = (int)(Random.value * (MaxActionTime - MinActionTime)) + MinActionTime;
        int action = (int)(Random.value * NumChoices);
        return new int[] { time, action };
    }

    private List<int> Mutate(List<int> actions, int semiRandomStart, int completeRandomStart)
    {
        var mutChance = (maxMutationChance - minMutationChance) * Random.value + minMutationChance;
        var mutActions = new List<int>();

        for (int i = 0; i < actions.Count; i += 2)
        {
            var roll = Random.value;
            if (i > completeRandomStart) roll = 0;

            if (i < semiRandomStart || roll > mutChance)
            {
                mutActions.Add(actions[i]);
                mutActions.Add(actions[i + 1]);
                continue;
            }


            int[] newSet = GenerateActionPair();
            mutActions.Add(newSet[0]);
            mutActions.Add(newSet[1]);
        }


        while (mutActions.Count < 999)
        {
            int[] n = GenerateActionPair();
            mutActions.Add(n[0]);
            mutActions.Add(n[1]);
        }

        return mutActions;
    }
}