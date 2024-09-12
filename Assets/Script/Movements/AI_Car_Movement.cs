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
            carMovement.currentSpeed = 0f;
            carMovement.currentSteerAngle = 0f;
            carMovement.currentAccelerateForce = 0f;
        }
        else
        {
            Debug.LogError("carMovement is null in OnEpisodeBegin!");
        }
        carMovement.ResetCarValues();
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        currentCheckpointIndex = 0;
        previous_distance = float.MaxValue;
        time = 0f;
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

            // 체크포인트 방향 계산
            if (currentCheckpointIndex < checkpointTransforms.Count)
            {
                Vector3 directionToCheckpoint = (checkpointTransforms[currentCheckpointIndex].position - transform.position).normalized;
                Vector3 carForward = transform.forward;

                // 차량이 체크포인트를 향하도록 회전각도를 조정
                float angleToCheckpoint = Vector3.SignedAngle(carForward, directionToCheckpoint, Vector3.up);
                horizontal = Mathf.Clamp(angleToCheckpoint / 45f, -1f, 1f);  // 방향을 서서히 맞추도록 스케일링
            }

            // 차량 제어
            carMovement.AiControl(vertical, horizontal);

            float currentSpeed = carMovement.GetCurrentSpeed;
            float speedReward = currentSpeed * 0.03f;
            AddReward(speedReward);

            Debug.Log($"Action: V={vertical:F2}, H={horizontal:F2}, Speed={currentSpeed:F2}, Distance={previous_distance:F2}");
        }
        else
        {
            Debug.LogError("carMovement is null or not AI controlled in OnActionReceived!");
            return;
        }

        // 체크포인트 처리
        if (currentCheckpointIndex < checkpointTransforms.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpointTransforms[currentCheckpointIndex].position);

            if (distanceToCheckpoint < previous_distance + 0.5f)
            {
                float rewardMultiplier = 1f - (distanceToCheckpoint / previous_distance);
                AddReward(rewardMultiplier * 0.4f);
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
        // 트리거된 오브젝트 로그 출력
        // Debug.Log("Trigger Detected with: " + other.gameObject.name);

        // 트랙 충돌 처리
        if (other.gameObject.CompareTag("Track"))
        {
            AddReward(trackCollisionPenalty);
            EndEpisode();
        }

        // 플레이어 충돌 처리
        if (other.gameObject.CompareTag("Player"))
        {
            AddReward(playerCollisionPenalty);
        }

        // 체크포인트 충돌 처리
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            string checkpointName = other.gameObject.name;
            int checkpointNumber = int.Parse(checkpointName.Substring("Checkpoint".Length));
            Debug.Log($"Current checkpoint: {checkpointNumber}, Expected: {currentCheckpointIndex}");

            // 현재 체크포인트와 비교하여 올바른 순서인지 확인
            if (checkpointNumber == currentCheckpointIndex)
            {
                currentCheckpointIndex++;
                AddReward(checkpointReward);  // 올바른 체크포인트 도착 시 보상
                time = 0f;                    // 시간 초기화
                previous_distance = float.MaxValue;  // 이전 거리 초기화

                // 다음 체크포인트로 업데이트
                // currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpointTransforms.Count;

                // Debug.Log($"Reached Checkpoint {checkpointNumber}, Next: {currentCheckpointIndex}");
            }
            else
            {
                // 잘못된 순서의 체크포인트 접근 시 페널티
                AddReward(wrongCheckpointPenalty);
                Debug.Log($"Wrong Checkpoint! Expected: {currentCheckpointIndex}, Got: {checkpointNumber}");
            }
        }

        // 종착선 도착 처리
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
        // AI 모드일 때는 아무 것도 하지 않음
    }

    public void CheckpointList()
    {
        checkpointTransforms = new List<Transform>();
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        foreach (GameObject checkpoint in checkpoints)
        {
            checkpointTransforms.Add(checkpoint.transform);
        }

        checkpointTransforms.Sort((a, b) => a.name.CompareTo(b.name));
        Debug.Log("Loaded " + checkpointTransforms.Count + " checkpoints.");
    }
}