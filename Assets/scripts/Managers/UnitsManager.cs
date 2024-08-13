using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers {
    public class UnitsManager : MonoBehaviour {
        // Start is called before the first frame update

        public List<Unit> SelectedUnits = new ();

        private static readonly RaycastHit[ ] s_Hits = new RaycastHit[1];
        public                  Camera        MainCam;


        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) {
                Ray ray = new Ray();
                if (MainCam != null) {
#if UNITY_ANDROID && !UNITY_EDITOR
                ray = MainCam.ScreenPointToRay(Input.GetTouch(0).position);
#else
                    ray = MainCam.ScreenPointToRay(Input.mousePosition);
#endif
                }

                // Perform non-allocating raycast
                if (Physics.RaycastNonAlloc(ray, s_Hits, Mathf.Infinity, LayerMask.GetMask("AI_VEHICLE")) > 0) {
                    RaycastHit hit = s_Hits[0]; // Get the first hit

                    // Check if clicked on a unit
                    Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
                    if (clickedUnit != null) {
                        // Toggle unit selection
                        if (SelectedUnits.Contains(clickedUnit)) {
                            DeselectUnit(clickedUnit);
                        }
                        else {
                            SelectUnit(clickedUnit);
                        }
                    }
                }

                if (Physics.RaycastNonAlloc(ray, s_Hits, Mathf.Infinity, LayerMask.GetMask("Ground")) > 0) {
                    RaycastHit hit = s_Hits[0]; // Get the first hit
                    // Handle click on location
                    HandleLocationClick(hit.point);
                }
            }
        }

        private void SelectUnit(Unit unit) {
            unit.isSelected = true;
            SelectedUnits.Add(unit);
            unit.OnSelect();
        }

        private void DeselectUnit(Unit unit) {
            unit.isSelected = false;
            SelectedUnits.Remove(unit);
            unit.OnDeselect();
        }

        private void HandleLocationClick(Vector3 location) {
            var selectedUnitCount = SelectedUnits.Count;
            foreach (Unit unit in SelectedUnits) {
                if (unit.UnitType == UnitType.Vehicle) {
                    
                    unit.Move(location);
                    // AiVehicleController vehicleController = unit.GetComponent<AiVehicleController>();
                    // if (vehicleController != null &&
                    //     !vehicleController.tryDriveTo(location, selectedUnitCount + 5f)) { }
                }
            }
        }
    }
}
