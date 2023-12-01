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
    
    void Start()
    {
        health = maxHealth;
        InitState();
    }
    
    public override void Update()
    {
        base.Update();
        UpdateState();
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
        yield return new WaitForSeconds(0.5f);
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
    }
}
