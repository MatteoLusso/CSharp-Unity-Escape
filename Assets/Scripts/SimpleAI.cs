using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{
    private NavMeshAgent enemyAgent;
    private Animator enemyAnimator;

    public float enemyWalkSpeed;
    public float enemyRunSpeed;

    public MazeGenerator level;
    public bool autoFindLevelLevelGenerator;

    public GameObject player;
    public bool autoFindPlayer;

    private Vector3 target;

    private int line;
    private int column;
    private int x;
    private int y;

    public float minDistance;
    public float maxDistance;

    private float distanceToTarget;
    public float targetRadius;
    private float timer;
    public float maxTimeToReachDestination;

    public float maxChaseDistance;

    private float actionTimer;
    public float actionMaxTime;

    public float maxNoiseDistance;

    private AnimatorStateInfo enemyAnimatorState;
    static int actionState;
    static int walkToActionState;
    static int actionToWalkState;

    static int rollState;

    private bool isChasing;

    public Light searchingLight;
    public float lightIntensity;

    private int layerMask;

    private bool isMovingToRandomDestination;

    private Vector3 forcedDestination;

    void Start()
    {
        if(autoFindLevelLevelGenerator)
        {
            level = GameObject.Find("LevelManager").GetComponent<MazeGenerator>();
        }

        if(autoFindPlayer)
        {
            player = GameObject.Find("Player");
        }

        NavMeshCorrection:

        if(level.IsGenerationEnded())
        {
            this.GetComponent<NavMeshObstacle>().enabled = false;
        }
        else
        {
            goto NavMeshCorrection;
        }

        layerMask = LayerMask.GetMask("Player", "Enemies");
        layerMask = ~layerMask;

        enemyAnimator = this.GetComponent<Animator>();
        enemyAgent = this.GetComponent<NavMeshAgent>();

        distanceToTarget = 0.0f;
        timer = 0.0f;

        actionTimer = Random.Range(0.0f, actionMaxTime/2);

        actionState = Animator.StringToHash("Base Layer.Analyze");
        enemyAnimator.SetBool("IsAnalyzing", false);

        walkToActionState = Animator.StringToHash("Base Layer.WalkToAnalyze");
        actionToWalkState = Animator.StringToHash("Base Layer.AnalyzeToWalk");
        rollState = Animator.StringToHash("Base Layer.Roll");

        isChasing = false;
        enemyAnimator.SetBool("IsChasing", false);

        isMovingToRandomDestination = true;
    }

    private void OnDrawGizmos()
    {
        if(Physics.Linecast(this.transform.position, player.transform.position, layerMask))
        {
            Debug.DrawLine(this.transform.position, player.transform.position, Color.red);
        }
        else
        {
            Debug.DrawLine(this.transform.position, player.transform.position, Color.green);
        }


        Debug.DrawLine(this.transform.position, enemyAgent.destination, Color.blue);

    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Flashlight"))
        {
        // Quando noi o il collider della torcia (quando è accesa) ci troviamo all'interno del collider del nemico, inizia l'inseguimento...
            if(!Physics.Linecast(this.transform.position, player.transform.position, layerMask))
            {
            // ... a meno che non ci sia fra noi e il nemico un qualsiasi ostacolo che interrompe la visuale.
                isChasing = true;
                enemyAnimator.SetBool("IsChasing", true);
                //timerUpdatePosition = 0.0f;
            }
        }
    }

    void Chase()
    {
        enemyAgent.SetDestination(player.transform.position);

        if((this.transform.position - player.transform.position).magnitude >= maxChaseDistance)
        {
            isChasing = false;
            enemyAnimator.SetBool("IsChasing", false);
            // Quando ci allontaniamo abbastanza da un nemico questo mette di inseguirci (la logica di fine inseguimento, come l'uso di nascondigli da parte del player, può essere aggiunta qui).
        }
    }

    void Run()
    {
        enemyAnimator.SetFloat("Speed", enemyRunSpeed);
        enemyAgent.speed = enemyRunSpeed;
        enemyAgent.angularSpeed = 240.0f;

    }

    void GoTo()
    {
        if(enemyAgent.destination != forcedDestination)
        {
            enemyAgent.SetDestination(forcedDestination);
        }

        distanceToTarget = (forcedDestination - this.transform.position).magnitude;

        if(distanceToTarget <= targetRadius)
        {
            isMovingToRandomDestination = true;
            Debug.Log("Raggiunto oggetto rumoroso");
        }
    }

    void LateUpdate()
    {
        
        if(isChasing)
        {
            Chase();
            searchingLight.intensity = 0.0f;
        }
        else
        {
            if(isMovingToRandomDestination)
            {
                Wandering();
            }
            else
            {
                GoTo();
            }

            searchingLight.intensity = lightIntensity;
        }

        if(enemyAgent.pathPending)
        {
            Idle();
        }
        else
        {
            if(isChasing)
            {
                Run();
            }
            else
            {
                Walk(); 
            }
        }
    }

    private void Wandering()
    {
        if(distanceToTarget <= targetRadius || enemyAgent.pathStatus == NavMeshPathStatus.PathInvalid || enemyAgent.pathStatus == NavMeshPathStatus.PathPartial || timer > maxTimeToReachDestination)
        {
            RandomBlock:

            line = Random.Range(0, level.maxX - 1);
            column = Random.Range(0, level.maxY - 1);

            if(level.blockMatrix[line, column] == null)
            {
                goto RandomBlock;
            }

            RandomRoom:
        
            x = Random.Range(0, level.roomSize - 1);
            y = Random.Range(0, level.roomSize - 1);

            if(!level.blockMatrix[line, column].roomsMatrix[x, y].roomExists)
            {
                goto RandomRoom;
            }
            target = level.blockMatrix[line, column].roomsMatrix[x, y].roomTransform.position;
            target = new Vector3(target.x - 0.5f * level.mazeScalingForNavMesh, target.y, target.z + 0.5f * level.mazeScalingForNavMesh);

            distanceToTarget = (target - this.transform.position).magnitude;
            if(distanceToTarget <= minDistance && distanceToTarget >= maxDistance)
            {
                goto RandomBlock;
            }

            enemyAgent.SetDestination(target);
            timer = 0.0f;
        }

        distanceToTarget = (target - this.transform.position).magnitude;
        //Debug.Log(distanceToTarget);
        timer += Time.deltaTime;
    }

    private void Idle()
    {
        enemyAnimator.SetFloat("Speed", 0.0f);
        enemyAgent.speed = 0.0f;
    }
    private void Walk()
    {
        enemyAnimatorState = enemyAnimator.GetCurrentAnimatorStateInfo(0);

        if(actionTimer <= actionMaxTime && enemyAnimatorState.fullPathHash != actionState && enemyAnimatorState.fullPathHash != walkToActionState && enemyAnimatorState.fullPathHash != actionToWalkState)
        {
            enemyAnimator.SetBool("IsAnalyzing", false);

            enemyAnimator.SetFloat("Speed", enemyWalkSpeed);
            enemyAgent.speed = enemyWalkSpeed;
            enemyAgent.angularSpeed = 120.0f;
        }
        else if(actionTimer > actionMaxTime && enemyAnimatorState.fullPathHash != actionState && enemyAnimatorState.fullPathHash != walkToActionState && enemyAnimatorState.fullPathHash != actionToWalkState)
        {
            enemyAnimator.SetBool("IsAnalyzing", true);

            actionTimer = Random.Range(0.0f, actionMaxTime/2);
        }
        else if(enemyAnimatorState.fullPathHash == actionState || enemyAnimatorState.fullPathHash == walkToActionState || enemyAnimatorState.fullPathHash == actionToWalkState)
        {
            enemyAnimator.SetFloat("Speed", 0.0f);
            enemyAgent.speed = 0.0f;
        }

        actionTimer += Time.deltaTime;
        /*Debug.Log(actionTimer + " " + actionMaxTime);
        Debug.Log(enemyAgent.speed);*/

    }

    public void SetDestination(Vector3 destination)
    {
        if(!isChasing)
        {
            Debug.Log("Distanza rumore: " + (forcedDestination - this.transform.position).magnitude);
            if((forcedDestination - this.transform.position).magnitude <= maxNoiseDistance)
            {
                isMovingToRandomDestination = false;
                forcedDestination = destination;

                Debug.Log("Nuova Destinazione");
            }
            else
            {
                Debug.Log("Rumore troppo distante");
            }
        }
    }
}
