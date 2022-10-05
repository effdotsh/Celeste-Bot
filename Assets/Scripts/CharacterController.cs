using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CharacterController : MonoBehaviour
{
    public float id;
    public bool trailEnabled;
    public TrailRenderer normalTrail;
    public TrailRenderer dashTrail;

    public Sprite normalSprite;
    public Sprite dashSprite;


    public float standDist = 0.54f;

    public Rigidbody2D rb;
    public Collider2D collider;

    public float speedX;


    private float _fitness = 0f;
    private ArrayList _scored = new ArrayList();

    private float _targetVelX = 0f;
    private float _targetVelY = 0f;
    private float _targetVelXChoice = 0f;
    private float _targetVelYChoice = 0f;

    private float _velX = 0;

    public float accelerationX;


    public float jumpAmount = 35;
    public float gravityScale = 10;
    public float fallingGravityScale = 40;

    public int dashLength;
    public float dashSpeed;

    private float _facing = 1;

    private bool _hasDash = false;

    private int _dashCounter = 0;
    private float _dashSpeedX = 0;
    private float _dashSpeedY = 0;

    public SpriteRenderer sprite;


    private float _spawnX;
    private float _spawnY;

    private int _moveDisableTimer = 0;
    public float wallJumpX;
    public float wallJumpY;
    public int wallJumpTime;

    public float springForce;

    public PopulationManager manager;


    private List<int> _actions = new List<int>(); //[t1, a1, t2, a2, ...]

    private float lastSpring = -999;
    public float springStun;

    private int _actionCounter = 0;
    private int _frameCounter = 0;
    public bool humanPlayer = true;


    private int prevCheckpointActionNumber = 0;
    private int curCheckpointActionNumber = 0;


    private List<float> _breakBlockStartTimes = new List<float>();
    private List<Collider2D> _breakBlockColliders = new List<Collider2D>();
    public float _breakBlockLag;

    public bool dead = false;

    public bool won = false;


    void Awake()
    {
        GiveDash();
        Vector3 t = transform.position;
        _spawnX = t.x;
        _spawnY = t.y;


        IgnoreOthers("Player");
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

    private void UpdatePhysics()
    {
        //physics
        _moveDisableTimer--;
        if (WallSliding() && _moveDisableTimer < wallJumpTime - 5)
        {
            _moveDisableTimer = -1;
        }

        if (_dashCounter > 0)
        {
            rb.velocity = new Vector2(_dashSpeedX, _dashSpeedY);
            _dashCounter--;

            if (_dashCounter <= 0)
            {
                Vector2 cur = rb.velocity;
                rb.velocity = new Vector2(cur.x, cur.y / 3);
            }
        }
        else
        {
            if (rb.velocity.y >= 0)
            {
                rb.gravityScale = gravityScale;
            }
            else if (rb.velocity.y < 0)
            {
                rb.gravityScale = fallingGravityScale;
            }

            if (Standing())
            {
                sprite.sprite = normalSprite;
                if (trailEnabled)
                {
                    normalTrail.sortingOrder = 4;
                    dashTrail.sortingOrder = 3;
                }

                _hasDash = true;
            }


            float dx = _targetVelX * speedX - _velX;


            rb.AddForce(new Vector2(accelerationX * _targetVelX, 0), ForceMode2D.Impulse);


            Vector2 cur = rb.velocity;

            if (Mathf.Abs(cur.x) > speedX)
            {
                rb.velocity = new Vector2(speedX * _targetVelX, cur.y);
            }

            if (_targetVelX == 0)
            {
                rb.velocity = new Vector2(cur.x / 1.3f, cur.y);
            }
        }
    }
    // Update is called once per frame

    private void AIControl()
    {
        if (humanPlayer) return;
        if (_actionCounter + 1 >= _actions.Count)
        {
            Kill();

            return;
        }


        _frameCounter += 1;

        int action = _actions[_actionCounter + 1];
        switch (action)
        {
            case 0:
                SetDirection(0, 0);

                break;
            case 1:
                Jump();
                break;
            case 2:
                SetDirection(-1, 0);
                break;
            case 3:
                SetDirection(1, 0);
                break;
            case 4:
                SetDirection(-1, -1);
                Dash();
                break;
            case 5:
                SetDirection(-1, 0);
                Dash();
                break;
            case 6:
                SetDirection(-1, 1);
                Dash();
                break;
            case 7:
                SetDirection(0, -1);
                Dash();
                break;
            case 8:
                SetDirection(0, 1);
                Dash();
                break;
            case 9:
                SetDirection(1, -1);
                Dash();
                break;
            case 10:
                SetDirection(1, 0);
                Dash();
                break;
            case 11:
                SetDirection(1, 1);
                Dash();
                break;
        }

        if (_frameCounter >= _actions[_actionCounter])
        {
            _actionCounter += 2;
            _frameCounter = 0;
        }
    }


    public void CheckBreakableBlocks()
    {
        for (int i = 0; i < _breakBlockColliders.Count; i++)
        {
            if (Time.realtimeSinceStartup - _breakBlockStartTimes[i] > _breakBlockLag)
            {
                Physics2D.IgnoreCollision(_breakBlockColliders[i], collider);
                Vector2 t = transform.position;
                transform.position = new Vector2(t.x, t.y + 0.001f);
            }
        }
    }

    public void NewStart()
    {
        prevCheckpointActionNumber = _actionCounter;
        curCheckpointActionNumber = _actionCounter;
    }

    void FixedUpdate()
    {
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        s.flipX = _targetVelX < 0;
        if (dead)
        {
            rb.velocity = new Vector2(0, 0);
            return;
        }

        CheckBreakableBlocks();
        AIControl();
        UpdatePhysics();
    }

    public void SetDirection(float x, float y)
    {
        _targetVelXChoice = x;
        _targetVelYChoice = y;

        if (_moveDisableTimer <= 0)
        {
            _targetVelX = x;

            _targetVelY = y;

            _facing = Mathf.Sign(_targetVelX);
        }
    }

    public void GiveDash()
    {
        _hasDash = true;
        sprite.sprite = normalSprite;
        if (trailEnabled)
        {
            normalTrail.sortingOrder = 4;
            dashTrail.sortingOrder = 3;
        }
    }

    private bool Standing()
    {
        var t = transform;
        RaycastHit2D hit;


        hit = Physics2D.Raycast(t.position - new Vector3(t.localScale.x / 2, 0, 0), -Vector2.up, standDist);
        if (hit.collider != null && (hit.collider.tag.Equals("floor") || hit.collider.tag.Equals("Cloud"))) return true;

        // Debug.DrawLine(t.position, t.position - Vector3.up * standDist, Color.red);

        hit = Physics2D.Raycast(t.position, -Vector2.up, standDist);
        if (hit.collider != null && (hit.collider.tag.Equals("floor") || hit.collider.tag.Equals("Cloud"))) return true;

        hit = Physics2D.Raycast(t.position + new Vector3(t.localScale.x / 2, 0, 0), -Vector2.up, standDist);
        if (hit.collider != null && (hit.collider.tag.Equals("floor") || hit.collider.tag.Equals("Cloud"))) return true;

        return false;
    }

    public void TouchBreakBlock(Collider2D col)
    {
        _breakBlockStartTimes.Add(Time.realtimeSinceStartup);
        _breakBlockColliders.Add(col);
    }

    private bool WallSliding()
    {
        if (Time.realtimeSinceStartup - lastSpring < springStun) return false;
        if (_targetVelX == 0) return false;

        Transform t = transform;
        RaycastHit2D hit;
        hit = Physics2D.Raycast(t.position, Vector2.left, standDist);
        if (hit.collider != null && hit.collider.tag.Equals("floor")) return true;

        hit = Physics2D.Raycast(t.position, Vector2.right, standDist);
        if (hit.collider != null && hit.collider.tag.Equals("floor")) return true;

        return false;
    }

    public void Jump()
    {
        if (Standing())
        {
            Vector2 curVel = rb.velocity;
            rb.velocity = new Vector2(curVel.x, 0);
            rb.AddForce(Vector2.up * jumpAmount, ForceMode2D.Impulse);
        }
        else if (WallSliding() && _moveDisableTimer <= 0)
        {
            // rb.AddForce(Vector2.up * jumpAmount, ForceMode2D.Impulse);
            rb.velocity = new Vector2(-wallJumpX * _targetVelX * speedX, wallJumpY * speedX);

            _moveDisableTimer = wallJumpTime;
            _targetVelX *= -1;
        }
    }


    public void Spring()
    {
        Vector2 curVel = rb.velocity;
        rb.velocity = new Vector2(curVel.x, 0);
        rb.AddForce(Vector2.up * springForce, ForceMode2D.Impulse);
        GiveDash();
        _dashCounter = 0;
        lastSpring = Time.realtimeSinceStartup;
    }

    public void Dash()
    {
        if (!_hasDash) return;

        _targetVelX = _targetVelXChoice;
        _targetVelY = _targetVelYChoice;

        _moveDisableTimer = -1;

        sprite.sprite = dashSprite;
        if (trailEnabled)
        {
            normalTrail.sortingOrder = 3;
            dashTrail.sortingOrder = 4;
        }

        _hasDash = false;
        _dashCounter = dashLength;


        var angle = Mathf.Atan2(_targetVelY, _targetVelX);
        _dashSpeedX = Mathf.Cos(angle) * dashSpeed;
        _dashSpeedY = Mathf.Sin(angle) * dashSpeed;
    }

    public bool HasDash()
    {
        return _hasDash;
    }

    public void Respawn()
    {
        sprite.sprite = normalSprite;

        dead = false;
        rb.simulated = true;
        transform.position = new Vector3(_spawnX, _spawnY, 0);
        _hasDash = true;
        _targetVelX = 0;
        _targetVelY = 0;
        _velX = 0;
        rb.velocity = new Vector2(0, 0);
        _dashCounter = 0;


        _scored = new ArrayList();
        _fitness = 0;

        _actionCounter = 0;
        _frameCounter = 0;


        foreach (Collider2D col in _breakBlockColliders)
        {
            Physics2D.IgnoreCollision(col, collider, false);
        }

        _breakBlockColliders = new List<Collider2D>();
        _breakBlockStartTimes = new List<float>();

        if (trailEnabled)
        {
            // normalTrail.enabled = true;
            // dashTrail.enabled = true;

            normalTrail.sortingOrder = 4;
            dashTrail.sortingOrder = 3;
        }

        prevCheckpointActionNumber = 0;
        curCheckpointActionNumber = 0;
    }

    public void Kill()
    {
        if (trailEnabled)
        {
            // Debug.Log("-----");
            // Debug.Log(_actions.Count);
            // Debug.Log(_actionCounter);
            // Debug.Log(prevCheckpointActionNumber);
            // Debug.Log("------");
        }

        dead = true;
        rb.gravityScale = 0;
        rb.simulated = false;
        if (!humanPlayer) manager.Report(this);
        else
        {
            Respawn();
            GameObject[] balloons = GameObject.FindGameObjectsWithTag("Balloon");
            if (balloons != null)
            {
                foreach (GameObject bal in balloons)
                {
                    Balloon b = bal.GetComponent<Balloon>();
                    b.TriggerReset();
                }
            }
        }

        // dashTrail.enabled = false;
        // normalTrail.enabled = false;
    }

    public int GetActionMutationStart()
    {
        return prevCheckpointActionNumber;
    }

    public int GetCompleteRandStart()
    {
        return curCheckpointActionNumber;
    }

    public void TargetReached(Target t, int retrograde)
    {
        // if (_scored.Contains(t.GetID())) return;
        // _scored.Add(t.GetID());

        if (t.value > _fitness)
        {
            _fitness = t.value;
            if (_hasDash) _fitness += 0.1f;
            _fitness -= 0.000001f * _actionCounter;

            prevCheckpointActionNumber = curCheckpointActionNumber - retrograde;
            curCheckpointActionNumber = _actionCounter - retrograde;


            if (t.acceptAny || (t.withDashOnly && HasDash())) NewStart();
            if (t.isWin)
            {
                print("win");
                won = true;
                Kill();
            }
        }
    }

    public void TargetReached(Target t)
    {
        TargetReached(t, 0);
    }

    public float GetFitness()
    {
        return _fitness;
    }

    public List<int> GetActions()
    {
        return _actions;
    }

    public void SetActions(List<int> a)
    {
        _actions = a;
    }
}