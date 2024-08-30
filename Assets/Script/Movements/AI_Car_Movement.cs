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

    public void Checkpoint_List()
    {
        checkpointTransforms = new List<Transform>();
        for (int index = 0; index <= 4; index++)
        {
            string checkpointName = "Checkpoint " + index;

            GameObject checkpointObject = GameObject.Find(checkpointName);

            if (checkpointObject != null)
            {
                checkpointTransforms.Add(checkpointObject.transform);
                Debug.Log("Loaded " + checkpointName + " checkpoints.");
            }
            else
            {
                Debug.LogWarning("GameObject not found: " + checkpointName);
            }
        }
    }

    public override void Initialize()
    {
        finishLine = GameObject.Find("FinishLine").transform;
        Checkpoint_List();
        carMovement = GetComponent<Car_movement>();
        previous_distance = float.MaxValue;
        initialPosition = transform.position;
        currentindex = 0;
        time = 0f;
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

            if (distanceToCheckpoint < 1f)
            {
                currentindex++;
                time = 0f;
            }

            if (currentindex == checkpointTransforms.Count)
            {
                float distanceToFinish = Vector3.Distance(transform.position, finishLine.position);
                if (distanceToFinish < 1f)
                {
                    SetReward(5f);
                    Debug.Log("Finished");
                    EndEpisode();
                }
            }

            if (distanceToCheckpoint < previous_distance)
            {
                if (distanceToCheckpoint < 5f)
                {
                    SetReward(1f);
                }
            }
            else
            {
                SetReward(-1f);
            }

            if (time > 10f)
            {
                SetReward(-0.5f);
                EndEpisode();
            }

            time += Time.deltaTime;
            previous_distance = distanceToCheckpoint;
        }
    }

    public override void OnEpisodeBegin()
    {
        carMovement.ResetCarValues();
        carMovement.
        transform.position = initialPosition; 
        currentindex = 0;
        previous_distance = float.MaxValue;
        time = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(carMovement.GetCurrentSpeed); 
        sensor.AddObservation(carMovement.GetCurrentSteerAngle);
        sensor.AddObservation(carMovement.transform.position);
        sensor.AddObservation(carMovement.transform.rotation);
        if (currentindex < checkpointTransforms.Count)
        {
            sensor.AddObservation(checkpointTransforms[currentindex].position);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track"))
        {
            SetReward(-0.3f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}