using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void DeadEventHandler();

public class Player : Character
{

    #region Fields

    [SerializeField]
    private GameObject gameOverUI;

    private static Player instance;

    public event DeadEventHandler Dead;

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Player>();
            }
            return instance;
        }

    }

    [SerializeField]
    private Transform[] groundPoints;

    [SerializeField]
    private float groundRadius;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private bool airControl;

    [SerializeField]
    private float jumpForce;

    private bool canDoubleJump = false;

    private SpriteRenderer spriteRenderer;

    //private bool hasKnives;

    [SerializeField]
    private float immortalTime;

    private bool immortal = false;

    public Rigidbody2D MyRigidbody { get; set; }

    public bool Slide { get; set; }
    public bool Jump { get; set; }
    public bool OnGround { get; set; }

    public override bool IsDead //Verifica daca e mort sau nu
    {
        get
        {
            if (healthStat.CurrentVal <= 0)
            {
                OnDead();
            }
            
            return healthStat.CurrentVal <= 0;
        }
    }

    [SerializeField]
    private Vector2 startPos; //Pos initiala
    #endregion

    public override void Start()
    {
        base.Start();

        //transform.position = startPos;
        spriteRenderer = GetComponent<SpriteRenderer>();
        MyRigidbody = GetComponent<Rigidbody2D>();

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (!TakingDamage && !IsDead)
        {
            HandleInput();

            float horizontal = Input.GetAxis("Horizontal");

            OnGround = IsGrounded();

            HandleMovement(horizontal);

            Flip(horizontal);

            HandleLayers();
        }
    }

    public void OnDead()
    {
        if (Dead != null)
        {
            Dead();
        }
    }

    private void HandleMovement(float horizontal)
    {
        if (MyRigidbody.velocity.y < 0)
        {
            MyAnimator.SetBool("land", true);
        }

        if (!Attack && !Slide && (OnGround || airControl))
        {
            MyRigidbody.velocity = new Vector2(horizontal * movementSpeed, MyRigidbody.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && !OnGround && canDoubleJump)
        {
            MyRigidbody.AddForce(new Vector2(0, jumpForce));
            canDoubleJump = false;
        }
        else
        if (Input.GetKeyDown(KeyCode.UpArrow) && OnGround)
        {
            MyRigidbody.AddForce(new Vector2(0, jumpForce));
            canDoubleJump = true;
        }


        MyAnimator.SetFloat("speed", Mathf.Abs(horizontal)); // Seteaza parametrul speed al animatorului in 1 --> se activeaza animatia run
    }

    private void HandleInput() // Verifica apasarea unor taste care activeaza triggere care activeaza aniamatii -> altceva
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            MyAnimator.SetTrigger("attack");
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MyAnimator.SetTrigger("throw");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MyAnimator.SetTrigger("jump");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && OnGround)
        {
            MyAnimator.SetTrigger("slide");
        }      
    }
        
    private void Flip(float horizontal)
    {
        if((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            ChangeDirection();
        }
    }

    private bool IsGrounded()
    {
        if (MyRigidbody.velocity.y <= 0)
        {
            foreach (Transform point in groundPoints)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(point.position, groundRadius, whatIsGround);

                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].gameObject != gameObject)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void HandleLayers() // Schimba layer-ul de animatii in cel de Airlayer daca playerul nu este pe pamant
    {
        if(!OnGround)
        {
            MyAnimator.SetLayerWeight(1, 1);
        }
        else
        {
            MyAnimator.SetLayerWeight(1, 0);
        }
    }

    public override void ThrowKnife(int value)
    {
        if ((!OnGround && value == 1) || (OnGround && value == 0))
        {
            base.ThrowKnife(value);
        }   
    }

    private IEnumerator IndicateImmortal() // CLipeste playerul cand nu mai poate fi atacat(imortal)
    {
        while (immortal)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(.1f);
        }
    }

    public override IEnumerator TakeDamage(string source)
    {
       
        if (!immortal)
        {
            if (source == "EnemySword")
            {
                healthStat.CurrentVal -= 20;
            }
            else
            if(source == "EnemyKnife")
            {
                healthStat.CurrentVal -= 20;
            }


            if (!IsDead)
            {
                MyAnimator.SetTrigger("damage");
                immortal = true;

                StartCoroutine(IndicateImmortal());

                yield return new WaitForSeconds(immortalTime);

                immortal = false;
            }
            else
            {
                MyAnimator.SetLayerWeight(1, 0);
                MyAnimator.SetTrigger("die");
            }
        }
    }

    public override void Death()
    {
        //MyRigidbody.velocity = Vector2.zero;
        //MyAnimator.SetTrigger("idle"); //Sa nu dea respwan cu death animation
        //healthStat.CurrentVal = 100;
        //transform.position = startPos;
        Destroy(base.gameObject);
        gameOverUI.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "EnemyCoin")
        {
            GameManager.Instance.Score += 100;
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Level2")
        {
            if (GameObject.Find("Enemy") == null)
            {
                SceneManager.LoadScene("Level2");
            }
        }
        
        if (other.gameObject.tag == "EndGame")
        {
            if (GameObject.Find("Enemy") == null)
            {
                SceneManager.LoadScene("EndGame");
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (damageSources.Contains(other.tag))
        {
            StartCoroutine(TakeDamage(other.tag));
        }
        else
        switch (other.tag)
        {
            case "Coin":
                GameManager.Instance.Score += 100;
                Destroy(other.gameObject);
                break;

            case "Apple":
                healthStat.CurrentVal += 15;
                GameManager.Instance.Score += 100;
                Destroy(other.gameObject);
                break;

            case "Heart":
                healthStat.MaxVal += 50;
                healthStat.CurrentVal += 50;
                Destroy(other.gameObject);
                break;

            case "SpeedBuff":
                movementSpeed += 5;
                Destroy(other.gameObject);
                break;

            case "Deadly":
                Death();
                break;

            default:
                break;
        }
    }
}
