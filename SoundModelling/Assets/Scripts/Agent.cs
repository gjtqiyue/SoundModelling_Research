using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SoundSystem;

public class Agent : AgentWithSound 
{
    public enum AgentStatus
    {
        InWaitingTime,
        InPatrolling,
        InSearching,
        InControlled
    }

    public float patrolSpeed;
    public float searchSpeed;
    public float waitTime;
    [Space]
    public bool canMakeSound;
    public SoundType typeToMake;
    public float duration;
    public float volume;
    [Space]
    public float threshold;

    [Space]
    [SerializeField]
    private List<Transform> patrolPoints = new List<Transform>();

    [SerializeField]
    private AgentStatus status;
    private AgentStatus previous_status;

    private int targetPoint;
    private int lastPoint;

    private float waitTimer;

    private AgentSoundComponent soundComp;

    [SerializeField]
    private List<PointIntensity> searchPath;

    [SerializeField]
    private bool loop = false;

    private void Start()
    {
        waitTimer = waitTime;
        lastPoint = targetPoint = -1;
        soundComp = GetComponent<AgentSoundComponent>();
    }

    private void Update()
    {
        if (canMakeSound && !soundComp.IsMakingSound() && SystemController.Instance.IsSimulationOn())
        {
            int t = (int)Random.Range(0, 4);
            SoundType s = (SoundType)t;
            soundComp.MakeSound(gameObject, transform.position, 30, s, 360, 500);
        }

        DoBehaviour();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void DoBehaviour()
    {
        switch (status) {
            case AgentStatus.InPatrolling:

                Patrol();
                break;

            case AgentStatus.InWaitingTime:

                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0)
                {
                    status = AgentStatus.InPatrolling;
                    previous_status = AgentStatus.InWaitingTime;
                    waitTimer = waitTime;
                }
                break;

            case AgentStatus.InSearching:

                if (searchPath == null || searchPath.Count == 0)
                {
                    //exit search mode
                    status = previous_status;
                    previous_status = AgentStatus.InSearching;
                    break;
                }

                // if we are not at the path destination, we move to there
                Vector3 dest = new Vector3(searchPath[searchPath.Count-1].pos.x, 0, searchPath[searchPath.Count - 1].pos.z);
                if (Vector3.SqrMagnitude(dest - transform.position) > threshold)
                {
                    //transform.position = Vector3.MoveTowards(transform.position, dest, searchSpeed);
                    gameObject.GetComponent<NavMeshAgent>().SetDestination(dest);
                    //Debug.Log(dest + " " + Vector3.SqrMagnitude(searchPath[0].pos - transform.position));

                }
                else
                {
                    //searchPath.RemoveAt(0);
                    searchPath.Clear();
                }

                break;

            case AgentStatus.InControlled:

                break;

            default:
                break; 
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count <= 0) return;

        if (targetPoint == -1)
        {
            targetPoint = FindCloestPoint();
            lastPoint = targetPoint - 1 < 0 ? targetPoint + 1 : targetPoint - 1;
        }

        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[targetPoint].position, patrolSpeed);

        if (Vector3.SqrMagnitude(patrolPoints[targetPoint].position - transform.position) < 0.05 && patrolPoints.Count > 1)
        {
            status = AgentStatus.InWaitingTime;
            previous_status = AgentStatus.InPatrolling;
            GetNextTargetPoint();
        }
    }

    private void GetNextTargetPoint()
    {
        int next = targetPoint - lastPoint > 0 ? targetPoint + 1 : targetPoint - 1;
        if (targetPoint == patrolPoints.Count - 1 || targetPoint == 0)
        {
            if (loop)
            {
                targetPoint = (next + patrolPoints.Count) % patrolPoints.Count;
                lastPoint = targetPoint == 0 ? -1 : patrolPoints.Count;
            }
            else
            {
                next = lastPoint;
                lastPoint = targetPoint;
                targetPoint = next;
            }
        }
        else
        {
            lastPoint = targetPoint;
            targetPoint = next;
        }
    }

    private int FindCloestPoint()
    {
        int minPointIndex = 0;
        float minDistance = int.MaxValue;
        for (int i=0; i<patrolPoints.Count; i++)
        {
            float dist = Vector3.SqrMagnitude(patrolPoints[i].position - transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                minPointIndex = i;
            }
        }

        return minPointIndex;
    }

    public override void SearchSoundSource(List<PointIntensity> path)
    {
        if (status != AgentStatus.InSearching)
        {
            previous_status = status;
            status = AgentStatus.InSearching;
        }

        //compare two plan's first intensity to decide if we take the new plan
        if (searchPath == null)
        {
            searchPath = path;
        }
        else if (searchPath.Count == 0)
        {
            searchPath = path;
        }
        else if (path[0].net_intensity > searchPath[0].net_intensity)
        {
            searchPath = path;
        }

    }

}
