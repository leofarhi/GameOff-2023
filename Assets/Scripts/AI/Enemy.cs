using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : TemperatureBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        IdlePatrol,
        Chase,
        Attack,
        Dead
    }
    
    [Header("Enemy")]
    public EnemyState enemyState;
    public NavMeshAgent agent;

    [Header("Patrol")]
    public float patrolRange = 10;
    public float patrolSpeed = 2;
    private Vector3 _startPosition;
    private Vector3 _patrolTarget;
    public float patrolTimer = 10;
    private float _patrolTimer = 0;

    [Header("Chase")]
    public float chaseRange = 20;
    public float chaseSpeed = 5;
    
    [Header("Attack")]
    public GameObject attackTarget;
    public float attackRange = 1;
    public float attackDamage = 10;
    public float attackRate = 1;//attack per second
    public float attackCooldown = 0;
    
    public void GoTo(Vector3 position, float speed)
    {
        agent.speed = speed;
        agent.SetDestination(position);
    }
    
    public void InitState()
    {
        _startPosition = transform.position;
        _patrolTarget = _startPosition;
    }
    
    public bool isStopped()
    {
        return agent.velocity.magnitude < 0.1f && agent.remainingDistance <= agent.stoppingDistance;
    }
    
    public void Patrol()
    {
        if (enemyState == EnemyState.Patrol)
        {
            if (Vector3.Distance(transform.position, _patrolTarget) < 1 || (isStopped() && Vector3.Distance(transform.position, _patrolTarget) >= 1))
            {
                _patrolTarget = _startPosition + Random.insideUnitSphere * patrolRange;
                _patrolTarget.y = _startPosition.y;
                _patrolTimer = patrolTimer;
                enemyState = EnemyState.IdlePatrol;
            }
            else
            {
                GoTo(_patrolTarget, patrolSpeed);
            }
        }
        if (enemyState == EnemyState.IdlePatrol)
        {
            //Stop AI
            agent.speed = 0;
            _patrolTimer -= Time.deltaTime;
            if (_patrolTimer <= 0)
            {
                enemyState = EnemyState.Patrol;
                GoTo(_patrolTarget, patrolSpeed);
            }
        }
    }

    public virtual void UpdateSate()
    {
        Patrol();
    }
}
