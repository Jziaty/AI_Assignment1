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
    public Transform[] enemiesTransform;

    private bool right;
    private bool left;


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
        GameObject[] Bots = GameObject.FindGameObjectsWithTag("Enemy");

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 100;
        Vector3 forwardright = transform.TransformDirection(Vector3.forward + new Vector3(1, 0, 0)) * 100;
        Vector3 forwardleft = transform.TransformDirection(Vector3.forward + new Vector3(-1, 0, 0)) * 100;

        //foreach (GameObject bot in Bots)
        //{
            //float distance = Vector3.Distance(bot.transform.position, transform.position);

            //if (distance < 50.0f && distance > 0f)
            //{
                
                //Debug.DrawRay(transform.position, forward, Color.green);

                if (Physics.Raycast(transform.position, forward, 100))
                {
                    Debug.LogWarning("Green Ray hit.");
                    left = true;
                    curState = FSMState.Evade;
                }

                
                //Debug.DrawRay(transform.position, forwardright, Color.green);
                if (Physics.Raycast(transform.position, forwardright, 100))
                {
                    Debug.LogWarning("Blue Ray hit.");
                    left = true;
                    curState = FSMState.Evade;

                }

                
                //Debug.DrawRay(transform.position, forwardleft, Color.green);
                if (Physics.Raycast(transform.position, forwardright, 100))
                {
                    Debug.LogWarning("Red Ray hit.");
                    left = false;
                    curState = FSMState.Evade;

                }
                    //}
                //}


                switch (curState)
                {
                    case FSMState.Patrol: UpdatePatrolState(); break;
                    case FSMState.Chase: UpdateChaseState(); break;
                    case FSMState.Evade: UpdateEvadeState(); break;
                    case FSMState.Attack: UpdateAttackState(); break;
                    case FSMState.Flee: UpdateFleeState(); break;
                    case FSMState.Dead: UpdateDeadState(); break;
                }

                //Vector3 forward = transform.TransformDirection(Vector3.forward) * 100;
                //Debug.DrawRay(transform.position, forward, Color.green);

                Debug.DrawRay(transform.position, forward, Color.green);

                //Vector3 forwardright = transform.TransformDirection(Vector3.forward + new Vector3(1, 0, 0)) * 100;
                Debug.DrawRay(transform.position, forwardright, Color.blue);

                //Vector3 forwardleft = transform.TransformDirection(Vector3.forward + new Vector3(-1, 0, 0)) * 100;
                Debug.DrawRay(transform.position, forwardleft, Color.red);

                //Update the time
                elapsedTime += Time.deltaTime;

                //Go to dead state is no health left
                if (health <= 0)
                    curState = FSMState.Dead;

                /*for (int i = 0; i < enemiesTransform.Length; i++)
                {


                    Edistance[i] = Vector3.Distance(transform.position, enemiesTransform[i].position);
                    if (Edistance[i] <= 100.0f &&  Edistance[i] > 0f)
                    {
                        //Go Backwards
                        //transform.Translate(Vector3.back * Time.deltaTime * curSpeed);
                        curState = FSMState.Evade;
                    }
                }*/
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

        if (left)
        {
            Debug.LogError("LEFT");
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(-45, 0, 0) - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
            left = false;

        } else if (right)
        {
            Debug.LogError("RIGHT");
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(45, 0, 0) - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
            right = false;
        }
        //Set the target position as the player position
        //destPos = playerTransform.position;

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 100;
        //Debug.DrawRay(transform.position, forward, Color.green);

        /*if (Physics.Raycast(transform.position, forward, 10))
        {
            Debug.LogWarning("Green Ray hit.");
            Quaternion targetRotation = Quaternion.LookRotation((destPos - new Vector3(0,-45,0)) - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        }

        Vector3 forwardright = transform.TransformDirection(Vector3.forward + new Vector3(1,0,0)) * 100;
        //Debug.DrawRay(transform.position, forwardright, Color.green);
        if (Physics.Raycast(transform.position, forwardright, 10))
        {
            Debug.LogWarning("Blue Ray hit.");
            Quaternion targetRotation = Quaternion.LookRotation((destPos - new Vector3(0, -45, 0)) - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        }

        Vector3 forwardleft = transform.TransformDirection(Vector3.forward + new Vector3(-1, 0, 0)) * 100;
        //Debug.DrawRay(transform.position, forwardleft, Color.green);
        if (Physics.Raycast(transform.position, forwardright, 10))
        {
            Debug.LogWarning("Red Ray hit.");
            Quaternion targetRotation = Quaternion.LookRotation((destPos - new Vector3(0, 45, 0)) - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        }*/

        //Check distance with other tanks, when none near enough do nothing, if one within a short range; evade
        /*for (int i = 0; i < enemiesTransform.Length; i++)
        {
            

            Edistance[i] = Vector3.Distance(transform.position, enemiesTransform[i].position);
            if (Edistance[i] <= 100.0f && >= 0f)
            {
                //Go Backwards
                //transform.Translate(Vector3.back * Time.deltaTime * curSpeed);
                
            }
        }*/


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
