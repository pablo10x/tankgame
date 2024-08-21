using System;
using DG.Tweening;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class humvee_ai : Unit, IMoveable {
    public override void TakeDamage(int amount) {
        Health -= amount;
        if (Health <= 0) {
            Health = 0;
            OnDeath();
        }
    }

    public override void Heal(int amount) {
        Health += amount;
        if (Health > MaxHealth) {
            Health = MaxHealth;
        }
    }


    public override void Attack(IDamageable target) { }


    [ FoldoutGroup("Death") ] public float     explosionForce  = 1000f;
    [ FoldoutGroup("Death") ] public float     explosionRadius = 5f;
    [ FoldoutGroup("Death") ] public float     upwardsModifier = 1f;
    [ FoldoutGroup("Death") ] public Rigidbody rigidbody;


    private bool Despawning = false;


    // ReSharper disable Unity.PerformanceAnalysis
    public override void OnDeath() {
        isMoving = false;
        // Disable NavMeshAgent
        if (agent != null) {
            agent.enabled = false;
        }


        var rb = rigidbody;
        // Enable physics and apply explosion force
        if (rb != null) {
            rb.isKinematic = false;
            Vector3 explosionPos = transform.position + Vector3.down; // Slightly below the vehicle
            rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardsModifier);

            // Add random torque for rotation
            rb.AddTorque(Random.insideUnitSphere * explosionForce / 2f);
        }

        // Optional: Add visual effects here (e.g., particle system for explosion)

        // Delay the destruction to allow for physics simulation
        UnitsManager.Instance.DestroyUnit(this);
        Invoke("DestroyUnit", 5f); // Adjust the delay as needed
    }

    private void DestroyUnit() {
        rigidbody.isKinematic = true;
        Despawning            = true;
        // transform.DOMoveY(- 4f, 15f).onComplete += () => { Destroy(gameObject); };
    }

    public override void OnStateChanged(unitState oldstate, unitState newstate) { }


    protected override void Update() {
        base.Update();
        if (Despawning) {
            var pos = transform.position;
            if (pos.y < - 20f) {
                Destroy(gameObject);
            }
            else {
                transform.position = Vector3.Slerp(pos,
                    new Vector3(pos.x, pos.y - 1f, pos.z), 2f * Time.deltaTime);
            }
        }
    }


    public void Move(Vector3 position) {
        SetDestination(position);
    }

    public void Stop() {
        SetDestination(transform.position);
    }
}
