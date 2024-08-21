using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public interface IDamageable {
    void TakeDamage(int amount);
    void Heal(int       amount);
    bool IsAlive();
}

public interface IAttackable {
    float AttackPerHit { get; }
    void  Attack(IDamageable target);
    float AttackCooldown { get; set; }
    bool  isAttacking    { get; set; }
}

public interface IMoveable {
    void Move(Vector3 position);
    void Stop();
}


public interface IDeathHandler {
    void OnDeath();
}

public interface ISelectable {
    void OnSelect();
    void OnDeselect();
}


//unit types

public enum UnitType {
    unknown,
    Infantry,
    Vehicle,
    Aircraft,
    Structure,
    Turret,
    Hero,
}

public enum UnitFaction {
    UFN,  //United Federation of Nations
    EUDF, // European Union Defense Force
    MEC,  //Middle Eastern Coalition
}


public enum unitState {
    Idle,
    Moving,
    Guarding,
    Attacking,
    Capturing
}


[ Serializable ]
public abstract class Unit : MonoBehaviour, ISelectable {
    public    int   MaxHealth;
    public    int   Health;
    protected int   BaseArmor;
    protected float MovementSpeed;


    protected unitState CurrentState;

    public bool isSelected = false;


    private   Vector3 targetPosition;
    protected bool    isMoving = false;
    private   Vector3 agentOffsetFromCharacter;
    protected Unit    CurrentlyAttacking = null;

    protected float   rotationThreshold = 5f; // Smaller angle for a tighter turn before moving
    protected bool    isRotating        = false;
    protected Vector3 lastMovementDirection;


    [ BoxGroup("Unit Data") ] public UnitData _UnitData;
    [ BoxGroup("Unit Data") ] public Collider _UnitCollider;

    [ BoxGroup("Unit Data") ] public int   teamID;
    [ BoxGroup("Unit Data") ] public Color teamColor;


    private bool stationary => _UnitData.IsStationary;

    [ HideIf("stationary") ]
    public NavMeshAgent agent;

    public void SetUpUnit() {
        MaxHealth     = _UnitData.MaxHealth;
        Health        = _UnitData.Health;
        BaseArmor     = _UnitData.Armor;
        MovementSpeed = _UnitData.MovementSpeed;
    }

    protected virtual void Start() {
        SetUpUnit();
        if (!_UnitData.IsStationary) {
            if (agent == null) {
                agent = GetComponentInChildren<NavMeshAgent>();
            }

            if (agent != null) {
                //agent.updatePosition = false;
                //agent.updateRotation = false;
                // Store the initial offset between the agent and the car
                agentOffsetFromCharacter = agent.transform.localPosition;
            }
        }
    }

    protected virtual void Update() {
        // UpdateAgentPosition();
        //
        // if (isMoving) {
        //     Vector3 movement = CalculateMovement();
        //
        //     if (IsDestinationInFacingAngle(movement, _UnitData.unitMaxRotationAngle)) {
        //         MoveTowardsTarget(movement);
        //         RotateTowardsTarget(movement);
        //     }
        //     else {
        //         RotateTowardsTarget(movement);
        //     }
        // }

        UpdateAgentPosition();

        if (isMoving) {
            Vector3 movement = CalculateMovement();


            if (_UnitData.UnitType == UnitType.Vehicle) {
                if (isRotating) {
                    RotateTowardsTarget(movement);
                    if (IsDestinationInFacingAngle(movement, rotationThreshold)) {
                        isRotating = false;
                    }
                }
                else {
                    if (!IsDestinationInFacingAngle(movement, _UnitData.unitMaxRotationAngle)) {
                        isRotating            = true;
                        lastMovementDirection = movement;
                    }
                    else {
                        MoveTowardsTarget(movement);
                        RotateTowardsTarget(movement);
                    }
                }
            }
            else {
                MoveTowardsTarget(movement);
                RotateTowardsTarget(movement);
            }
        }
    }

    protected Vector3 CalculateMovement() {
        if (agent == null) return Vector3.zero;

        Vector3 desiredMovement = agent.desiredVelocity.normalized;
        Vector3 avoidance       = CalculateAvoidanceVector();
        return ( desiredMovement + avoidance ).normalized;
    }

    protected void UpdateAgentPosition() {
        if (agent != null) {
            // Update the agent's position to maintain its relative position to the car
            agent.transform.position = transform.TransformPoint(agentOffsetFromCharacter);
            // Update the agent's rotation to match the car's rotation
            agent.transform.rotation = transform.rotation;
        }
    }

    public void SetDestination(Vector3 destination) {
        if (agent != null) {
            agent.SetDestination(destination);
            targetPosition = destination;
            isMoving       = true;
        }
    }

    protected Vector3 CalculateAvoidanceVector() {
        var avoidanceVector = Vector3.zero;
        Collider[ ] nearbyObstacles
            = Physics.OverlapSphere(transform.position, _UnitData.avoidanceRadius, _UnitData.obstacleLayer);

        foreach (Collider obstacle in nearbyObstacles) {
            Vector3 directionToObstacle = obstacle.ClosestPoint(transform.position) - transform.position;
            float   distance            = directionToObstacle.magnitude;

            if (distance < _UnitData.avoidanceRadius) {
                avoidanceVector -= directionToObstacle.normalized * ( _UnitData.avoidanceRadius - distance ) /
                                   _UnitData.avoidanceRadius;
            }
        }

        return avoidanceVector.normalized * _UnitData.avoidanceForce;
    }


    protected void MoveTowardsTarget(Vector3 movement) {
        if (agent == null) return;

        if (agent.remainingDistance <= _UnitData.stoppingDistance) {
            isMoving = false;
            return;
        }

        Vector3 movementStep = movement * ( _UnitData.MovementSpeed * Time.deltaTime );
        transform.position += movementStep;
    }


    protected void RotateTowardsTarget(Vector3 movement) {
        if (movement != Vector3.zero) {
            //impl old: 
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                _UnitData.rotationSpeed * Time.deltaTime);

            // Vector3 rotationTarget = isRotating ? lastMovementDirection : movement;
            // if (rotationTarget != Vector3.zero) {
            //     Quaternion targetRotation = Quaternion.LookRotation(rotationTarget);
            //     transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            //         _UnitData.rotationSpeed * Time.deltaTime);
            // }
        }
    }


    protected bool IsDestinationInFacingAngle(Vector3 movement, float maxAngle = 40f) {
        Vector3 forward = transform.forward;
        float   angle   = Vector3.Angle(forward, movement);
        return angle <= maxAngle;
    }

    public bool HasReachedDestination() {
        return !isMoving;
    }

    public abstract void TakeDamage(int amount);
    public abstract void Heal(int       amount);

    public abstract void Attack(IDamageable target);


    public abstract void OnDeath();

    public abstract void OnStateChanged(unitState oldstate, unitState newstate);


    public void SetState(unitState state) {
        OnStateChanged(CurrentState, state);
    }

    public void OnSelect() { }

    public void OnDeselect() { }

    public bool IsAlive() => Health > 0;
}
