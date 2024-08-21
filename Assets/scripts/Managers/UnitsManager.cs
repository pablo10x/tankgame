using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Formation {
    Line,
    Circle,
    Wedge,
    Square,
    V,
    EchelonRight,
    EchelonLeft,
    Column,
    StaggeredColumn,
    Diamond,
    CustomRandom
}

namespace Managers {
    public class UnitsManager : Singleton<UnitsManager> {
        public Dictionary<Collider, Unit> RegistredUnits = new ();

        public List<Unit> selectedUnits = new ();

        public Formation currentFormation = Formation.Line;
        public float     formationSpacing = 2f;

        public LayerMask unitLayer;
        public LayerMask terrainLayer;


        [ BoxGroup("Custom formation") ] public float customRandomBaseAreaWidth     = 20f;
        [ BoxGroup("Custom formation") ] public float customRandomBaseAreaLength    = 20f;
        [ BoxGroup("Custom formation") ] public float customRandomSpacingMultiplier = 1.5f;
        [ BoxGroup("Custom formation") ] public float customRandomMinDistance       = 2f;
        [ BoxGroup("Custom formation") ] public int?  customRandomSeed              = 20;

        void Update() {
            HandleInput();
        }


        private void Start() {
            RegisterUnitsInEditor();
        }



        public void DestroyUnit(Unit _unit) {
            if (RegistredUnits.ContainsKey(_unit._UnitCollider)) {
                RegistredUnits.Remove(_unit._UnitCollider);
                
            }

            if (selectedUnits.Contains(_unit)) {
                selectedUnits.Remove(_unit);
            }
            
            
        }
        
        
        void HandleInput() {
            if (Input.GetMouseButtonDown(1)) // Left click
            {
                SelectUnit();
            }
            else
                if (Input.GetMouseButtonDown(0)) // Right click
                {
                    MoveSelectedUnits();
                }

            // Formation hotkeys
            if (Input.GetKeyDown(KeyCode.F1)) SetFormation(Formation.Line);
            if (Input.GetKeyDown(KeyCode.F2)) SetFormation(Formation.Circle);
            if (Input.GetKeyDown(KeyCode.F3)) SetFormation(Formation.Wedge);
            if (Input.GetKeyDown(KeyCode.F4)) SetFormation(Formation.Square);
        }

        void SelectUnit() {
            Ray        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitLayer)) {
                if (RegistredUnits.TryGetValue(hit.collider, out Unit unit)) {
                    if (unit != null) {
                        if (!Input.GetKey(KeyCode.LeftShift)) // Clear selection if Shift is not held
                        {
                            selectedUnits.Clear();
                        }

                        if (!selectedUnits.Contains(unit)) {
                            selectedUnits.Add(unit);
                        }
                    }
                }


                // Unit unit = hit.collider.GetComponent<Unit>();
            }
        }

        void MoveSelectedUnits() {
            if (selectedUnits.Count < 1) return;

            Ray        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer)) {
                Vector3[ ] formationPositions = GetFormationPositions(hit.point);

                //get unit widh to use as formation spacing

                formationSpacing = selectedUnits[0].agent.radius * customRandomSpacingMultiplier;


                for (int i = 0; i < selectedUnits.Count; i ++) {
                    // selectedUnits[i].SetDestination(formationPositions[i]);
                    selectedUnits[i].SetDestination(( formationPositions[i] ));
                }
            }
        }

        public void SetFormation(Formation formation) {
            currentFormation = formation;
        }

        private Vector3[ ] GetVFormation(Vector3 center) {
            Vector3[ ] positions      = new Vector3[selectedUnits.Count];
            float      angle          = 60f * Mathf.Deg2Rad; // 60 degree angle for V shape
            Vector3    rightDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3    leftDirection  = new Vector3(Mathf.Cos(angle), 0, - Mathf.Sin(angle));

            positions[0] = center; // Leader at the front


            for (int i = 1; i < selectedUnits.Count; i ++) {
                if (i % 2 == 0) {
                    positions[i] = center + rightDirection * ( i / 2 ) * formationSpacing;
                }
                else {
                    positions[i] = center + leftDirection * ( ( i + 1 ) / 2 ) * formationSpacing;
                }
            }

            return positions;
        }

        private Vector3[ ] GetEchelonFormation(Vector3 center, bool isRight) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];
            float      angle     = 45f * Mathf.Deg2Rad; // 45 degree angle for echelon
            Vector3 direction = isRight
                ? new Vector3(Mathf.Cos(angle), 0, - Mathf.Sin(angle))
                : new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            for (int i = 0; i < selectedUnits.Count; i ++) {
                positions[i] = center + direction * i * formationSpacing;
            }

            return positions;
        }

        private Vector3[ ] GetColumnFormation(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];

            for (int i = 0; i < selectedUnits.Count; i ++) {
                positions[i] = center + Vector3.back * i * formationSpacing;
            }

            return positions;
        }

        private Vector3[ ] GetStaggeredColumnFormation(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];

            for (int i = 0; i < selectedUnits.Count; i ++) {
                float xOffset = ( i % 2 == 0 ) ? formationSpacing / 2 : - formationSpacing / 2;
                positions[i] = center + Vector3.back * i * formationSpacing + Vector3.right * xOffset;
            }

            return positions;
        }

        private Vector3[ ] GetDiamondFormation(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];
            positions[0] = center + Vector3.forward * formationSpacing; // Front

            if (selectedUnits.Count > 1)
                positions[1] = center + Vector3.back * formationSpacing; // Back

            if (selectedUnits.Count > 2)
                positions[2] = center + Vector3.left * formationSpacing; // Left

            if (selectedUnits.Count > 3)
                positions[3] = center + Vector3.right * formationSpacing; // Right

            // For additional units, place them in expanding diamond layers
            for (int i = 4; i < selectedUnits.Count; i ++) {
                int   layer    = ( i - 4 ) / 4 + 2;
                int   position = ( i - 4 ) % 4;
                float distance = formationSpacing * layer;

                switch (position) {
                    case 0:
                        positions[i] = center + Vector3.forward * distance;
                        break;
                    case 1:
                        positions[i] = center + Vector3.back * distance;
                        break;
                    case 2:
                        positions[i] = center + Vector3.left * distance;
                        break;
                    case 3:
                        positions[i] = center + Vector3.right * distance;
                        break;
                }
            }

            return positions;
        }

        private Vector3[ ] GetFormationPositions(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];

            switch (currentFormation) {
                case Formation.Line:
                    return GetLineFormation(center);
                case Formation.Circle:
                    return GetCircleFormation(center);
                case Formation.Wedge:
                    return GetWedgeFormation(center);
                case Formation.Square:
                    return GetSquareFormation(center);
                case Formation.V:
                    return GetVFormation(center);
                case Formation.EchelonRight:
                    return GetEchelonFormation(center, true);
                case Formation.EchelonLeft:
                    return GetEchelonFormation(center, false);
                case Formation.Column:
                    return GetColumnFormation(center);
                case Formation.StaggeredColumn:
                    return GetStaggeredColumnFormation(center);
                case Formation.Diamond:
                    return GetDiamondFormation(center);
                case Formation.CustomRandom:
                    // Default values, you can adjust these or expose them as properties
                    // Set to a specific value for reproducible randomness
                    return GetCustomRandomFormation(center, customRandomBaseAreaWidth, customRandomBaseAreaLength,
                        customRandomSpacingMultiplier, customRandomSeed);
                default:
                    return GetLineFormation(center);
            }
        }

        private Vector3[ ] GetLineFormation(Vector3 center) {
            Vector3[ ] positions  = new Vector3[selectedUnits.Count];
            float      totalWidth = ( selectedUnits.Count - 1 ) * formationSpacing;
            Vector3    start      = center - ( Vector3.right * totalWidth * 0.5f );

            for (int i = 0; i < selectedUnits.Count; i ++) {
                positions[i] = start + ( Vector3.right * i * formationSpacing );
            }

            return positions;
        }


        private Vector3[ ] GetCustomRandomFormation(Vector3 center,
                                                    float   baseAreaWidth,
                                                    float   baseAreaLength,
                                                    float   spacingMultiplier,
                                                    int?    seed = null
        ) {
            Vector3[ ]    positions = new Vector3[selectedUnits.Count];
            System.Random random    = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            // Calculate the average NavMesh radius of the units
            float avgRadius = selectedUnits.Average(unit => unit.agent.radius);

            // Adjust area size based on unit count and average radius
            float adjustedAreaWidth  = Mathf.Max(baseAreaWidth, Mathf.Sqrt(selectedUnits.Count) * avgRadius * 4);
            float adjustedAreaLength = Mathf.Max(baseAreaLength, Mathf.Sqrt(selectedUnits.Count) * avgRadius * 4);

            // Set minimum distance based on average radius and spacing multiplier
            float minDistance = avgRadius * 2 * spacingMultiplier;

            List<Vector3> occupiedPositions = new List<Vector3>();

            for (int i = 0; i < selectedUnits.Count; i ++) {
                Vector3   randomPosition;
                bool      positionFound = false;
                int       attempts      = 0;
                const int maxAttempts   = 100;

                do {
                    float randomX = (float)( random.NextDouble() - 0.5 ) * adjustedAreaWidth;
                    float randomZ = (float)( random.NextDouble() - 0.5 ) * adjustedAreaLength;
                    randomPosition = center + new Vector3(randomX, 0, randomZ);

                    positionFound = !occupiedPositions.Any(pos => Vector3.Distance(pos, randomPosition) < minDistance);

                    attempts ++;
                    if (attempts >= maxAttempts) {
                        Debug.LogWarning(
                            "Couldn't find a suitable position for all units. Using last generated position.");
                        break;
                    }
                } while (!positionFound);

                positions[i] = randomPosition;
                occupiedPositions.Add(randomPosition);
            }

            return positions;
        }

        private Vector3[ ] GetCircleFormation(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];
            float      angleStep = 360f / selectedUnits.Count;

            for (int i = 0; i < selectedUnits.Count; i ++) {
                float   angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 pos   = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * formationSpacing;
                positions[i] = center + pos;
            }

            return positions;
        }

        private Vector3[ ] GetWedgeFormation(Vector3 center) {
            Vector3[ ] positions = new Vector3[selectedUnits.Count];
            int        rowCount  = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));

            int unitIndex = 0;
            for (int row = 0; row < rowCount; row ++) {
                int   unitsInRow = Mathf.Min(row + 1, selectedUnits.Count - unitIndex);
                float rowWidth   = ( unitsInRow - 1 ) * formationSpacing;

                for (int col = 0; col < unitsInRow; col ++) {
                    float x = col * formationSpacing - rowWidth * 0.5f;
                    float z = - row * formationSpacing;
                    positions[unitIndex] = center + new Vector3(x, 0, z);
                    unitIndex ++;

                    if (unitIndex >= selectedUnits.Count) break;
                }

                if (unitIndex >= selectedUnits.Count) break;
            }

            return positions;
        }

        private Vector3[ ] GetSquareFormation(Vector3 center) {
            Vector3[ ] positions  = new Vector3[selectedUnits.Count];
            int        sideLength = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));
            float      offset     = ( sideLength - 1 ) * formationSpacing * 0.5f;

            for (int i = 0; i < selectedUnits.Count; i ++) {
                int   row = i / sideLength;
                int   col = i % sideLength;
                float x   = col * formationSpacing - offset;
                float z   = row * formationSpacing - offset;
                positions[i] = center + new Vector3(x, 0, z);
            }

            return positions;
        }


        [ FoldoutGroup("Testing") ]
        [ Button("Register Units") ]
        private void RegisterUnitsInEditor() {
            var units = FindObjectsByType<Collider>(FindObjectsSortMode.InstanceID);

            int c = 0;
            foreach (Collider col in units) {
                var p = col.GetComponent<Unit>();
                if (p) {
                    if (!RegistredUnits.ContainsKey(col)) {
                        c ++;
                        RegistredUnits.Add(col, p);
                    }
                }
            }

           
        }
    }
}
