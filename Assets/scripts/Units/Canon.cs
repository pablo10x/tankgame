using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Managers;
using UnityEngine.Serialization;

public class Canon : Unit, IDeathHandler, IDamageable {
    [ SerializeField ] private GameObject topCanon;
    [ SerializeField ] private GameObject missilePrefab;
    [ SerializeField ] private Transform  missileSpawnPoint;


    public LayerMask unitsLayer;
    public float     attackDamage             = 20f;
    public float     attackRange              = 50f;
    public float     attackCooldown           = 5f;
    public float     canonRotationSpeed       = 2f;
    public float     canonRotationSpeedonIdle = 2f;

    private float     lastAttackTime;
    private bool      isCanonLoaded = true;
    private Coroutine attackCoroutine;

    public Unit  currentlTarget;
    public float missileFlightTime = 2f;
    public float missileSpeed      = 20f;

    //effects


    public float explosionRadius = 5f;

    //random rotation
    private float lastRandomRotationTime;
    private float randomRotationInterval = 5f; // Time between random rotations when idle
    private bool  xisRotating            = false;

    protected override void Start() {
        base.Start();

        _UnitData.IsStationary = true;
        SetState(unitState.Idle);
    }

    protected override void Update() {
        base.Update();
        if (currentlTarget == null || !currentlTarget.IsAlive()) {
            HandleIdleState();
        }
        else
            if (isCanonLoaded && Time.time - lastAttackTime >= attackCooldown) {
                StartAttackSequence();
            }
    }

    private void HandleIdleState() {
        if (SetNewTarget()) {
            isRotating = false;
        }
        else
            if (!isRotating && Time.time - lastRandomRotationTime >= randomRotationInterval) {
                RotateCanonRandomly();
            }
    }

    private bool SetNewTarget() {
        currentlTarget = null;
        Collider[ ] colliders       = Physics.OverlapSphere(transform.position, attackRange, unitsLayer);
        float       closestDistance = float.MaxValue;

        foreach (Collider col in colliders) {
            if (UnitsManager.Instance.RegistredUnits.TryGetValue(col, out Unit unit) && unit.IsAlive()) {
                float distance = Vector3.Distance(unit.transform.position, transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    currentlTarget  = unit;
                }
            }
        }

        return currentlTarget != null;
    }

    private void StartAttackSequence() {
        isCanonLoaded = false;
        RotateCanonTowardTarget(() => {
            if (currentlTarget != null && currentlTarget.IsAlive()) {
                FireMissile();
            }
            else {
                isCanonLoaded = true;
            }
        });
    }


    private void FireMissile() {
        GameObject missileObj = Instantiate(missilePrefab, missileSpawnPoint.position, missileSpawnPoint.rotation);
        Missile    missile    = missileObj.GetComponent<Missile>();
        if (missile != null) {
            missile.Initialize(currentlTarget.transform, OnMissileImpact);
        }

        lastAttackTime = Time.time;
        StartCoroutine(ReloadCanon());
    }

    private IEnumerator ReloadCanon() {
        yield return new WaitForSeconds(attackCooldown);
        isCanonLoaded = true;
    }

    private void OnMissileImpact(Vector3 impactPosition) {
        Collider[ ] colliders = Physics.OverlapSphere(impactPosition, explosionRadius, unitsLayer);
        foreach (Collider col in colliders) {
            if (UnitsManager.Instance.RegistredUnits.TryGetValue(col, out Unit unit) && unit.IsAlive()) {
                unit.TakeDamage((int)attackDamage);
            }
        }
    }


    private void RotateCanonTowardTarget(Action onComplete) {
        if (currentlTarget == null) return;

        Vector3    direction         = currentlTarget.transform.position - transform.position;
        Quaternion targetRotation    = Quaternion.LookRotation(direction, transform.up);
        Vector3    targetEulerAngles = targetRotation.eulerAngles;
        Vector3 finalRotation
            = new Vector3(transform.localEulerAngles.x, targetEulerAngles.y, transform.localEulerAngles.z);

        topCanon.transform.DOLocalRotate(finalRotation, canonRotationSpeed).onComplete += () => onComplete?.Invoke();
    }

    private void RotateCanonRandomly() {
        if (isRotating) return;

        isRotating = true;
        float randomYRotation = UnityEngine.Random.Range(0f, 360f);
        Vector3 randomRotation
            = new Vector3(transform.localEulerAngles.x, randomYRotation, transform.localEulerAngles.z);

        topCanon.transform.DOLocalRotate(randomRotation, canonRotationSpeedonIdle)
                .OnComplete(() => {
                     isRotating             = false;
                     lastRandomRotationTime = Time.time;
                 });
    }

    private void OnTriggerEnter(Collider other) {
        if (currentlTarget == null && UnitsManager.Instance.RegistredUnits.TryGetValue(other, out Unit unit)) {
            float distance = Vector3.Distance(unit.transform.position, transform.position);
            if (distance < attackRange) {
                currentlTarget = unit;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (currentlTarget != null && UnitsManager.Instance.RegistredUnits.TryGetValue(other, out Unit unit)) {
            if (currentlTarget == unit) {
                currentlTarget = null;
            }
        }
    }

    // Implement other required methods from Unit, IDeathHandler, and IDamageable interfaces
    public override void TakeDamage(int amount) {
        /* Implementation */
    }

    public override void Heal(int amount) {
        /* Implementation */
    }


    public override void Attack(IDamageable target) {
        /* Implementation */
    }

    public override void OnDeath() {
        /* Implementation */
    }

    public override void OnStateChanged(unitState oldstate, unitState newstate) {
        /* Implementation */
    }

   
}
