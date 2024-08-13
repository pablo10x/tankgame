using System;
using System.Collections;
using System.Collections.Generic;
using EVP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;


public class AiVehicleController : MonoBehaviour {
    [ SerializeField ] private                     NavMeshAgent      navigator;
    private                                        GameObject        detector;
    [ BoxGroup("Ref") ] [ SerializeField ] private VehicleController _vehicleController;

    [ BoxGroup("Ref") ] [ SerializeField ] private Transform PositionToGo;


    [ BoxGroup("Steering Helper") ] public float SteeringHelperRayDistance = 20f;
    [ BoxGroup("Steering Helper") ] public float SteeringValue             = 0.3f;

    public bool isMoving;
    public bool isReversing;


    public float _stuckTimeThreshold = 2f;
    public float _stuckTimer         = 0;


    protected bool BrakingDuetoObstacle = false;


    [ SerializeField ] private Collider VehicleCollider;


    /// <summary>
    /// if multiple units move the tolerance should be higher so they won't keep trying to get to the exact point
    /// </summary>
    public float finalDestinationTolerance = 1.2f;

    private void Awake() { }

    // Creating our Navigator and setting properties.
    private void Start() {
        navigator.transform.SetParent(transform, false);

        // navigator                       = navigatorObject.AddComponent<NavMeshAgent>();
        // navigator.radius                = 1;
        // navigator.speed                 = 1;
        // navigator.angularSpeed          = 100000f;
        // navigator.acceleration          = 100000f;
        // navigator.height                = 1;
        // navigator.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
        // navigator.avoidancePriority     = 99;

        // Creating our Detector and setting properties. Used for getting nearest target gameobjects.
        detector = new GameObject("Detector");
        detector.transform.SetParent(transform, false);
        detector.gameObject.AddComponent<SphereCollider>();
        detector.GetComponent<SphereCollider>().isTrigger = true;
        detector.GetComponent<SphereCollider>().radius    = 10f;
    }


    private void Update() {
        //navigator object fix
        navigator.transform.localPosition = Vector3.zero;


        if (_vehicleController.throttleInput >= 0) {
            navigator.transform.localPosition += Vector3.forward *
                                                 _vehicleController.wheels[0].wheelTransform.transform.localPosition
                                                                   .z;
        }
        else {
            navigator.transform.localPosition
                += Vector3.back * _vehicleController.wheels[0].wheelTransform.transform.localPosition.z;
        }
    }

    internal bool tryDriveTo(Vector3 position, float DestinationTolerance = 10f) {
        if (NavMesh.SamplePosition(position, out NavMeshHit hits, 5.0f, NavMesh.AllAreas)) {
            navigator.SetDestination(hits.position);
            if (_vehicleController.brakeInput > 0)
                _vehicleController.brakeInput = 0;

            if (navigator.isStopped) {
                navigator.isStopped = false;
            }

            isMoving                  = true;
            finalDestinationTolerance = DestinationTolerance;


            if (IsPositionBehindVehicle(hits.position) && Vector3.Distance(transform.position, hits.position) < 20f) {
                _vehicleController.Direction = 0;
            }
            else
                _vehicleController.Direction = 1;
        }

        return false;
    }

    public float VehicleSensorsSteeringHelper(float steerValue, float maxRayDistance, out float Throttle) {
        RaycastHit[ ] Fronthit   = new RaycastHit[1]; //front
        RaycastHit[ ] FrontRight = new RaycastHit[1]; //front right
        RaycastHit[ ] FrontLeft  = new RaycastHit[1]; //front left


        Throttle = 1;


        // Raycast directions
        Vector3 forward      = transform.TransformDirection(Vector3.forward);
        Vector3 forwardLeft  = Quaternion.Euler(0, - 35f, 0) * forward;
        Vector3 forwardRight = Quaternion.Euler(0, 35f, 0) * forward;

        Vector3 pos = transform.position;

        float steerInput = 0f;


        if (_vehicleController.speed <= 0.3f && !isReversing) {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= _stuckTimeThreshold) {
                _vehicleController.Direction = - 1;
                isReversing                  = true;
            }
        }
        else {
            
            
            
            
            if (isReversing && _vehicleController.speed > 0.2f) {
                isReversing                  = false;
                _vehicleController.Direction = 1;
                _stuckTimer                  = 0f;
            }
        }


        // if (Physics.RaycastNonAlloc(pos, forward, Fronthit, maxRayDistance, LayerMask.GetMask("AI_VEHICLE")) > 0 &&
        //     Fronthit[0].collider != VehicleCollider && _vehicleController.Direction == 1) {
        //     Debug.DrawRay(transform.position, forward * Fronthit[0].distance, Color.yellow);
        //     //reduce speed 
        //
        //     _vehicleController.brakeInput = 1f;
        //     
        //
        //
        //     // steerInput = - Mathf.Sign(_vehicleController.steerInput) * steerValue;
        // }
        // else {
        //     _vehicleController.brakeInput = 0.0f;
        // }

        // //use this ray just to brake
        // if (Physics.RaycastNonAlloc(pos, forward, Fronthit, maxRayDistance) > 0) {
        //     Debug.DrawRay(transform.position, forward * Fronthit[0].distance, Color.yellow);
        //     //reduce speed 
        //
        //     
        //     if(Fronthit[0].collider.tag.Contains(""))
        //     if (!BrakingDuetoObstacle) {
        //         _vehicleController.brakeInput = 0.5f;
        //         BrakingDuetoObstacle          = true;
        //     }
        //
        //     // steerInput = - Mathf.Sign(_vehicleController.steerInput) * steerValue;
        // }
        // else {
        //     if (_vehicleController.brakeInput >= 0.5f)
        //         _vehicleController.brakeInput -= 0.5f;
        //
        //     if (BrakingDuetoObstacle)
        //         BrakingDuetoObstacle = false;
        // }

        if (Physics.RaycastNonAlloc(pos, forwardRight, FrontRight, maxRayDistance) > 0) {
            Debug.DrawRay(transform.position, forwardRight * FrontRight[0].distance, Color.red);
            float distance    = FrontRight[0].distance;
            float steerFactor = Mathf.Clamp01(distance / maxRayDistance);
            steerInput += Mathf.Lerp(0f, - steerValue, steerFactor);
        }

        if (Physics.RaycastNonAlloc(pos, forwardLeft, FrontLeft, maxRayDistance) > 0) {
            Debug.DrawRay(transform.position, forwardLeft * FrontLeft[0].distance, Color.red);
            float distance    = FrontLeft[0].distance;
            float steerFactor = Mathf.Clamp01(distance / maxRayDistance);
            steerInput += Mathf.Lerp(0f, steerValue, steerFactor);
        }


        return steerInput;
    }

    private void FixedUpdate() {
        //handling navigatio

        if (!isMoving) {
            return;
        }

        float fixedSteeringInput
            = VehicleSensorsSteeringHelper(SteeringValue, SteeringHelperRayDistance, out var throttleInput);

        float navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1.5f,
            - 1f, 1f);
        _vehicleController.steerInput = Mathf.Clamp(navigatorInput + fixedSteeringInput, - 1, 1);


        // _vehicleController.throttleInput = throttleInput;
        SetThrottle(throttleInput);


        if (navigator.remainingDistance >= 0.2f) {
            //don't do any throttle changes if the vehicle is reversing or getting unstuck
            if (navigator.remainingDistance <= finalDestinationTolerance) {
                Stop();
            }
        }


        //Resetting();
    }


    // void Resetting() {
    //     // If unable to move forward, puts the gear to R.
    //
    //
    //     if (isStuck) {
    //         VehicleStuckresetTime += Time.deltaTime;
    //     }
    //
    //     if (VehicleStuckresetTime >= 2f) {
    //         isReversing                      = true;
    //         _vehicleController.throttleInput = - 1f;
    //         Debug.Log($"stuck:: reversing now");
    //     }
    //
    //     if (VehicleStuckresetTime >= 3) {
    //         if (isStuck) {
    //             Debug.Log($"Vehicle still stuck");
    //         }
    //         else {
    //             VehicleStuckresetTime = 0;
    //             isReversing           = false;
    //
    //             Debug.Log($"Vehicle is good now");
    //         }
    //
    //         // if (isReversing) {
    //         //     _vehicleController.throttleInput = 1f;
    //         //     VehicleStuckresetTime            = 0;
    //         //     isReversing                      = false;
    //         // }
    //     }
    //
    //
    //     if (VehicleStuckresetTime >= 10) {
    //         Debug.Log("stuck for long time");
    //     }
    // }

    public bool IsPositionBehindVehicle(Vector3 position) {
        Vector3 forward  = transform.forward;
        Vector3 toTarget = position - transform.position;

        float angle = Vector3.Angle(forward, toTarget);

        // Adjust the angle threshold as needed
        return angle > 90;
    }


    protected bool UnitsNearby = false;


    void Stop() {
        navigator.isStopped              = true;
        isMoving                         = false;
        _vehicleController.brakeInput    = 1f;
        _vehicleController.throttleInput = 0;
    }

    private void SetThrottle(float throttle) {
        if (_vehicleController.Direction == 1) _vehicleController.throttleInput = throttle;
        else _vehicleController.throttleInput                                   = - throttle;
    }
}
