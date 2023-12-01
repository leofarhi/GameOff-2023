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
    public Animator animator;
    public float viewAngle = 45;

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
    protected IEnumerator _attackCoroutine;
    
    
    public bool isDead()
    {
        return enemyState == EnemyState.Dead;
    }
    
    public bool ViewTarget()
    {
        if (attackTarget != null)
        {
            if (Vector3.Distance(transform.position, attackTarget.transform.position) < chaseRange)
            {
                //check if player is in view angle
                var direction = attackTarget.transform.position - transform.position;
                var angle = Vector3.Angle(direction, transform.forward);
                if (Mathf.Abs(angle) < viewAngle)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
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
            if (agent.velocity.magnitude <= 0.05f)
                _patrolTimer -= Time.deltaTime;
            if (_patrolTimer <= 0)
            {
                enemyState = EnemyState.Patrol;
                GoTo(_patrolTarget, patrolSpeed);
            }
        }
    }
    
    public virtual void Chase()
    {
        if (Vector3.Distance(transform.position, attackTarget.transform.position) <= attackRange)
        {
            enemyState = EnemyState.Attack;
        }
        else
        {
            GoTo(attackTarget.transform.position, chaseSpeed);
            if(isStopped())
            {
                if (Vector3.Distance(transform.position, attackTarget.transform.position) <
                    attackRange + 0.25f)
                {
                    enemyState = EnemyState.Attack;
                }
                else
                {
                    agent.speed = 0;
                }
            }
            
        }
        if (Vector3.Distance(transform.position, attackTarget.transform.position) >= chaseRange)
        {
            enemyState = EnemyState.Patrol;
        }
    }
    
    public virtual void Attack()
    {
        if (attackCooldown <= 0)
        {
            attackCooldown = 1 / attackRate;
            _attackCoroutine = AttackAnimation();
            StartCoroutine(_attackCoroutine);
            //attackTarget.GetComponent<TemperatureBehaviour>().currentTemperature -= attackDamage;
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
        if (Vector3.Distance(transform.position, attackTarget.transform.position) > attackRange && 
            !(isStopped() && Vector3.Distance(transform.position, attackTarget.transform.position) < attackRange+0.25f))
        {
            enemyState = EnemyState.Chase;
        }
    }
    
    public virtual IEnumerator AttackAnimation()
    {
        yield return null;
        _attackCoroutine = null;
    }

    public virtual void UpdateState()
    {
        if (GameStateManager.Instance.CurrentGameState != GameState.Gameplay)
        {
            return;
        }
        if (enemyState == EnemyState.Dead)
        {
            return;
        }
        if (_attackCoroutine != null)
        {
            return;
        }
        Patrol();
        if (attackTarget == null)
        {
            enemyState = EnemyState.Patrol;
        }
        else
        {
            if (enemyState == EnemyState.Patrol || enemyState == EnemyState.IdlePatrol)
            {
                if (ViewTarget())
                {
                    enemyState = EnemyState.Chase;
                }
            }
            if (enemyState == EnemyState.Chase)
            {
                Chase();
            }
            else if (enemyState == EnemyState.Attack)
            {
                Attack();
            }
        }
    }
    
    public virtual void Die()
    {
        enemyState = EnemyState.Dead;
        agent.speed = 0;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        animator.SetTrigger("Die");
    }

    public override void TakeDamageTemperature(float damage)
    {
        base.TakeDamageTemperature(damage);
        enemyState = EnemyState.Chase;
    }
    
    public void OnGameStateChanged(GameState newGameState)
    {
        agent.enabled = newGameState == GameState.Gameplay;
        animator.enabled = newGameState == GameState.Gameplay;
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }
        
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
}
