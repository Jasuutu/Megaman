using UnityEngine;
using UnityEngine.Assertions;
using Extensions;

public class KaminariGoro : MonoBehaviour 
{
	#region Variables

	// Unity Editor Variables
	[SerializeField] protected Rigidbody2D electricShot;
	[SerializeField] protected CirclingPlatform platform;
	[SerializeField] protected BoxCollider2D robotCollider;
    [SerializeField] protected float distanceToStop = 32.0f;
    [SerializeField] protected float shootingRangeDiameter = 10f;
    [SerializeField] private float shotSpeed = 50f;

    private Animator robotAnim = null;
    private Animator platformAnim = null;

    // Protected Instance Variables
    protected int health = 30;
	protected int currentHealth;
	protected int damage = 10;
	protected bool isShooting = false;
	protected bool isDead = false;
    private bool canShoot = false;

	protected float shootAgainDelay = 2f;
	protected float shootingTimer;
	protected Vector3 turningLeftColliderPos = new Vector3(0.2f, -0.8f, 0f);
	protected Vector3 turningRightColliderPos = new Vector3(-0.2f, -0.8f, 0f);
	protected Collider2D col = null;
	protected SpriteRenderer rend = null;

	#endregion


	#region MonoBehaviour

	// Constructor
	protected void Awake()
	{
		col = GetComponent<Collider2D>();
		Assert.IsNotNull(col);

		rend = GetComponent<SpriteRenderer>();
		Assert.IsNotNull(rend);

        robotAnim = GetComponent<Animator>();
        Assert.IsNotNull(robotAnim);

        platformAnim = platform.GetComponentInChildren<Animator>();
        Assert.IsNotNull(platformAnim);
    }

	// Use this for initialization 
	protected void Start()
	{
		currentHealth = health;
        platformAnim.Play("Platform");
        robotAnim.Play("Wait");

    }

    // Update is called once per frame 
    protected void Update() 
	{
		// Stop fighting if the player is too far away
		if ((GameEngine.Player.transform.transform.position - transform.position).magnitude >= distanceToStop)
		{
			platform.ShouldAnimate = false;
		}
		else
		{
			platform.ShouldAnimate = true;
			
			// Check if the robot can shoot the player
			CheckIfRobotCanShoot();
			
			// Put the correct texture on the robot
			SetAnimations();
		}
	}	

	#endregion

	
	#region Protected Functions

	// 
	protected void KillRobot()
	{
		isDead = true;
		col.enabled = false;
	}
	
	// 
	protected void OnTriggerStay2D(Collider2D other) 
	{
		if (other.tag == "Player")
		{
			GameEngine.Player.TakeDamage(damage);
		}
	}
	
	//  Make the robot take damage 
	protected void TakeDamage(int damageTaken)
	{
		GameEngine.SoundManager.Play(AirmanLevelSounds.BOSS_HURTING);
		currentHealth -= damageTaken;
		if (currentHealth <= 0)
		{
			KillRobot();
		}
	}

	// 
	protected void SetAnimations()
	{
		
		// Make the robot always face the player...
		bool playerOnLeftSide = (GameEngine.Player.transform.position.x - transform.position.x < -1.0f);
		
		// If the robot is dead
		if (isDead == true)
		{
            robotAnim.StopPlayback();
            rend.enabled = false;
           // robotCollider.offset = turningLeftColliderPos;
		}
		
		// If the robot is shooting...
		else if (isShooting == true && canShoot == true)
		{
            robotAnim.Play("Throw");
            canShoot = false;
            rend.flipX = !playerOnLeftSide;
           // robotCollider.offset = (playerOnLeftSide == true) ? turningLeftColliderPos : turningRightColliderPos;
		}
		else
		{
            rend.flipX = !playerOnLeftSide;
           // robotCollider.offset = (playerOnLeftSide == true) ? turningLeftColliderPos : turningRightColliderPos;
		}
	}
	
	//  Shoot an electric arrow towards the player 	
	protected void Shoot()
	{
		isShooting = true;
		shootingTimer = Time.time;

        Rigidbody2D shot = Instantiate(electricShot, transform.position, transform.rotation);
		shot.transform.parent = gameObject.transform;
		Physics2D.IgnoreCollision(shot.GetComponent<Collider2D>(), col);
        Physics2D.IgnoreCollision(shot.GetComponent<Collider2D>(), platform.GetComponent<Collider2D>());
        //var PlatformCollider = platform.gameObject.GetChildWithName("PlatformBoxCollider");
        //Physics2D.IgnoreCollision(shot.GetComponent<Collider2D>(), PlatformCollider.GetComponent<Collider2D>());

        KaminariGoroShot shotScript = shot.GetComponent<KaminariGoroShot>();
		if (shotScript)
		{
            shotScript.Speed = shotSpeed;
            
            // set the target's position for the shot to aim at.
            var offset = platform.gameObject.GetChildWithName("Target");

            var direction = -1;
            if ((offset.transform.localPosition.x < 0 && !rend.flipX) || (offset.transform.localPosition.x > 0 && rend.flipX))
            {
                direction = 1;
            }
 
            offset.transform.localPosition = new Vector3(offset.transform.localPosition.x * direction, offset.transform.localPosition.y, offset.transform.localPosition.z);
            shotScript.target = offset.transform;
			shotScript.TargetDirection = GameEngine.Player.transform.position;
		}
	}
	
	// 
	protected void CheckIfRobotCanShoot()
	{
		// If the robot is alive, check if he is in range to shoot the player
		if (isDead == false)
		{
			// If the robot is in shooting range...
			float distanceToPlayer = (GameEngine.Player.transform.position - transform.position).magnitude;
			if (distanceToPlayer < shootingRangeDiameter)
			{
				// If the robot is ready to shoot...
				if (isShooting == false && Time.time - shootingTimer >= shootAgainDelay)
				{
                    canShoot = true;
					Shoot();
				}
			}
		}
	}

	#endregion


	#region Public Functions

	// 
	public void SetIsShooting(bool status)
	{
		isShooting = status;
	}
	
	// 
	public void Reset()
	{
		isDead = false;
		col.enabled = true;
		currentHealth = health;
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + shootingRangeDiameter / 2, transform.position.y, transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - shootingRangeDiameter / 2, transform.position.y, transform.position.z));

    }

    #endregion
}
