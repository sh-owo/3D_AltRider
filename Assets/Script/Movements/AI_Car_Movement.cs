using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;

public class AI_Car_Movement : Agent
{   
    private float time;
    private Vector3 initialPosition;

    private Car_movement carMovement;
    private float previous_distance;
    public List<Transform> checkpointTransforms;
    private Transform finishLine;

    private int currentCheckpointIndex = 0;

    private float checkpointReachDistance = 1f;
    private float timeLimit = 8f;
    private float finishReward = 10f;
    private float checkpointReward = 3f;
    private float wrongDirectionPenalty = -0.1f;
    private float timeoutPenalty = -0.5f;
    private float trackCollisionPenalty = -0.7f;
    private float playerCollisionPenalty = -0.5f;
    private float wrongCheckpointPenalty = -3f;

    private void Awake()
    {
        InitializeCarMovement();
        CheckpointList();
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
        finishLine = checkpointTransforms[checkpointTransforms.Count - 1];
        previous_distance = float.MaxValue;
        initialPosition = transform.position;
        currentCheckpointIndex = 0;
        time = 0f;

        if (carMovement == null)
        {
            Debug.LogError("Car_movement component is still null in Initialize!");
        }
    }
    
    public override void OnEpisodeBegin()
    {
        if (carMovement != null)
        {
            carMovement.ResetCarValues();
            transform.position = initialPosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            currentCheckpointIndex = 0;
            previous_distance = float.MaxValue;
            time = 0f;
        }
        else
        {
            Debug.LogError("carMovement is null in OnEpisodeBegin!");
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carMovement.GetCurrentSteerAngle);
        sensor.AddObservation(carMovement.transform.position);
        sensor.AddObservation(carMovement.transform.rotation);
        sensor.AddObservation(carMovement.GetCurrentSpeed);

        if (currentCheckpointIndex < checkpointTransforms.Count)
        {
            sensor.AddObservation(checkpointTransforms[currentCheckpointIndex].position - transform.position);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (carMovement != null && carMovement.isAIControlled)
        {
            float vertical = actionBuffers.ContinuousActions[0] + 0.5f;
            float horizontal = actionBuffers.ContinuousActions[1];

            if (currentCheckpointIndex < checkpointTransforms.Count)
            {
                Vector3 directionToCheckpoint = (checkpointTransforms[currentCheckpointIndex].position - transform.position).normalized;
                Vector3 carForward = transform.forward;

                // 차량이 체크포인트를 향하도록 회전각도를 조정
                float angleToCheckpoint = Vector3.SignedAngle(carForward, directionToCheckpoint, Vector3.up);
                horizontal = Mathf.Clamp(angleToCheckpoint / 45f, -1f, 1f);  // 방향을 서서히 맞추도록 스케일링
            }

            carMovement.AiControl(vertical, horizontal);

            float currentSpeed = carMovement.GetCurrentSpeed;
            AddReward(currentSpeed * 0.03f);
        }
        else
        {
            Debug.LogError("carMovement is null or not AI controlled in OnActionReceived!");
        }

        if (currentCheckpointIndex < checkpointTransforms.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpointTransforms[currentCheckpointIndex].position);

            if (distanceToCheckpoint < previous_distance + 0.5f)
            {
                AddReward((1f - (distanceToCheckpoint / previous_distance)) * 0.4f);
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            AddReward(playerCollisionPenalty);
        }

        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log("Current Checkpoint Index: " + currentCheckpointIndex);
        
            string checkpointName = other.gameObject.name;

            int checkpointNumber = -1;
            string pattern = @"Checkpoint \((\d+)\)";
            Match match = Regex.Match(checkpointName, pattern);

            if (match.Success)
            {
                checkpointNumber = int.Parse(match.Groups[1].Value);
            }
            else
            {
                Debug.LogError("Checkpoint name is not in the correct format: " + checkpointName);
                return;
            }

            if (currentCheckpointIndex == checkpointNumber)
            {   
                currentCheckpointIndex++;
                AddReward(checkpointReward);
                previous_distance = float.MaxValue;
            }

            time = 0f;
        }
    
        if (other.gameObject.CompareTag("Endline"))
        {
            AddReward(finishReward);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!carMovement.isAIControlled)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Vertical");
            continuousActionsOut[1] = Input.GetAxis("Horizontal");
        }
    }

    public void CheckpointList()
    {
        checkpointTransforms = new List<Transform>();
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        foreach (GameObject checkpoint in checkpoints)
        {
            checkpointTransforms.Add(checkpoint.transform);
        }

        checkpointTransforms.Sort((a, b) => 
        {
            int aNumber = ExtractNumber(a.name);
            int bNumber = ExtractNumber(b.name);
            return aNumber.CompareTo(bNumber);
        });

        Debug.Log("Loaded " + checkpointTransforms.Count + " checkpoints.");
    }

    private int ExtractNumber(string name)
    {
        string numberPart = Regex.Match(name, @"\d+").Value;
        if (int.TryParse(numberPart, out int number))
        {
            return number;
        }
        return 0; 
    }
}
