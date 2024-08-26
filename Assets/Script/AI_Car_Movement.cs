using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AI_Car_Movement : Agent
{
    private Car_movement carMovement;

    public override void Initialize()
    {
        carMovement = GetComponent<Car_movement>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var vectorAction = actionBuffers.ContinuousActions;
        carMovement.SetCurrentAccelerateForce(vectorAction[0] * carMovement.GetMaxAccelerateForce());
        carMovement.SetCurrentSteerAngle(vectorAction[1] * carMovement.GetMaxSteerAngle());
    }

    public override void OnEpisodeBegin()
    {
        carMovement.ResetCarValues();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carMovement.GetCurrentSpeed());
        sensor.AddObservation(carMovement.GetCurrentSteerAngle());
        sensor.AddObservation(carMovement.GetCurrentAccelerateForce());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}