using System;
using System.Collections;
using System.Collections.Generic;
using EVP;
using TMPro;
using UnityEngine;

public class tempuiDebug : MonoBehaviour {
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text vehicleData;


    [SerializeField] private VehicleController   car;
    [SerializeField] private AiVehicleController _aiVehicleController;


    private void FixedUpdate()
    {
        speed.text = $"Speed: {car.speed}\n" +
                     $"Moving: {_aiVehicleController.isMoving}\n" +
                     $"ResetTime: {_aiVehicleController.VehicleStuckresetTime}\n" +
                     $"Reverse ?: {_aiVehicleController.isReversing}";
        vehicleData.text = $"Throttle: <color=red> {car.throttleInput}</color>\nBrake: {car.brakeInput}\nHandBrake: {car.handbrakeInput}";
    }
}