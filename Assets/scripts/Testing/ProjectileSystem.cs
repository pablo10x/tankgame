using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileSystem : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public float defaultLaunchAngle = 45f;
    public float defaultProjectileSpeed = 20f;
    public float gravity = 9.81f;

    [Header("Trajectory Visualization")]
    public int trajectoryResolution = 30;
    public float maxTrajectoryTime = 5f;

    private static ProjectileSystem instance;
    public static ProjectileSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ProjectileSystem>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ProjectileSystem");
                    instance = obj.AddComponent<ProjectileSystem>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Calculates the initial velocity needed to hit a target.
    /// </summary>
    /// <param name="startPosition">Starting position of the projectile</param>
    /// <param name="targetPosition">Target position to hit</param>
    /// <param name="launchAngle">Angle of launch in degrees</param>
    /// <param name="projectileSpeed">Speed of the projectile</param>
    /// <returns>Initial velocity vector</returns>
    public Vector3 CalculateInitialVelocity(Vector3 startPosition, Vector3 targetPosition, float launchAngle = -1f, float projectileSpeed = -1f)
    {
        launchAngle = launchAngle < 0 ? defaultLaunchAngle : launchAngle;
        projectileSpeed = projectileSpeed < 0 ? defaultProjectileSpeed : projectileSpeed;

        Vector3 toTarget = targetPosition - startPosition;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude;
        float y = toTarget.y;
        float angleRad = launchAngle * Mathf.Deg2Rad;

        float v2 = (gravity * x * x) / (2 * (y - Mathf.Tan(angleRad) * x) * Mathf.Pow(Mathf.Cos(angleRad), 2));
        float v = Mathf.Sqrt(Mathf.Abs(v2));

        Vector3 result = toTargetXZ.normalized * v * Mathf.Cos(angleRad);
        result.y = v * Mathf.Sin(angleRad);

        return result.normalized * projectileSpeed;
    }

    /// <summary>
    /// Calculates points along the projectile's trajectory.
    /// </summary>
    /// <param name="startPosition">Starting position of the projectile</param>
    /// <param name="initialVelocity">Initial velocity of the projectile</param>
    /// <returns>Array of Vector3 points representing the trajectory</returns>
    public Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity)
    {
        Vector3[] points = new Vector3[trajectoryResolution];
        Vector3 position = startPosition;
        Vector3 velocity = initialVelocity;
        float timeStep = maxTrajectoryTime / trajectoryResolution;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            points[i] = position;

            position += velocity * timeStep;
            velocity += Vector3.down * gravity * timeStep;

            if (position.y < 0)
            {
                System.Array.Resize(ref points, i + 1);
                break;
            }
        }

        return points;
    }

    /// <summary>
    /// Creates a LineRenderer component and sets it up with the given points.
    /// </summary>
    /// <param name="points">Array of points to draw the line through</param>
    /// <param name="color">Color of the line</param>
    /// <param name="width">Width of the line</param>
    /// <returns>The created LineRenderer component</returns>
    public LineRenderer CreateTrajectoryLine(Vector3[] points, Color color, float width = 0.1f)
    {
        GameObject lineObj = new GameObject("TrajectoryLine");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.startWidth = width;
        line.endWidth = width;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        return line;
    }

    /// <summary>
    /// Moves a projectile along a calculated trajectory.
    /// </summary>
    /// <param name="projectile">The projectile GameObject to move</param>
    /// <param name="startPosition">Starting position of the projectile</param>
    /// <param name="initialVelocity">Initial velocity of the projectile</param>
    /// <returns>IEnumerator for use with StartCoroutine</returns>
    public IEnumerator MoveProjectile(GameObject projectile, Vector3 startPosition, Vector3 initialVelocity)
    {
        Vector3 position = startPosition;
        Vector3 velocity = initialVelocity;
        float timeStep = Time.fixedDeltaTime;

        while (true)
        {
            position += velocity * timeStep;
            velocity += Vector3.down * gravity * timeStep;

            projectile.transform.position = position;

            if (position.y < 0)
            {
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}