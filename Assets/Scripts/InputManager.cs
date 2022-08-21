using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    private bool _lastJump = false;
    private bool _lastDash = false;

    public CharacterController character;
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int x = 0;
        int y = 0;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            x += 1;
        }else if (Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            y += 1;
        }else if (Input.GetKey(KeyCode.DownArrow))
        {
            y -=  1;
        }
        
        if (Input.GetKey(KeyCode.Z) && !_lastJump)
        {
            character.Jump();
        }
        if (Input.GetKey(KeyCode.X) && !_lastDash)
        {
            character.Dash();
        }
        _lastJump = Input.GetKey(KeyCode.Z);
        _lastDash = Input.GetKey(KeyCode.X);
        character.SetDirection(x, y);
    }
}