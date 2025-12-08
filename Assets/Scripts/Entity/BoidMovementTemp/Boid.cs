using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UseEntity;

[RequireComponent(typeof(BoidMovement))]
public class Boid : Entity
{
    [SerializeField] BoidData _data;
    [SerializeField] BoidMovement _boidMovement;
    [SerializeField] BoidAvoidance _avoidance;
    [SerializeField] float stopDist = 1f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _acceleration;

    private Vector3 _previousLookDir;
    private Vector3 _lookDirRef;
    [SerializeField] private bool lookFlat;

    [SerializeField] private float _period = 0.3f;
    private Vector3 _recentMovementInPeriod;
    private Vector3 _positionLastFrame;
    public bool wasNullInPeriod = false;
    [SerializeField] private bool hasNewTarget = false;
    private Rigidbody _rb;
    public BoidData Data => _data;
    public bool HasNewTarget => hasNewTarget;
    public BoidMovement Movement => _boidMovement;
    public BoidAvoidance Avoidance => _avoidance;
    public Vector3 Position { get; private set; }

    private Vector3 forwardVec;

    public Vector3 target = Vector3.zero;
    public float StopDist => stopDist;

    public Vector3 GetVelocity() => _velocity;

    private void Awake()
    {
        _boidMovement = GetComponent<BoidMovement>();
        _avoidance = GetComponent<BoidAvoidance>();
        Position = transform.position;
        forwardVec = transform.forward;
        _positionLastFrame = transform.position;

        _rb = GetComponent<Rigidbody>();
    }

    public void Steer(Vector3 direction, float weight)
    {
        var steer = direction.normalized * _data.maxSpeed - _velocity;
        _acceleration += Vector3.ClampMagnitude(steer, _data.maxSteerForce) * weight;
    }

    public void Steer(Vector3 direction, float weight, float speed)
    {
        var steer = direction.normalized * speed - _velocity;
        _acceleration += Vector3.ClampMagnitude(steer, _data.maxSteerForce) * weight;
    }

    public void Steer(Vector3 velocity)
    {
        _acceleration += velocity;
    }

    private void Update()
    {
        if(Mathf.Abs(_acceleration.y) < 0.3)
        {
            _acceleration.y = 0;
        }

        _velocity += _acceleration;
        //transform.position += (_velocity) * Time.deltaTime;
        _rb.linearVelocity = _velocity;

        Vector3 movementThisFrame = transform.position - _positionLastFrame;
        _positionLastFrame = transform.position;

        LoadMovementInPeriod(movementThisFrame);

        var lookDir = Vector3.zero;
        if (Mathf.Abs(transform.position.x) <= _data.limitX + 1f && Mathf.Abs(transform.position.x) >= _data.limitX - 0.1f)
        {
            lookDir = Vector3.SmoothDamp(_previousLookDir, Vector3.forward, ref _lookDirRef, 0.2f);
        }
        else if (lookFlat)
        {
            lookDir = Vector3.SmoothDamp(_previousLookDir, new Vector3(_velocity.x, 0f, _velocity.z), ref _lookDirRef, 0.2f);
        }
        else
        {
            lookDir = Vector3.SmoothDamp(_previousLookDir, new Vector3(_velocity.x, _velocity.y, _velocity.z), ref _lookDirRef, 0.2f);
        }
        transform.LookAt(transform.position + lookDir);
        _previousLookDir = lookDir;

        _velocity = _acceleration = Vector3.zero;

        Position = transform.position;
    }

    public void SetVelocity(Vector3 velocity)
    {
        _velocity = velocity;
    }
    public void SetTarget(Vector3 target)
    {
        this.target = target;
        hasNewTarget = true;
        Invoke("killNewTarget", 1f);
    }

    public void MoveToTarget()
    {
        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        _rb.angularVelocity = Vector3.zero;
        if (target != Vector3.zero && Vector3.Distance(transform.position, target) > stopDist)
        {
            Vector3 direction = (target - transform.position).normalized;
            //direction.y = 0;
            _boidMovement.Move(direction * Data.forwardMovementSpeed);
        }
    }
private void killNewTarget()
    {
        Debug.Log("lostNewTarget");
        hasNewTarget= false;
    }

    private void LoadMovementInPeriod(Vector3 movementThisFrame)
    {
        _recentMovementInPeriod += movementThisFrame;
        if (movementThisFrame.magnitude < 0.01f)
        {
            wasNullInPeriod = true;
            StartCoroutine(RemoveMovementInPeriodNull(_period, movementThisFrame));
        }
        StartCoroutine(RemoveMovementInPeriod(_period, movementThisFrame));
    }

    private IEnumerator RemoveMovementInPeriod(float delay, Vector3 removeMovement)
    {
        yield return new WaitForSeconds(delay);

        _recentMovementInPeriod -= removeMovement;
    }
    private IEnumerator RemoveMovementInPeriodNull(float delay, Vector3 removeMovement)
    {
        yield return new WaitForSeconds(delay);

        _recentMovementInPeriod -= removeMovement;
        wasNullInPeriod = false;
    }

    public Vector3 getRecentMovement() => _recentMovementInPeriod;
}
