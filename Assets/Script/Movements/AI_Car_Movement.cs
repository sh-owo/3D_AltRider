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

            //TODO 점검기
            // OnDrawGizmos();
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

            // Debug.Log($"Action: V={vertical:F2}, H={horizontal:F2}, Speed={currentSpeed:F2}, Distance={previous_distance:F2}");
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
        Debug.Log($"collided with {other.gameObject.name}, tag: {other.gameObject.tag}");
        Debug.Log($"Currnet Checkpoint Index: {currentCheckpointIndex}, Touched Checkpoint: {other.gameObject.name}");
        
        if(other.gameObject.CompareTag("Track"))
        {
            AddReward(trackCollisionPenalty);
            EndEpisode();
        }
        if (other.gameObject.CompareTag("Player")) { AddReward(playerCollisionPenalty); }

        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log("Current Checkpoint Index: " + currentCheckpointIndex);
            int checkpointNumber = int.Parse(other.gameObject.name.Substring(10));

            if (currentCheckpointIndex == checkpointNumber)
            {   
                currentCheckpointIndex++;
                AddReward(checkpointReward);
                previous_distance = float.MaxValue;
            }
        }
        
        if(other.gameObject.CompareTag("Endline"))
        {
            AddReward(finishReward);
            EndEpisode();
        }
    }
    /*private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"collided with {other.gameObject.name}, tag: {other.gameObject.tag}");
        // 체크포인트 충돌 처리
        if (other.gameObject.name == $"Checkpoint{currentCheckpointIndex}")
        {
            int checkpointNumber = checkpointTransforms.IndexOf(other.transform);  // 충돌한 체크포인트 인덱스
            Debug.Log($"Reached Checkpoint {checkpointNumber}, Next Checkpoint: {currentCheckpointIndex}");
            // 올바른 체크포인트에 도달했는지 확인
            if (checkpointNumber == currentCheckpointIndex)
            {
                // 올바른 체크포인트 도착 시 보상
                AddReward(checkpointReward);
                currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpointTransforms.Count;  // 다음 체크포인트로 이동
                previous_distance = float.MaxValue;  // 거리 초기화
                Debug.Log($"Reached Checkpoint {checkpointNumber}, Next Checkpoint: {currentCheckpointIndex}");
            }
            else
            {
                // 잘못된 순서의 체크포인트 접근 시 페널티
                AddReward(wrongCheckpointPenalty);
                Debug.Log($"Wrong Checkpoint! Expected: {currentCheckpointIndex}, Got: {checkpointNumber}");
            }
        }
        //TODO 플레이어 충돌도 제작해야함

        // 종료선 도착 처리
        if (other.gameObject.CompareTag("Endline"))
        {
            // 종료선 도착 시 보상
            AddReward(finishReward);
            EndEpisode();
        }
    }*/



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

        checkpointTransforms.Sort((a, b) => 
        {
            int aNumber = ExtractNumber(a.name);
            int bNumber = ExtractNumber(b.name);
            return aNumber.CompareTo(bNumber);
        });

        Debug.Log("Loaded " + checkpointTransforms.Count + " checkpoints.");
        for(int i = 0; i < checkpointTransforms.Count; i++)
        {
            Debug.Log($"Checkpoint {i}: {checkpointTransforms[i].name}");
        }
    }

    private int ExtractNumber(string name)
    {
        string numberPart = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        int number;
        if (int.TryParse(numberPart, out number))
        {
            return number;
        }
        return 0; // 숫자를 추출할 수 없는 경우 0을 반환
    }
    
    public void OnDrawGizmos()
    {
        // 체크포인트가 설정되어 있는 경우에만 시각화
        if (checkpointTransforms != null && checkpointTransforms.Count > 0)
        {
            // 현재 체크포인트 인덱스가 유효한지 확인
            if (currentCheckpointIndex >= 0 && currentCheckpointIndex < checkpointTransforms.Count)
            {
                // 현재 위치와 목표 체크포인트 위치를 가져옴
                Vector3 currentPosition = transform.position;
                Vector3 checkpointPosition = checkpointTransforms[currentCheckpointIndex].position;

                // Gizmos를 사용하여 직선을 그려 현재 위치와 체크포인트를 연결함
                Gizmos.color = Color.red;  // 선 색상 설정
                Gizmos.DrawLine(currentPosition, checkpointPosition);
            }
        }
    }
}