using System;
using System.Collections;
using System.Collections.Generic;
using EVP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;


public class AiVehicleController : MonoBehaviour {
    [SerializeField] private NavMeshAgent navigator;
    private GameObject detector;
    [BoxGroup("Ref")] [SerializeField] private VehicleController _vehicleController;

    [BoxGroup("Ref")] [SerializeField] private Transform PositionToGo;


    internal bool isMoving;
    internal bool isReversing;
    internal float VehicleStuckresetTime = 0f;

    /// <summary>
    /// if multiple units move the tolerance should be higher so they won't keep trying to get to the exact point
    /// </summary>
    public float finalDestinationTolerance = 1.2f;

    private void Awake() {
    }

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

    private void OnTriggerEnter(Collider other) {
    }

    private void Update() {
        if (isMoving) {
            navigator.transform.localPosition =  Vector3.zero;
            navigator.transform.localPosition += Vector3.forward * _vehicleController.wheels[0].wheelTransform.transform.localPosition.z;
        }
    }

    internal bool tryDriveTo(Vector3 position, float DestinationTolerance = 10f) {
        if (NavMesh.SamplePosition(position, out NavMeshHit hits, 5.0f, NavMesh.AllAreas)) {
            navigator.SetDestination(hits.position);
            if (_vehicleController.brakeInput > 0)
                _vehicleController.brakeInput = 0;

            isMoving                  = true;
            finalDestinationTolerance = DestinationTolerance;
        }

        //todo check if this ok
        //Stop();
        return false;
    }


    private void FixedUpdate() {
        //handling navigatio

        if (!isMoving)
            return;

        float navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1.5f, -1f, 1f);
        _vehicleController.steerInput = navigatorInput;

        if (navigator.remainingDistance >= finalDestinationTolerance) {
            if (_vehicleController.brakeInput > 0)
                _vehicleController.brakeInput = 0;

            if (!isReversing) {
                //don't do any throttle changes if the vehicle is reversing or getting unstuck
                if (navigator.remainingDistance >= 10f) {
                    _vehicleController.throttleInput = 1f;
                }
                else {
                    _vehicleController.throttleInput = 0.5f;
                }
            }
        }
        else {
            //reached destination

            isMoving                         = false;
            _vehicleController.throttleInput = 0.0f;
            _vehicleController.brakeInput    = 1f;
        }


        // Debug.Log($"Speed: ${_vehicleController.speed}");

        
        Resetting();
    }


    #region Helpers

    
 
    
    public struct VehicleSensonsObstacleData {
        public bool front;
        public bool front_right;
        public bool front_left;

    }
    VehicleSensonsObstacleData VehicleSensors() {
        RaycastHit[] hits       = new RaycastHit[3]; // Array to store raycast hits
        RaycastHit[] Fronthit   = new RaycastHit[1]; //front
        RaycastHit[] FrontRight = new RaycastHit[1]; //front right
        RaycastHit[] FrontLeft  = new RaycastHit[1]; //front left


        VehicleSensonsObstacleData data = new VehicleSensonsObstacleData();
        // Raycast directions
        Vector3 forward      = transform.TransformDirection(Vector3.forward);
        Vector3 forwardLeft  = Quaternion.Euler(0, -40f, 0) * forward;
        Vector3 forwardRight = Quaternion.Euler(0, 40f, 0) * forward;

        Vector3 pos = transform.position;


        if (Physics.RaycastNonAlloc(pos, forward, Fronthit, 8f) > 0) {
            Debug.DrawRay(transform.position, forward * Fronthit[0].distance, Color.red);
            data.front = true;
        }
        else {
            Debug.DrawRay(pos, forward * 8f, Color.green);
        }

        if (Physics.RaycastNonAlloc(pos, forwardRight, FrontRight, 8f) > 0) {
            Debug.DrawRay(transform.position, forwardRight * FrontRight[0].distance, Color.red);
            data.front_right = true;
        }
        else {
            Debug.DrawRay(pos, forwardRight * 8f, Color.green);
        }


        if (Physics.RaycastNonAlloc(pos, forwardLeft, FrontLeft, 8f) > 0) {
            Debug.DrawRay(transform.position, forwardLeft * FrontRight[0].distance, Color.red);
            data.front_left = true;
        }
        else {
            Debug.DrawRay(pos, forwardLeft * 8f, Color.green);
        }

        return data;
    }


    void Resetting() {
        // If unable to move forward, puts the gear to R.


        float carSpeed = Mathf.Abs(_vehicleController.speed);

        if (carSpeed <= 0.1f) {
            
            VehicleStuckresetTime += Time.deltaTime;

            
            
        }

        if (VehicleStuckresetTime >= 2f) {
            isReversing = true;
            _vehicleController.throttleInput = -1f;
            
            var dt = VehicleSensors();

            if (dt.front_left) _vehicleController.steerInput = 1;
            if (dt.front_right) _vehicleController.steerInput = -1;
            
        }
        if (VehicleStuckresetTime >= 5.5) {
            if (isReversing) {
                _vehicleController.throttleInput = 1f;
                VehicleStuckresetTime            = 0;
                isReversing                      = false;
            }
        }
        

        // Rest of your existing code for reversing and recovery logic...
    }


    public void Stop() {
        isMoving                         = false;
        _vehicleController.brakeInput    = 1;
        _vehicleController.throttleInput = 0f;
        _vehicleController.steerInput    = 0f;
    }

    #endregion
}