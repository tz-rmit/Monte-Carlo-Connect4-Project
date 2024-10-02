using UnityEngine;
using SteeringCalcs;
using Globals;
using System.Linq;
using System.Linq.Expressions;

public class Frog : MonoBehaviour
{
    // Frog status.
    public int Health;
    public int MaxFlies;
    public int FliesCaught;

    // Steering parameters.
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;
    public float WaterDrag;

    // The arrival radius is set up to be dynamic, depending on how far away
    // the player right-clicks from the frog. See the logic in Update().
    public float ArrivePct;
    public float MinArriveRadius;
    public float MaxArriveRadius;
    private float _arriveRadius;

    // Turn this off to make it easier to see overshooting when seek is used
    // instead of arrive.
    public bool HideFlagOnceReached;

    // Prefab for the shootable bubble projectile.
    public GameObject bubblePrefab;

    // References to various objects in the scene that we want to be able to modify.
    private Transform _flag;
    private SpriteRenderer _flagSr;
    private DrawGUI _drawGUIScript;
    private Animator _animator;
    private Rigidbody2D _rb;

    // Stores the last position that the player right-clicked. Initially null.
    private Vector2? _lastClickPos;

    // For pathfinding
    private Node[] path;
    private int pathIndex;

    void Start()
    {
        // Initialise the various object references.
        _flag = GameObject.Find("Flag").transform;
        _flagSr = _flag.GetComponent<SpriteRenderer>();
        _flagSr.enabled = false;

        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager != null)
        {
            _drawGUIScript = uiManager.GetComponent<DrawGUI>();
        }

        _animator = GetComponent<Animator>();

        _rb = GetComponent<Rigidbody2D>();

        _lastClickPos = null;
        _arriveRadius = MinArriveRadius;
    }

    void Update()
    {
        // Check whether the spacebar was pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bubble = Instantiate(bubblePrefab, transform.position + transform.up.normalized, Quaternion.identity);
            bubble.GetComponent<Rigidbody2D>().velocity = _rb.velocity + 5.0f * (Vector2)transform.up.normalized;
        }

        // Check whether the player right-clicked (mouse button #1).
        if (Input.GetMouseButtonDown(1))
        {
            pathIndex = 0;

            _lastClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _flag.position = (Vector2)_lastClickPos + new Vector2(0.55f, 0.55f);
            _flagSr.enabled = true;

            // Set the arrival radius dynamically.
            _arriveRadius = Mathf.Clamp(ArrivePct * ((Vector2)_lastClickPos - (Vector2)transform.position).magnitude, MinArriveRadius, MaxArriveRadius);

            path = Pathfinding.RequestPath(transform.position, (Vector2)_lastClickPos);

            // Change the world position of the final path node to the actual clicked position,
            // since the centre of the final node might be off somewhat.
            if (path.Length > 0)
            {
                Node fixedFinalNode = path[path.Length - 1].Clone();
                fixedFinalNode.worldPosition = (Vector2)_lastClickPos;
                path[path.Length - 1] = fixedFinalNode;
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 desiredVel = Vector2.zero;

        // If the last-clicked position is non-null, move there. Otherwise do nothing.
        if (_lastClickPos != null)
        {
            // Draw the path found by the A* algorithm.
            if (path.Length > 0)
            {
                for (int i = 1; i < path.Length; i++)
                {
                    Debug.DrawLine(path[i - 1].worldPosition, path[i].worldPosition, Color.black);
                }
            }

            if (pathIndex < path.Length && (path[pathIndex].worldPosition - (Vector2)gameObject.transform.position).magnitude > Constants.TARGET_REACHED_TOLERANCE)
            {
                // TODO: Make it so that the frog follows the A* path instead.
                if ((path.Length - 1) > pathIndex)
                {
                    desiredVel = Steering.BasicSeek(gameObject.transform.position, path[pathIndex].worldPosition, MaxSpeed);
                }
                // Until the last node
                else
                {
                    float radius = Vector2.Distance(gameObject.transform.position, path[pathIndex].worldPosition);
                    desiredVel = Steering.BasicArrive(gameObject.transform.position, path[pathIndex].worldPosition, radius, MaxSpeed);
                }

            }
            else
            {
                pathIndex++;

                if (path.Length <= pathIndex)
                {
                    _lastClickPos = null;
                }

                if (HideFlagOnceReached)
                {
                    _flagSr.enabled = false;
                }
            }
        }

        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        if (_rb.velocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            _animator.SetBool("Walking", true);
            transform.up = _rb.velocity;
        }
        else
        {
            _animator.SetBool("Walking", false);
        }
    }

    public void EnterWater()
    {
        _rb.drag = WaterDrag;
    }

    public void ExitWater()
    {
        _rb.drag = 0.0f;
    }

    public void TakeDamage()
    {
        if (Health > 0)
        {
            Health--;
        }

        if (Health <= 0)
        {
            _drawGUIScript.HandleGameOver(false);
        }
    }

    public void CatchFly()
    {
        FliesCaught++;

        if (FliesCaught >= MaxFlies)
        {
            _drawGUIScript.HandleGameOver(true);
        }
    }
}
