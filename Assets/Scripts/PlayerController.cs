using System;
using System.Collections.Generic;
using UnityEngine;

internal enum RunDirection { Forward, Backward, Left, Right }

public class PlayerController : MonoBehaviour
{
    // Time it takes for the player to no longer be able to jump (prevents accidents)
    private const float MaxHangTime = 0.5f;
    // Time it takes to get to full speed
    private const float MovementChargeTime = 1.0f;
    
    [SerializeField] private float maxWalkSpeed = 2.0f;
    [SerializeField] private float speedIncrement = 1.0f;
    [SerializeField] private float jumpHeight = 3.0f;

    private Dictionary<KeyCode,Action<bool>> _keyMappings;
    private Dictionary<RunDirection, bool> _activeMovement;
    private Dictionary<RunDirection, float> _activeMovementTime;
    private Rigidbody _rb;
    private float _hangTime;
    private bool _midair = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var keyListener = gameObject.AddComponent<KeyListener>();
        _rb = GetComponent<Rigidbody>();
        _activeMovement = new Dictionary<RunDirection, bool>
        {
            [RunDirection.Forward] = false,
            [RunDirection.Backward] = false,
            [RunDirection.Left] = false,
            [RunDirection.Right] = false
        };
        _activeMovementTime = new Dictionary<RunDirection, float>
        {
            [RunDirection.Forward] = 0,
            [RunDirection.Backward] = 0,
            [RunDirection.Left] = 0,
            [RunDirection.Right] = 0,
        };
        _keyMappings = new Dictionary<KeyCode,Action<bool>>
        {
            [KeyCode.W] = value => _activeMovement[RunDirection.Forward] = value,
            [KeyCode.UpArrow] = value => _activeMovement[RunDirection.Forward] = value,
            [KeyCode.A] = value => _activeMovement[RunDirection.Left] = value,
            [KeyCode.LeftArrow] = value => _activeMovement[RunDirection.Left] = value,
            [KeyCode.S] = value => _activeMovement[RunDirection.Backward] = value,
            [KeyCode.DownArrow] = value => _activeMovement[RunDirection.Backward] = value,
            [KeyCode.D] = value => _activeMovement[RunDirection.Right] = value,
            [KeyCode.RightArrow] = value => _activeMovement[RunDirection.Right] = value,
            [KeyCode.Space] = Jump
        };
        keyListener.onInitialized.AddListener(() =>
        {
            foreach (var k in _keyMappings.Keys)
            {
                if (keyListener.KeyEvents.ContainsKey(k))
                {
                    keyListener.KeyEvents[k].AddListener(value =>
                    {
                        _keyMappings[k].Invoke(value);
                    });      
                }
            } 
        });
    }

    // Update is called once per frame
    private void Update()
    {
        HandleAirTime();
        HandleMovement();
    }

    private void HandleAirTime()
    {
        if (!IsGrounded())
        {
            _hangTime += Time.deltaTime;
            _midair = true;
        }
        else
        {
            _hangTime = 0;
            _midair = false;
        }
    }

    private void HandleMovement()
    {
        var desiredVelocity = Vector3.zero;
        if (_activeMovement[RunDirection.Forward])
        {
            desiredVelocity += Utilities.CubicEaseIn(
                transform.forward * maxWalkSpeed, 
                Vector3.zero, 
                Mathf.Clamp(_activeMovementTime[RunDirection.Forward]/MovementChargeTime,0,1)
            );
            _activeMovementTime[RunDirection.Forward] += Time.deltaTime;
        }
        else
        {
            _activeMovementTime[RunDirection.Forward] = 0;
        }
        
        if (_activeMovement[RunDirection.Backward])
        {
            desiredVelocity += Utilities.CubicEaseIn(
                -transform.forward * maxWalkSpeed, 
                Vector3.zero, 
                Mathf.Clamp(_activeMovementTime[RunDirection.Backward]/MovementChargeTime,0,1)
            );
            _activeMovementTime[RunDirection.Backward] += Time.deltaTime;
        }
        else
        {
            _activeMovementTime[RunDirection.Backward] = 0;
        }
        
        if (_activeMovement[RunDirection.Left])
        {
            desiredVelocity += Utilities.CubicEaseIn(
                -transform.right * maxWalkSpeed, 
                Vector3.zero, 
                Mathf.Clamp(_activeMovementTime[RunDirection.Left]/MovementChargeTime,0,1)
            );
            _activeMovementTime[RunDirection.Left] += Time.deltaTime;
        }
        else
        {
            _activeMovementTime[RunDirection.Left] = 0;
        }
        
        if (_activeMovement[RunDirection.Right])
        {
            desiredVelocity += Utilities.CubicEaseIn(
                transform.right * maxWalkSpeed, 
                Vector3.zero, 
                Mathf.Clamp(_activeMovementTime[RunDirection.Right]/MovementChargeTime,0,1)
            );
            _activeMovementTime[RunDirection.Right] += Time.deltaTime;
        }
        else
        {
            _activeMovementTime[RunDirection.Right] = 0;
        }

        desiredVelocity.y = 0;
        _rb.AddForce(desiredVelocity, ForceMode.Impulse);
    }
    
    private void Jump(bool on)
    {
        if (!on) return;
        if (_hangTime < MaxHangTime) {
            _rb?.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        var rayCollide = Physics.Raycast(transform.position, Vector3.down, out var hitInfo);
        return rayCollide && hitInfo.transform.gameObject.CompareTag("Ground") && hitInfo.distance < 0.1f;
    }
}
