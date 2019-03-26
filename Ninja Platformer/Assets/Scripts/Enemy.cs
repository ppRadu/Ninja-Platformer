using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    private IEnemyState currentState;

    public GameObject Target { get; set; }

    [SerializeField]
    private float meleeRange;

    [SerializeField]
    private float throwRange;

    public bool InMeleeRange
    {
        get
        {
            if (Target != null)
            {
                return Vector2.Distance(transform.position, Target.transform.position) <= meleeRange;
            }

            return false;
        }
    }

    public bool InThrowRange
    {
        get
        {
            if (Target != null)
            {
                return Vector2.Distance(transform.position, Target.transform.position) <= throwRange;
            }

            return false;
        }
    }

    public override bool IsDead
    {
        get
        {
            return healthStat.CurrentVal <= 0;
        }
    }

    private Canvas healthCanvas;

    public override void Start ()
    {
        base.Start();
        Player.Instance.Dead += new DeadEventHandler(RemoveTarget);
        ChangeState(new IdleState());

        healthCanvas = transform.GetComponentInChildren<Canvas>();
	}
	
	void Update ()
    {
        if (!IsDead)
        {
            if (!TakingDamage)
            {
                currentState.Execute();
            }
            
            LookAtTarget();
        } 
	}

    public void RemoveTarget()
    {
        Target = null;
        ChangeState(new PatrolState());
    }

    private void LookAtTarget()
    {
        if (Target != null)
        {
            float xDir = Target.transform.position.x - transform.position.x;

            if ((xDir < 0 && facingRight) || (xDir > 0 && !facingRight))
            {
                ChangeDirection();
            }
        }
    }

    public void ChangeState(IEnemyState newState)
    {
        if(currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;

        currentState.Enter(this);
    }

    public void Move()
    {
        if (!Attack)
        {
            MyAnimator.SetFloat("speed", 1);

            transform.Translate(GetDirection() * (movementSpeed * Time.deltaTime));
        }
    }

    public Vector2 GetDirection()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        currentState.OnTriggerEnter(other);
    }

    public override IEnumerator TakeDamage(string source)
    {
        if (!healthCanvas.isActiveAndEnabled)
        {
            healthCanvas.enabled = true;
        }

        if (source == "Sword")
        {
            healthStat.CurrentVal -= 25;
        }
        else
        if (source == "Knife")
        {
            healthStat.CurrentVal -= 15;
        }

       
        if (!IsDead)
        {
            MyAnimator.SetTrigger("damage");
        }
        else
        {
            GameObject coin = (GameObject)Instantiate(GameManager.Instance.EnemyCoinPrefab, new Vector3(transform.position.x, transform.position.y + 1.5f), Quaternion.identity);
            Physics2D.IgnoreCollision(coin.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            GameManager.Instance.Score += 250;

            MyAnimator.SetTrigger("die");
            yield return null;
        }
    }

    public override void Death()
    {
        Destroy(gameObject);
        healthCanvas.enabled = false;
    }

    public override void ChangeDirection()
    {
        //Makes a reference to the enemys canvas
        Transform tmp = transform.Find("EnemyHealthBarCanvas").transform;

        //Stores the position, so that we know where to move it after we have flipped the enemy
        Vector3 pos = tmp.position;

        ///Removes the canvas from the enemy, so that the health bar doesn't flip with it
        tmp.SetParent(null);

        ///Changes the enemys direction
        base.ChangeDirection();

        //Puts the health bar back on the enemy.
        tmp.SetParent(transform);

        //Pits the health bar back in the correct position.
        tmp.position = pos;
    }
}
