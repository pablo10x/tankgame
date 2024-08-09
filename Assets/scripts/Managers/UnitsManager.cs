using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour {
    // Start is called before the first frame update

    public List<Unit> SelectedUnits = new List<Unit>();


    public Camera MainCam;

    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) {
            Ray ray = new Ray();
            if (MainCam!= null) {
                #if UNITY_ANDROID && !UNITY_EDITOR
                ray = MainCam.ScreenPointToRay(Input.GetTouch(0).position);
                #else
                ray = MainCam.ScreenPointToRay(Input.mousePosition);
                #endif
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                //check if clicked on unit
                Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();

                if (clickedUnit != null) {
                    //if not selected select it
                    if (SelectedUnits.Contains(clickedUnit)) {
                        clickedUnit.isSelected = false;
                        SelectedUnits.Remove(clickedUnit);
                        clickedUnit.OnDeselect();
                    }
                    else {
                        clickedUnit.isSelected = true;
                        SelectedUnits.Add(clickedUnit);
                        clickedUnit.OnSelect();
                    }
                }
                else {
                    // a location
                    foreach (var unit in SelectedUnits) {
                        if (unit.UnitType == UnitType.Vehicle) {
                            if (!unit.GetComponent<AiVehicleController>().tryDriveTo(hit.point)) {
                                Debug.Log("Can't move there , invalid location");
                            }
                        }
                    }
                }


                //       PositionToGo.position = hit.point;
                //       DriveTo(PositionToGo.position);
            }
        }
    }
}