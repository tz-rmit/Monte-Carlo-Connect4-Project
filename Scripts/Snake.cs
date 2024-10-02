using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SteeringCalcs;
using Globals;

public class Snake : MonoBehaviour
{
    public SnakeState State;

    // Obstacle avoidance parameters (see the assignment spec for an explanation).
    public AvoidanceParams AvoidParams;

    // Steering parameters.
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;

    // Use this as the arrival radius for all states where the steering behaviour == arrive.
    public float ArriveRadius;

    // Parameters controlling transitions in/out of the Aggro state.
    public float AggroRange;
    public float DeAggroRange;
    
    // The current target of the snake (see the assignment spec for an explanation).
    private Vector2 _target;

    // The snake's initial position (the target for the PatrolHome and Harmless states).
    private Vector2 _home;

    // The patrol point (the target for the PatrolAway state).
    public Transform PatrolPoint;

    // Reference to the frog (the target for the Aggro state).
    public GameObject Frog;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;

    // Snake FSM states (don't edit this enum).
    public enum SnakeState : int
    {
        PatrolAway = 0,
        PatrolHome = 1,
        Aggro = 2,
        Harmless = 3
    }

    // Snake FSM events (don't edit this enum).
    public enum SnakeEvent : int
    {
        FrogInRange = 0,
        FrogOutOfRange = 1,
        HitFrog = 2,
        ReachedTarget = 3,
        HitByBubble = 4
    }

    // Direction IDs used by the snake animator (don't edit these).
    private enum Direction : int
    {
        Up = 0,
        Left = 1,
        Down = 2,
        Right = 3
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _home = transform.position;

        SetState(SnakeState.PatrolAway);
    }

    void FixedUpdate()
    {
        UpdateTarget();

        Vector2 desiredVel = Vector2.zero;

        switch (State)
        {
            case SnakeState.PatrolAway:
            case SnakeState.PatrolHome:
            case SnakeState.Harmless:
                desiredVel = Steering.Arrive(transform.position, _target, MaxSpeed, ArriveRadius, AvoidParams);
                if (((Vector2)transform.position - _target).magnitude < Constants.TARGET_REACHED_TOLERANCE)
                {
                    HandleEvent(SnakeEvent.ReachedTarget);
                }
                break;

            case SnakeState.Aggro:
                desiredVel = Steering.Seek(transform.position, _target, MaxSpeed, AvoidParams);
                break;
        }

        // Convert the desired velocity to a force, then apply it.
        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        // Throw frog in/out of range events.
        if ((transform.position - Frog.transform.position).magnitude < AggroRange)
        {
            HandleEvent(SnakeEvent.FrogInRange);
        }
        else if ((transform.position - Frog.transform.position).magnitude > DeAggroRange)
        {
            HandleEvent(SnakeEvent.FrogOutOfRange);
        }

        UpdateAppearance();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (State == SnakeState.Aggro && collision.gameObject == Frog)
        {
            HandleEvent(SnakeEvent.HitFrog);
            collision.gameObject.GetComponent<Frog>().TakeDamage();
        }
    }

    void SetState(SnakeState newState)
    {
        if (newState != State)
        {
            // Can uncomment this for debugging purposes.
            //Debug.Log(name + " switching state to " + newState.ToString());

            State = newState;
        }
    }

    // Per the spec, the target of the snake depends on the current FSM state.
    // This logic is already set up for you.
    void UpdateTarget()
    {
        switch (State)
        {
            case SnakeState.PatrolAway:
                _target = PatrolPoint.position;
                break;
            case SnakeState.PatrolHome:
            case SnakeState.Harmless:
                _target = _home;
                break;
            case SnakeState.Aggro:
                _target = Frog.transform.position;
                break;
        }
    }

    public void HandleEvent(SnakeEvent e)
    {
        switch (State)
        {
            case SnakeState.PatrolAway:
                if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolHome);
                }
                else if (e == SnakeEvent.FrogInRange)
                {
                    SetState(SnakeState.Aggro);
                }
                break;

            case SnakeState.PatrolHome:
                if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolAway);
                }
                else if (e == SnakeEvent.FrogInRange)
                {
                    SetState(SnakeState.Aggro);
                }
                break;

            case SnakeState.Aggro:
                if (e == SnakeEvent.HitFrog || e == SnakeEvent.HitByBubble)
                {
                    SetState(SnakeState.Harmless);
                }
                else if (e == SnakeEvent.FrogOutOfRange)
                {
                    SetState(SnakeState.PatrolHome);
                }
                break;

            case SnakeState.Harmless:
                if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolAway);
                }
                break;

            default:
                break;
        }
    }

    private void UpdateAppearance()
    {
        // Update the snake's colour to provide a visual indication of its state.
        switch (State)
        {
            case SnakeState.PatrolAway:
            case SnakeState.PatrolHome:
                _sr.color = new Color(1.0f, 1.0f, 1.0f);
                break;

            case SnakeState.Aggro:
                _sr.color = new Color(1.0f, 0.7f, 0.7f);
                break;

            case SnakeState.Harmless:
                _sr.color = new Color(0.6f, 0.6f, 0.6f);
                break;
        }

        if (_rb.velocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            // Determine the bearing of the snake in degrees (between -180 and 180)
            float angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;

            if (angle > -135.0f && angle <= -45.0f) // Down
            {
                transform.up = new Vector2(0.0f, -1.0f);
                _animator.SetInteger("Direction", (int)Direction.Down);
            }
            else if (angle > -45.0f && angle <= 45.0f) // Right
            {
                transform.up = new Vector2(1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Right);
            }
            else if (angle > 45.0f && angle <= 135.0f) // Up
            {
                transform.up = new Vector2(0.0f, 1.0f);
                _animator.SetInteger("Direction", (int)Direction.Up);
            }
            else // Left
            {
                transform.up = new Vector2(-1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Left);
            }
        }
    }
}
