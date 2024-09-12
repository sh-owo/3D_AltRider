using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
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
    private float timeLimit = 100f;
    private float finishReward = 5f;
    private float checkpointReward = 1f;
    private float wrongDirectionPenalty = -0.1f;
    private float timeoutPenalty = -0.5f;
    private float trackCollisionPenalty = -0.7f;
    private float playerCollisionPenalty = -0.5f;
    
    private void Awake()
    {
        InitializeCarMovement();
    }

    private void InitializeCarMovement()
    {
        carMovement = GetComponent<Car_movement>();
        if (carMovement == null)
        {
            Debug.LogError("Car_movement component not found on this GameObject!");
        }
        else
        {
            carMovement.isAIControlled = true;
        }
    }

    public override void Initialize()
    {
        finishLine = GameObject.Find("FinishLine")?.transform;
        CheckpointList();
        previous_distance = float.MaxValue;
        initialPosition = transform.position;
        currentindex = 0;
        time = 0f;

        if (carMovement == null)
        {
            Debug.LogError("Car_movement component is still null in Initialize!");
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (carMovement != null && carMovement.isAIControlled)
        {
            float vertical = actionBuffers.ContinuousActions[0] * 2f - 1f;  // [-1, 1] 범위로 스케일링
            float horizontal = actionBuffers.ContinuousActions[1] * 2f - 1f;

            carMovement.AiControl(vertical, horizontal);
        
            float currentSpeed = carMovement.GetCurrentSpeed;
            float speedReward = currentSpeed * 0.1f;
            AddReward(speedReward);

            Debug.Log($"Action: V={vertical:F2}, H={horizontal:F2}, Speed={currentSpeed:F2}, Reward={speedReward:F2}");
        }
        else
        {
            Debug.LogError("carMovement is null or not AI controlled in OnActionReceived!");
            return;
        }
        
        AddReward(carMovement.GetCurrentSpeed*0.5f);

        if (currentindex < checkpointTransforms.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpointTransforms[currentindex].position);

            if (distanceToCheckpoint < checkpointReachDistance)
            {
                currentindex++;
                time = 0f;
                SetReward(checkpointReward);
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
            
            if (distanceToCheckpoint < previous_distance+0.5f)
            {
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
        if (carMovement != null)
        {
            carMovement.currentSpeed = 0f;
            carMovement.currentSteerAngle = 0f;
            carMovement.currentAccelerateForce = 0f;
        }
        else
        {
            Debug.LogError("carMovement is null in OnEpisodeBegin!");
        }

        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        currentindex = 0;
        previous_distance = float.MaxValue;
        time = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carMovement.GetCurrentSteerAngle);
        sensor.AddObservation(carMovement.transform.position);
        sensor.AddObservation(carMovement.transform.rotation);
        sensor.AddObservation(carMovement.GetCurrentSpeed);
        if (currentindex < checkpointTransforms.Count)
        {
            sensor.AddObservation(checkpointTransforms[currentindex].position - transform.position);
        }
    }

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
        if (!carMovement.isAIControlled)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Vertical");
            continuousActionsOut [1] = Input.GetAxis("Horizontal");
        }
        // AI 모드일 때는 아무 것도 하지 않음
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