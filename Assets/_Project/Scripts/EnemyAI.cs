using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent enemy;

    public Transform player;

    public LayerMask playerObject;

    public Vector3 route;

    public bool routeSet;

    public bool posDirection;

    public float range;

    // a cd time in between attacks
    public float attackTime;
    //whether the enemy has attacked
    public bool attacked;
    //sight range of the enemy - raycasting distance
    public float sightRange;
    //attack range of the enemy
    public float attackRange;
    //if player is insight / in attack range
    public bool playerInSight;
    public bool playerInAttackRange;

    private void Awake() {
        //set player for ray cast identification
        player = GameObject.Find("Player").transform;
        enemy = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerObject);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerObject);

        if (playerInSight == true && playerInAttackRange == true && player.tag =="Player")
        {
            attack();
        }
        else if (playerInSight == true && playerInAttackRange == false && player.tag == "Player")
        {
            moveTowardPlayer();
        }
        else{ 
            patrol();
            //Debug.Log("it is patroling");
        }
    }

    public void patrol() {
        //check if a route is set
        if (routeSet != true)
        {
            //Debug.Log("inside create route");
            createRoute(posDirection);
        }
        if (routeSet == true) {
            //Debug.Log(route);
            enemy.SetDestination(route);
        }
        //check if the route is fully patrolled
        Vector3 remainingDistance = transform.position - route;
        //if the distance is finished then start constructing a new route
        if (remainingDistance.magnitude < 1f) {
            //change direction
            posDirection = !posDirection;
            routeSet = false;
        }
    }

    public virtual void createRoute(bool direction) {
        routeSet = true;

        float x = range;

        if (direction == false) {
            x = -range;
        }
        //Debug.Log(transform.position.x);
        route = new Vector3(transform.position.x+x, transform.position.y, transform.position.z);
    }

    private void attack() {
        transform.LookAt(player);

        if (!attacked) {
            attacked = true;
            Invoke(nameof(resetAttack), attackTime);
        }
    }

    private void resetAttack() {
        attacked = false;
    }

    private void moveTowardPlayer() {
        //this might cause problem of hitting walls, but yuh prototype
        enemy.SetDestination(player.position);
    }



}
