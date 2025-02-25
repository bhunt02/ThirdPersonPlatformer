using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

internal enum RunDirection { Forward, Backward, Left, Right }

public class PlayerController : MonoBehaviour
{
    // Time it takes to get to full speed
    private const float MovementChargeTime = 0.2f;
    // Maximum allowed jumps
    private const int MaxJumpCount = 2;
    // Cooldown between dash actions
    private const float DashCooldown = 2.0f;
    // Time a dash action takes
    private const float MaxDashTime = 0.1f;
    // Speed in a dash action
    private const float DashSpeed = 30.0f;
    
    [SerializeField] private float maxWalkSpeed = 1.0f;
    [SerializeField] private float jumpHeight = 10.0f;
    
    private Dictionary<KeyCode,Action<bool>> _keyMappings;
    private Dictionary<RunDirection, bool> _activeMovement;
    private Dictionary<RunDirection, float> _activeMovementTime;
    
    private Rigidbody _rb;
    
    private bool _isDashing;
    private bool _appliedDash;
    private Vector3 _dashDirection = Vector3.zero;
    private float _dashTime;
    private float _dashCooldown;
    private float _prevDashCooldown;
    
    private readonly UnityEvent<float> _onDashCooldownChanged = new();
    public void AddDashCooldownListener(UnityAction<float> call) => _onDashCooldownChanged.AddListener(call);
    
    private bool _midair;
    private int _jumpCount;
    
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
            [KeyCode.LeftShift] = Dash,
            [KeyCode.RightShift] = Dash,
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
        var mainCamera = Camera.main;
        if (!_isDashing && mainCamera)
        {
            _rb.rotation = Quaternion.LookRotation(new Vector3(mainCamera.transform.forward.x,0,mainCamera.transform.forward.z), Vector3.up);
        }
        
        HandleDashing();
        HandleMovement();
    }

    private void HandleDashing()
    {
        
        if (_isDashing)
        {
            _dashTime += Time.deltaTime;
            if (_dashTime > MaxDashTime)
            {
                _isDashing = false;
            }
        }

        if (_midair)
        {
            _onDashCooldownChanged.Invoke(_dashCooldown);
            _prevDashCooldown = _dashCooldown;
            return;
        }
        
        _dashCooldown -= Time.deltaTime;
        if (_dashCooldown < 0) _dashCooldown = 0;
        
        if (Math.Abs(_dashCooldown) - Math.Abs(_prevDashCooldown) <= 0) 
            _onDashCooldownChanged.Invoke(_dashCooldown);
        
        _prevDashCooldown = _dashCooldown;
    }

    private void HandleMovement()
    {
        var desiredVelocity = Vector3.zero;
        if (_isDashing)
        {
            if (_appliedDash) return;
            _rb.AddForce(_dashDirection * DashSpeed, ForceMode.Impulse);
            _appliedDash = true;
            return;
        }
        
        var directions = new Dictionary<RunDirection, Vector3>
        {
            [RunDirection.Forward] = transform.forward,
            [RunDirection.Backward] = -transform.forward,
            [RunDirection.Left] = -transform.right,
            [RunDirection.Right] = transform.right,
        };
        
        foreach (var direction in directions.Keys)
        {
            if (!_activeMovement.ContainsKey(direction) || !_activeMovementTime.ContainsKey(direction)) continue;
            
            if (_activeMovement[direction])
            {
                desiredVelocity += Utilities.CubicEaseIn(
                    directions[direction] * (maxWalkSpeed), 
                    Vector3.zero, 
                    Mathf.Clamp(_activeMovementTime[direction]/MovementChargeTime,0,1)
                );
                _activeMovementTime[direction] += Time.deltaTime;
            }
            else
            {
                _activeMovementTime[direction] = 0;
            }
        }

        var prevVelocity = _rb.linearVelocity;
        // If the velocity dictated by movement for either x,z directions is 0, then default to Unity friction (velocity loss) behavior
        // Also, lock to maximum walking speed (side effect of dash feature)
        if (Mathf.Abs(desiredVelocity.x) < 0.01f)
        {
            desiredVelocity.x = Mathf.Sign(prevVelocity.x) * Mathf.Min(Mathf.Abs(prevVelocity.x),maxWalkSpeed) / (_midair ? 1.01f : 1f);
        }
        if (Mathf.Abs(desiredVelocity.z) < 0.01f)
        {
            desiredVelocity.z = Mathf.Sign(prevVelocity.z) * Mathf.Min(Mathf.Abs(prevVelocity.z),maxWalkSpeed) / (_midair ? 1.01f : 1f);
        }
        
        _rb.linearVelocity = new Vector3(desiredVelocity.x, prevVelocity.y, desiredVelocity.z);
    }

    private void Dash(bool on)
    {
        if (!on || _isDashing || _dashCooldown > 0) return;
        _dashTime = 0;
        _appliedDash = false;
        _isDashing = true;
        _dashDirection = transform.forward;
        _dashCooldown = DashCooldown;
    }
    
    private void Jump(bool on)
    {
        if (!on) return;
        switch (_jumpCount)
        {
            // If player's jumped the allowed number of times (2), cancel
            case >= MaxJumpCount:
                return;
            // Otherwise, initiate a jump and increment the count
            default:
                _jumpCount++;
                _rb?.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                break;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && collision.transform.position.y < transform.position.y)
        {
            _midair = false;
            _jumpCount = 0;
            _dashCooldown = 0;
        } 
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && collision.transform.position.y < transform.position.y)
        {
            _midair = false;
            _jumpCount = 0;
        } 
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _midair = true;
        }
    }
}
