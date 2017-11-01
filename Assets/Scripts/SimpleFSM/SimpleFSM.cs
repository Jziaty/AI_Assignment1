using UnityEngine;
using System.Collections;

public class SimpleFSM : FSM 
{
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Evade,
        Attack,
        Flee,
        Dead,
    }

    //Current state that the NPC is reaching
    public FSMState curState;

    //Speed of the tank
    private float curSpeed;

    //Tank Rotation Speed
    private float curRotSpeed;

    //Bullet
    public GameObject Bullet;

    //Whether the NPC is destroyed or not
    private bool bDead;
    private int health;

    //Distance array for the enemy tanks
    //private float[] Edistance;

    //Transform array for the enemy tanks
    //public Transform[] enemiesTransform;

    public bool turnRight;
    public bool turnLeft;

    private float force = 50f;

    private Vector3 targetPoint;

    public Transform HeadlightL;
    public Transform HeadlightR;

    private Vector3 trgtRot;
    private int StndOffset;

    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize () 
    {
        curState = FSMState.Patrol;
        curSpeed = 150.0f;
        curRotSpeed = 2.0f;
        bDead = false;
        elapsedTime = 0.0f;
        shootRate = 3.0f;
        health = 150;

        //Get the list of points
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");

        //Set Random destination point first
        FindNextPoint();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        if(!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
    }

    //Update each frame
    protected override void FSMUpdate()
    {
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Evade: UpdateEvadeState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Flee: UpdateFleeState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }


        //GameObject[] Bots = GameObject.FindGameObjectsWithTag("Enemy");

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 100;
        Vector3 forwardright = transform.TransformDirection(Vector3.forward + new Vector3(1, 0, 0)) * 100;
        Vector3 forwardleft = transform.TransformDirection(Vector3.forward + new Vector3(-1, 0, 0)) * 100;
        Vector3 forwardL = HeadlightL.TransformDirection(Vector3.forward) * 100;
        Vector3 forwardR = HeadlightR.TransformDirection(Vector3.forward) * 100;
        
        RaycastHit hit;
        RaycastHit hit2;
        RaycastHit hit3;
        RaycastHit hit4;
        RaycastHit hit5;

        if (Physics.Raycast(transform.position, forward, out hit, 400))
        {
            if (hit.distance <= 300)
            {
                Debug.DrawLine(transform.position, hit.transform.position, Color.yellow);
                trgtRot = Quaternion.LookRotation(hit.transform.position + Vector3.right * 100).eulerAngles;
                turnRight = true;
                curState = FSMState.Evade;

            }
        }

        if (Physics.Raycast(transform.position, forwardright, out hit2, 400))
        {
            if (hit2.distance <= 300)
            {
                Debug.DrawLine(transform.position, hit2.transform.position, Color.blue);
                trgtRot = Quaternion.LookRotation(hit2.transform.position + Vector3.left).eulerAngles;
                turnLeft = true;
                curState = FSMState.Evade;

            }
        }

        if (Physics.Raycast(transform.position, forwardleft, out hit3, 400))
        {
            if (hit3.distance <= 300)
            {
                Debug.DrawLine(transform.position, hit3.transform.position, Color.green);
                trgtRot = Quaternion.LookRotation(hit3.transform.position + Vector3.right).eulerAngles;
                turnRight = true;
                curState = FSMState.Evade;
            }
        }

        if (Physics.Raycast(transform.position, forwardL, out hit4, 400))
        {
            if (hit4.distance <= 300)
            {
                Debug.DrawLine(transform.position, hit4.transform.position, Color.red);
                trgtRot = Quaternion.LookRotation(hit4.transform.position + Vector3.left).eulerAngles;
                turnRight = true;
                curState = FSMState.Evade;
            }
        }
        if (Physics.Raycast(transform.position, forwardR, out hit5, 400))
        {
            if (hit5.distance <= 300)
            {
                Debug.DrawLine(transform.position, hit5.transform.position, Color.magenta);
                trgtRot = Quaternion.LookRotation(hit5.transform.position + Vector3.right).eulerAngles;
                turnLeft = true;
                curState = FSMState.Evade;

            }
        }



        //Update the time
        elapsedTime += Time.deltaTime;

                //Go to dead state is no health left
                if (health <= 0)
                    curState = FSMState.Dead;
            }
        
            /// <summary>
            /// Patrol state
            /// </summary>
            protected void UpdatePatrolState()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= 100.0f)
        {
            print("Reached to the destination point\ncalculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        else if (dist <= 300.0f && dist > 200.0f)
        {
            print("Switch to Chase Position");
            curState = FSMState.Chase;
        } else if (dist <= 50.0f && health <= 50)
        {
            curState = FSMState.Flee;
        } 

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Chase state
    /// </summary>
    protected void UpdateChaseState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= 200.0f && dist >= 100.0f)
        {
            curState = FSMState.Attack;
        }
        //Go back to patrol is it become too far
        else if (dist >= 300.0f)
        {
            curState = FSMState.Patrol;
        } else if (dist <= 100.0f && health <= 50)
        {
            curState = FSMState.Flee;
        }

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Evade state
    /// </summary>
    protected void UpdateEvadeState()
    {
        StndOffset = 180;
        if (turnLeft)
        {
            //StndOffset = -180;
        }
            

        if (turnRight)
        {
            //StndOffset = 180;
        }
            

        trgtRot = new Vector3(trgtRot.x, trgtRot.y + StndOffset, trgtRot.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(trgtRot), Time.deltaTime * curRotSpeed);
        turnLeft = false;
        turnRight = false;
       
        


        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= 200.0f && dist >= 100.0f)
        {
            curState = FSMState.Attack;
        }
        //Go back to patrol is it become too far
        else if (dist >= 300.0f)
        {
            curState = FSMState.Patrol;
        }
        else if (dist <= 100.0f && health <= 50)
        {
            curState = FSMState.Flee;
        }

        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Attack state
    /// </summary>
    protected void UpdateAttackState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with the player tank
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist >= 200.0f && dist < 300.0f)
        {
            //Rotate to the target point
            Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

            //Go Forward
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

            curState = FSMState.Attack;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
        {
            curState = FSMState.Patrol;
        } else if (dist <= 200.0f && health <= 100)
        {
            curState = FSMState.Flee;
        }        

        //Always Turn the turret towards the player
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed); 

        //Shoot the bullets
        ShootBullet();
    }

    /// <summary>
    /// Flee state
    /// </summary>
    protected void UpdateFleeState()
    {
        //Find another random patrol point if the current point is reached
        FindNextPoint();

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        if (dist <= 300.0f && dist > 200.0f)
        {
            print("Switch to Chase Position");
            curState = FSMState.Chase;
        }

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Dead state
    /// </summary>
    protected void UpdateDeadState()
    {
        //Show the dead animation with some physics effects
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    private void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            //Shoot the bullet
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if(collision.gameObject.tag == "Bullet")
            health -= collision.gameObject.GetComponent<Bullet>().damage;
    }   

    /// <summary>
    /// Find the next semi-random patrol point
    /// </summary>
    protected void FindNextPoint()
    {
        print("Finding next point");
        int rndIndex = Random.Range(0, pointList.Length);
        float rndRadius = 10.0f;
        
        Vector3 rndPosition = Vector3.zero;
        destPos = pointList[rndIndex].transform.position + rndPosition;

        //Check Range
        //Prevent to decide the random point as the same as before
        if (IsInCurrentRange(destPos))
        {
            rndPosition = new Vector3(Random.Range(-rndRadius, rndRadius), 0.0f, Random.Range(-rndRadius, rndRadius));
            destPos = pointList[rndIndex].transform.position + rndPosition;
        }
    }

    /// <summary>
    /// Check whether the next random position is the same as current tank position
    /// </summary>
    /// <param name="pos">position to check</param>
    protected bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= 50 && zPos <= 50)
            return true;

        return false;
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }

}
