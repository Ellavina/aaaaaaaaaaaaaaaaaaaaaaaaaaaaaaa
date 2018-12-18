using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CompleteProject
{

    public class ClickToMove : MonoBehaviour
    {
        public float stopbefore = .25f;
        public float aaRange = 10f;
        public float aaSpeed = .5f;
        public PlayerShooting shootingScript;

        private Animator anim;
        private CharacterController controller;
        private UnityEngine.AI.NavMeshAgent navMeshAgent;
        private Transform targetedEnemy; //stores targeted enemy location so player can move towards them
        private Ray shootRay; //check if we have hit enemy
        private RaycastHit shootHit; //store information about what we hit, beyond a simple bool
        private bool walking; //control walk animation on/off
        private bool enemyClicked; // t/f clicked on enemy instead of ground
        private float nextFire; // based on AS, can we attack again (yet)
        private bool autobreak = false;

        // Use this for initialization
        void Awake()
        {
            anim = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            controller = GetComponent<CharacterController>();
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;

        }

        // Update is called once per frame
        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // pass a ray from mousePos into the scene
            RaycastHit hit;
            if (Input.GetButtonDown("Fire2"))
            {

                // Debug.Log(navMeshAgent.remainingDistance);
                // Debug.Log(navMeshAgent.destination);

                if (Physics.Raycast(ray, out hit, 1000))//store hit info in out hit. instead of just returning bool, additional info is returned
                {
                    transform.LookAt(hit.point);

                    if (hit.collider.CompareTag("Enemy"))
                    { //check if obj tag = Enemy
                        targetedEnemy = hit.transform; // set enemy variable to the transform of enemy you clicked on
                        enemyClicked = true;
                    }

                    else
                    {
                        walking = true;
                        enemyClicked = false;
                        navMeshAgent.isStopped = false;
                        // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                        Vector3 playerToMouse = hit.point - transform.position;

                        // Ensure the vector is entirely along the floor plane.
                        playerToMouse.y = 0f;

                        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                        Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);

                        // Set the player's rotation to this new rotation.
                        Quaternion.SlerpUnclamped(transform.rotation, newRotatation, 100);
                        //playerRigidbody.MoveRotation (newRotatation);
                        controller.SimpleMove(transform.forward);
                        navMeshAgent.destination = hit.point;
                        Debug.Log(navMeshAgent.destination);

                    }
                }
            }
            /*
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || Mathf.Abs(navMeshAgent.velocity.sqrMagnitude) < float.Epsilon)
                    Stopall();
            }
            else
            {
                walking = true;
            }
            */

            if (enemyClicked)
            {
                MoveAndShoot();
            }
                     

            if ((navMeshAgent.remainingDistance < stopbefore) && (autobreak)) 
            {
                Debug.Log("Called");
                Stopall();
            }
            

            Debug.Log("remain distance " + navMeshAgent.remainingDistance);
            //Debug.Log(navMeshAgent.destination);

            anim.SetBool("IsWalking", walking);
        }

        private void Stopall()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        private void MoveAndShoot()
        {
            if (targetedEnemy == null)
                return;
            navMeshAgent.destination = targetedEnemy.position;
            if (navMeshAgent.remainingDistance >= aaRange)
            {

                navMeshAgent.isStopped = false;
                walking = true;
            }

            if (navMeshAgent.remainingDistance <= aaRange)
            {

                transform.LookAt(targetedEnemy);
                Vector3 dirToShoot = targetedEnemy.transform.position - transform.position;
                if (Time.time > nextFire)
                {
                    nextFire = Time.time + aaSpeed;
                    shootingScript.Shoot(dirToShoot);
                }
                navMeshAgent.isStopped = true;
                walking = false;
            }
        }



    }

}
