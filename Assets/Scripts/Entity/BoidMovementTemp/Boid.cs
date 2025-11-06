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

    [SerializeField] private float _magnitureThreshold = 2f;
    [SerializeField] private float _period = 0.3f;
    private Vector3 _recentMovementInPeriod;
    private Vector3 _positionLastFrame;
    [SerializeField] private bool wasNullInPeriod = false;
    private Rigidbody _rb;

    public BoidData Data => _data;
    public BoidMovement Movement => _boidMovement;
    public Vector3 Position { get; private set; }

    private Vector3 forwardVec;

    private Vector3 target = Vector3.zero;

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
        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        _rb.angularVelocity = Vector3.zero;
        if (target != Vector3.zero && Vector3.Distance(transform.position, target) > stopDist)
        {
            Vector3 direction = (target - transform.position).normalized;
            //direction.y = 0;
            _boidMovement.Move(direction * Data.forwardMovementSpeed);
        }

        if(Mathf.Abs(_acceleration.y) < 0.3)
        {
            _acceleration.y = 0;
        }

        if (Physics.Raycast(_avoidance.getOffsetPosition() + Vector3.up, Vector3.down, out RaycastHit hit, 2.5f, groundMask))
        {
            Vector3 movementThisFrame = transform.position - _positionLastFrame;
            _positionLastFrame = transform.position;

            LoadMovementInPeriod(movementThisFrame);
            //Debug.Log(_recentMovementInPeriod.magnitude + " < " + _magnitureThreshold);
            if (_recentMovementInPeriod.magnitude < _magnitureThreshold && !wasNullInPeriod)
            {
                _boidMovement.OnJump();
                StartCoroutine(_avoidance.DelayPush((target - transform.position).normalized * Data.forwardMovementSpeed * 6, 0.2f));
                //Debug.Log("YAY!" + _recentMovementInPeriod.magnitude);
            }

            //var dir = _velocity.normalized;
            //var speed = _velocity.magnitude;
            //speed = Mathf.Clamp(speed, _data.minSpeed, _data.maxSpeed);
            //_velocity = dir * speed;
            //(Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(transform.up, hit.normal), 1.25f))


            _velocity += _acceleration;
            transform.position += (_velocity) * Time.deltaTime;
            //Debug.Log(_velocity + " | " + ((Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(transform.up, hit.normal), 1.5f)) * _velocity));
        }
        else
        {
            _velocity += _acceleration;
            transform.position += _velocity * Time.deltaTime;
        }

        var lookDir = Vector3.zero;
        if (Mathf.Abs(transform.position.x) <= _data.limitX + 1f && Mathf.Abs(transform.position.x) >= _data.limitX - 0.1f)
        {
            lookDir = Vector3.SmoothDamp(_previousLookDir, Vector3.forward, ref _lookDirRef, 0.2f);
        }
        else
        {
            lookDir = Vector3.SmoothDamp(_previousLookDir, new Vector3(_velocity.x, 0f, _velocity.z), ref _lookDirRef, 0.2f);
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
    }


    private void LoadMovementInPeriod(Vector3 movementThisFrame)
    {
        _recentMovementInPeriod += movementThisFrame;
        if (movementThisFrame.magnitude < _magnitureThreshold/4)
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
}
