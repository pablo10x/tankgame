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
    internal float VehicleStillStuckTimer = 0f;


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

    internal bool tryDriveTo(Vector3 position) {
        if (NavMesh.SamplePosition(position, out NavMeshHit hits, 5.0f, NavMesh.AllAreas)) {
            navigator.SetDestination(hits.position);
            if (_vehicleController.brakeInput > 0)
                _vehicleController.brakeInput = 0;

            isMoving = true;
            return true;
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

        if (navigator.remainingDistance > 1.5f) {
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

    void Resetting() {
        // If unable to move forward, puts the gear to R.

        float carspeed = Mathf.Abs(_vehicleController.speed);


        // if (carspeed <= 0.2f && transform.InverseTransformDirection(_vehicleController.cachedRigidbody.velocity).z < 1f) {
        //     resetTime += Time.deltaTime;
        // }


        if (carspeed <= 0.2f || VehicleStuckresetTime >= 2) {
            VehicleStuckresetTime += Time.deltaTime;
        }

        if (VehicleStuckresetTime >= 2.5) {
            _vehicleController.throttleInput = -1f;
            _vehicleController.steerInput    = 0f;
            isReversing                      = true;
        }


        if (VehicleStuckresetTime >= 5.5) {
            if (isReversing) {
                _vehicleController.throttleInput = 1f;
                VehicleStuckresetTime            = 0;
                isReversing                      = false;
            }
        }
    }


    public void Stop() {
        isMoving                         = false;
        _vehicleController.brakeInput    = 1;
        _vehicleController.throttleInput = 0f;
        _vehicleController.steerInput    = 0f;
    }

    #endregion
}