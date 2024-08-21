using System;
using UnityEngine;

public class ArtilleryFiringSystem : MonoBehaviour {
    public GameObject bombPrefab;
    public float      launchAngle     = 45f;
    public float      projectileSpeed = 20f;


    public GameObject target;

    private LineRenderer trajectoryLine;


    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Fire();
        }
    }

    public void FireAtTarget(Vector3 targetPosition) {
        Vector3 startPosition = transform.position;

        // Calculate initial velocity
        Vector3 initialVelocity = ProjectileSystem.Instance.CalculateInitialVelocity(
            startPosition, targetPosition, launchAngle, projectileSpeed);

        // Calculate trajectory points
        Vector3[ ] trajectoryPoints
            = ProjectileSystem.Instance.CalculateTrajectoryPoints(startPosition, initialVelocity);

        // Create or update trajectory line
        if (trajectoryLine == null) {
            trajectoryLine = ProjectileSystem.Instance.CreateTrajectoryLine(trajectoryPoints, Color.red, 0.1f);
        }
        else {
            trajectoryLine.positionCount = trajectoryPoints.Length;
            trajectoryLine.SetPositions(trajectoryPoints);
        }

        // Instantiate and launch the bomb
        GameObject bomb = Instantiate(bombPrefab, startPosition, Quaternion.identity);
        StartCoroutine(ProjectileSystem.Instance.MoveProjectile(bomb, startPosition, initialVelocity));
    }

    // Call this method when you want to fire the canon
    public void Fire() {
        // Vector3 targetPosition = GetTargetPosition(); // Your method to determine target
        FireAtTarget(target.transform.position);
    }
}
