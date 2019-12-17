
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region Variables

    // Unity Editor Variables
    [SerializeField] protected Rigidbody2D deathParticlePrefab;
    [SerializeField] protected Renderer playerTexRend;
    [SerializeField] protected Transform playerTexObj;
    [SerializeField] protected List<Material> playerMaterials;

    // Public Properties
    public bool IsPlayerInactive { get; set; }
    public bool IsFrozen { get { return movement.IsFrozen; } set { movement.IsFrozen = value; } }
    public bool IsExternalForceActive { get { return movement.IsExternalForceActive; } set { movement.IsExternalForceActive = value; } }
    public bool IsDead { get { return health.IsDead; } set { health.IsDead = value; } }
    public bool CanShoot { get { return shooting.CanShoot; } set { shooting.CanShoot = value; } }
    public bool IsInvincible { get; set; }
    public float CurrentHealth { get { return health.CurrentHealth; } set { health.CurrentHealth = value; } }
    public Vector3 ExternalForce { get { return movement.ExternalForce; } set { movement.ExternalForce = value; } }
    public Vector3 CheckpointPosition { get { return movement.CheckPointPosition; } set { movement.CheckPointPosition = value; } }

    // Protected Instance Variables
    protected int walkingTexIndex = 0;
    protected int standingTexIndex = 0;
    protected float walkingTexInterval = 0.2f;
    protected float standingTexInterval = 0.3f;
    protected Health health = null;
    protected Vector2 texScaleLeft = new Vector2(1.0f, -1.0f);
    protected Vector2 texScaleRight = new Vector2(-1.0f, -1.0f);
    //protected Collider col = null;
    protected BoxCollider2D col = null;
    protected Movement movement = null;
    protected Shooting shooting = null;
    protected LevelCamera levelCamera = null;

    private Animator animator;

    #endregion


    #region MonoBehaviour

    // Constructor 
    protected void Awake()
    {
        GameEngine.Player = this;

        Assert.IsNotNull(deathParticlePrefab);
        Assert.IsTrue(playerMaterials.Count == 17);

        levelCamera = FindObjectOfType<LevelCamera>();
        Assert.IsNotNull(levelCamera);

        movement = gameObject.GetComponent<Movement>();
        Assert.IsNotNull(movement);

        shooting = gameObject.GetComponent<Shooting>();
        Assert.IsNotNull(shooting);

        health = gameObject.GetComponent<Health>();
        Assert.IsNotNull(health);

        col = GetComponent<BoxCollider2D>();
        Assert.IsNotNull(col);

        animator = GetComponent<Animator>();
        Assert.IsNotNull(animator);
    }

    // Use this for initialization 
    protected void Start()
    {
        IsPlayerInactive = false;
        health.HealthbarPosition = new Vector2(10, 10);
        health.ShowHealthBar = true;
    }

    // Update is called once per frame 
    protected void Update()
    {
        if (IsPlayerInactive == false)
        {
            // Handle the horizontal and Vertical movements
            movement.HandleMovement();

            // Handle shooting
            if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Fire1")) && shooting.CanShoot == true)
            {
                shooting.Shoot(movement.IsTurningLeft);
                GameEngine.SoundManager.Play(AirmanLevelSounds.SHOOTING);
            }

            // Handle health
            if (health.IsHurting)
            {
                if (Time.time - health.HurtingTimer >= health.HurtingDelay)
                {
                    movement.IsHurting = false;
                    health.IsHurting = false;
                    CanShoot = true;
                    Invoke("ResetInvincibility", 3f);
                }
            }

            if (IsInvincible)
            {
                //playerTexRend.enabled = !playerTexRend.enabled;
                var color = GetComponent<SpriteRenderer>().color;
                color.a = (color.a == 0f) ? 1f : 0f;
                GetComponent<SpriteRenderer>().color = color;
            }

            bool flip = true;
            if (movement.IsTurningLeft == true)
            {
                flip = false;
            }

            GetComponent<SpriteRenderer>().flipX = flip;

            // Assign the appropriate texture to the player...
            AssignTexture();

            // keep the z axis as zero
            if (transform.position.z != 0)
            {
                Vector3 pos = transform.position;
                pos.z = 0;
                transform.position = pos;
            }
        }
    }

    private void ResetInvincibility()
    {
        IsInvincible = false;

        var color = GetComponent<SpriteRenderer>().color;
        color.a = 1f;
        GetComponent<SpriteRenderer>().color = color;

    }
    // Called when the behaviour becomes disabled or inactive
    protected void OnDisable()
    {
        GameEngine.Player = null;
    }

    #endregion


    #region Protected Functions

    // 
    protected void Reset()
    {
        health.Reset();
        movement.Reset();
        shooting.Reset();
        IsPlayerInactive = false;
    }

    // 
    protected void CreateDeathParticle(float speed, Vector3 pos, Vector3 vel)
    {
        Rigidbody2D particle = (Rigidbody2D)Instantiate(deathParticlePrefab, pos, transform.rotation);
        Physics2D.IgnoreCollision(particle.GetComponent<Collider2D>(), col);
        particle.transform.Rotate(90, 0, 0);
        particle.velocity = vel * speed;
    }

    // 
    protected IEnumerator CreateDeathParticles(Vector3 pos)
    {
        float deathParticleSpeed = 6.0f;

        // Before the wait...
        Vector3 p1 = pos + Vector3.up;
        Vector3 p2 = pos - Vector3.up;
        Vector3 p3 = pos + Vector3.right;
        Vector3 p4 = pos - Vector3.right;

        Vector3 p5 = pos + Vector3.up + Vector3.right;
        Vector3 p6 = pos + Vector3.up - Vector3.right;
        Vector3 p7 = pos - Vector3.up - Vector3.right;
        Vector3 p8 = pos - Vector3.up + Vector3.right;

        p1.z = p2.z = -5;
        p3.z = p4.z = -7;
        p5.z = p6.z = p7.z = p8.z = -9;

        this.CreateDeathParticle(deathParticleSpeed, p1, (transform.up));
        this.CreateDeathParticle(deathParticleSpeed, p2, (-transform.up));
        this.CreateDeathParticle(deathParticleSpeed, p3, (transform.right));
        this.CreateDeathParticle(deathParticleSpeed, p4, (-transform.right));
        this.CreateDeathParticle(deathParticleSpeed, p5, (transform.up + transform.right));
        this.CreateDeathParticle(deathParticleSpeed, p6, (transform.up - transform.right));
        this.CreateDeathParticle(deathParticleSpeed, p7, (-transform.up - transform.right));
        this.CreateDeathParticle(deathParticleSpeed, p8, (-transform.up + transform.right));

        // Start the wait...
        yield return new WaitForSeconds(0.7f);

        // After the wait...
        this.CreateDeathParticle(deathParticleSpeed / 2.5f, p1, transform.up);
        this.CreateDeathParticle(deathParticleSpeed / 2.5f, p2, -transform.up);
        this.CreateDeathParticle(deathParticleSpeed / 2.5f, p3, transform.right);
        this.CreateDeathParticle(deathParticleSpeed / 2.5f, p4, -transform.right);
    }

    // 
    protected IEnumerator MovePlayerUp()
    {
        while (true)
        {
            transform.position += Vector3.up * 35.0f * Time.deltaTime;
            yield return null;
        }
    }

    // TODO: Fix
    protected IEnumerator MakeThePlayerLeaveStageRoutine()
    {
        playerTexRend.material = playerMaterials[14];
        yield return new WaitForSeconds(0.05f);

        playerTexRend.material = playerMaterials[15];
        yield return new WaitForSeconds(0.05f);

        GameEngine.SoundManager.Play(AirmanLevelSounds.LEAVE_LEVEL);
        playerTexRend.material = playerMaterials[16];
        playerTexObj.localScale = new Vector3(0.04f, 1.0f, 0.2f);

        StartCoroutine(MovePlayerUp());

        yield return new WaitForSeconds(3.0f);

        StopCoroutine(MovePlayerUp());

        IsPlayerInactive = false;

        SceneManager.LoadScene(0);
    }

    // 
    protected IEnumerator WaitAndResetRoutine()
    {
        // Before the wait... 
        health.IsDead = true;
        movement.IsFrozen = true;
        //playerTexRend.enabled = false;
        levelCamera.ShouldStayStill = true;
        shooting.CanShoot = false;

        yield return new WaitForSeconds(3.6f);

        // Reset the camera
        levelCamera.Reset();

        // Reset the player
        Reset();
    }

    // 	
    protected void AssignTexture()
    {
        if (health.IsHurting == true && health.IsDead == false)
        {
            animator.Play("Hurt");
        }
        else if (movement.IsJumping == true)
        {
            if (shooting.IsShooting == true)
            {
                animator.Play("JumpShoot");
            }
            else
            {
                animator.Play("Jumping");
            }
        }
        else if (movement.IsWalking == true)
        {

            if (shooting.IsShooting == true)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Walking") == true)
                {
                    animator.Play("WalkShoot", 0, stateInfo.normalizedTime);
                }
                else
                {
                    animator.Play("WalkShoot");
                }

            }
            else
            {
                animator.Play("Walking");
            }
        }
        // Standing...
        else
        {
            if (shooting.IsShooting == true)
            {
                animator.Play("Shooting");
            }
            else
            {
                animator.Play("Standing");
            }
        }
    }

    #endregion

    #region Public Functions

    // 
    public void PlayEndSequence()
    {
        StartCoroutine(MakeThePlayerLeaveStageRoutine());
    }

    // 
    public void KillPlayer()
    {
        IsInvincible = false;
        StartCoroutine(CreateDeathParticles(transform.position));
        StartCoroutine(WaitAndResetRoutine());
    }

    // 
    public void TakeDamage(float damage)
    {
        // If the player isn't already hurting or dead...
        if (health.IsHurting == false && health.IsDead == false && IsInvincible == false)
        {
            GameEngine.SoundManager.Play(AirmanLevelSounds.HURTING);
            health.ChangeHealth(-damage);
            movement.IsHurting = true;
            IsInvincible = true;
            CanShoot = false;

            if (health.IsDead == true)
            {
                KillPlayer();
            }
        }
    }

    public void RevivePlayer()
    {
        GameEngine.SoundManager.Play(AirmanLevelSounds.STAGE);
        levelCamera.ShouldStayStill = false;
    }
    #endregion
}