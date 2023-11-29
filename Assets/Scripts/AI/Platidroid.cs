using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platidroid : Enemy
{
    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private int _animIDLookAround;
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDLookAround = Animator.StringToHash("LookAround");
    }
    // Start is called before the first frame update
    void Start()
    {
        InitState();
        AssignAnimationIDs();
    }

    // Update is called once per frame
    public override void Update()
    {
        attackTarget = PersistenceDataScene.Instance.player.gameObject;
        base.Update();
        float speed;
        if (agent.speed == 0)
        {
            agent.velocity = agent.velocity/2;
        }
        animator.SetFloat(_animIDSpeed, agent.speed * 2.5f);
        animator.SetFloat(_animIDMotionSpeed, Mathf.Clamp(agent.velocity.magnitude, 0.1f, 1.5f));
        animator.SetBool(_animIDLookAround, enemyState == EnemyState.IdlePatrol && agent.velocity.magnitude <= 0.05);
        UpdateSate();
    }

    public override IEnumerator AttackAnimation()
    {
        //Orient towards target
        Vector3 direction = attackTarget.transform.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        //Play attack animation
        animator.PlayInFixedTime("SimpleAttack", 0, 0);
        yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SimpleAttack"))
        {
            yield return null;
        }
        _attackCoroutine = null;
    }
}
