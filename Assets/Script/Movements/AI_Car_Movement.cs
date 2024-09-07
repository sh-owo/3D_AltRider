using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEngine;

public class AI_Car_Movement : Agent
{
    private float time;
    private Vector3 initialPosition;

    private Car_movement carMovement;
    private float previous_distance;
    private List<Transform> checkpointTransforms;
    private Transform finishLine;

    private int currentindex = 0;
    
    private float checkpointReachDistance = 1f;
    private float timeLimit = 10f;
    private float finishReward = 5f;
    private float checkpointReward = 1f;
    private float wrongDirectionPenalty = -0.1f;
    private float timeoutPenalty = -0.5f;
    private float trackCollisionPenalty = -0.7f;
    private float playerCollisionPenalty = -0.5f;

    public override void Initialize()
    {
        finishLine = GameObject.Find("FinishLine")?.transform;
        CheckpointList();
        previous_distance = float.MaxValue;
        initialPosition = transform.position;
        currentindex = 0;
        time = 0f;
        carMovement = GetComponent<Car_movement>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float vertical = actionBuffers.ContinuousActions[0];
        float horizontal = actionBuffers.ContinuousActions[1];

        carMovement.Accelerate(vertical);
        carMovement.Steering(horizontal);

        if (currentindex < checkpointTransforms.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpointTransforms[currentindex].position);

            if (distanceToCheckpoint < checkpointReachDistance)
            {
                currentindex++;
                time = 0f;
                SetReward(checkpointReward);  // 수정함! 체크포인트 도달 시 보상
            }

            if (currentindex == checkpointTransforms.Count)
            {
                float distanceToFinish = Vector3.Distance(transform.position, finishLine.position);
                if (distanceToFinish < 0.1f)
                {
                    SetReward(finishReward);
                    Debug.Log("Finished");
                    EndEpisode();
                }
            }

            if (distanceToCheckpoint < previous_distance)
            {
                // 수정함! 거리에 따른 세밀한 보상
                float rewardMultiplier = 1f - (distanceToCheckpoint / previous_distance);
                AddReward(rewardMultiplier * 0.1f);
            }
            else
            {
                AddReward(wrongDirectionPenalty * Time.deltaTime);
            }

            if (time > timeLimit)
            {
                AddReward(timeoutPenalty);
                EndEpisode();
            }

            time += Time.deltaTime;
            previous_distance = distanceToCheckpoint;
        }
    }

    public override void OnEpisodeBegin()
    { 
        // carMovement.currentSpeed = 0f;
        // carMovement.currentSteerAngle = 0f;
        // carMovement.currentAccelerateForce = 0f;
        transform.position = initialPosition; 
        currentindex = 0;
        previous_distance = float.MaxValue;
        time = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(carMovement.GetCurrentSpeed);
        sensor.AddObservation(carMovement.GetCurrentSteerAngle);//1
        sensor.AddObservation(carMovement.transform.position);//3
        sensor.AddObservation(carMovement.transform.rotation);//4
        if (currentindex < checkpointTransforms.Count)
        {
            sensor.AddObservation(checkpointTransforms[currentindex].position - transform.position);//3
        }
    }//1+3+4+3 = 11

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track"))
        {
            AddReward(trackCollisionPenalty);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            AddReward(playerCollisionPenalty);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
    
    public void CheckpointList()
    {
        checkpointTransforms = new List<Transform>();
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");
    
        foreach (GameObject checkpoint in checkpoints)
        {
            checkpointTransforms.Add(checkpoint.transform);
        }
    
        checkpointTransforms.Sort((a, b) => a.name.CompareTo(b.name));
        Debug.Log("Loaded " + checkpointTransforms.Count + " checkpoints.");
    }
}