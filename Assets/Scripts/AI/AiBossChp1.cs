using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AiBossChp1 : Enemy
{
    public UnityEvent OnDeath;
    public float health;
    public float maxHealth;
    public Image healthBar;
    public List<TakeDamage> weakPoints = new List<TakeDamage>();

    public List<string> attackAnimations = new List<string>();

    void Start()
    {
        health = maxHealth;
        InitState();
        //Disable all weakpoints when OnHit is invoked
        foreach (TakeDamage weakPoint in weakPoints)
        {
            weakPoint.OnHit.AddListener(() =>
            {
                foreach (TakeDamage weakPoint in weakPoints)
                {
                    weakPoint.gameObject.SetActive(false);
                }
            });
            weakPoint.gameObject.SetActive(false);
        }
    }
    
    public override void Update()
    {
        base.Update();
        UpdateState();
        foreach (TakeDamage weakPoint in weakPoints)
        {
            weakPoint.damage = attackDamage;
        }
        //look at target
        if (attackTarget != null)
        {
            Vector3 dir = attackTarget.transform.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        healthBar.fillAmount = health / maxHealth;
    }

    public override IEnumerator AttackAnimation()
    {
        int randomAttack = Random.Range(0, attackAnimations.Count);
        animator.PlayInFixedTime(attackAnimations[randomAttack], 0, 0);
        yield return null;
        float waitTime = 0;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimations[randomAttack]))
        {
            if (waitTime >= 0.5f && waitTime!=-1)
            {
                foreach (TakeDamage weakPoint in weakPoints)
                {
                    weakPoint.gameObject.SetActive(true);
                }
                waitTime = -1;
            }
            else
            {
                waitTime += Time.deltaTime;
            }
            yield return null;

        }
        foreach (TakeDamage weakPoint in weakPoints)
        {
            weakPoint.gameObject.SetActive(false);
        }
        _attackCoroutine = null;
    }

    public override void Die()
    {
        base.Die();
        OnDeath.Invoke();
    }
    
    public override void TakeDamageTemperature(float damage)
    {
        health -= Mathf.Abs(damage);
        if (health <= 0)
        {
            enemyState = EnemyState.Dead;
            Die();
        }
    }
}
