using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_movement : MonoBehaviour
{
    [Header("Car Component")]
    Rigidbody carRigidbody;
    private float mass = 20f;
    [SerializeField] private Transform carTransform;
    [SerializeField] private Transform[] wheels;

    [Header("Car Max Value")]
    private float maxSpeed = 50f;
    private float maxAccelerateForce = 1000f;
    private float minAccelerateForce = 800f;
    private float maxSteerAngle = 30f;

    [Header("Acceleration Value")]
    private float steerRotatePerSecond = 20f;
    private float accelerateForcePerSecond = 700f;
    private float deAccelerateForcePerSecond = 300f;

    [Header("Car Current Value")]
    public float currentSteerAngle = 0f;
    public float currentAccelerateForce = 0f;
    public float currentSpeed = 0f;
    
    public bool isAIControlled = false;
    
    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carTransform = GetComponent<Transform>();
        carRigidbody.mass = mass;
    }

    void FixedUpdate()
    {
        if (!isAIControlled)
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            Accelerate(vertical);
            Steering(horizontal);
            Flip();
        }
        
        // 현재 속도 업데이트
        currentSpeed = GetSpeed();
        
        // 디버그 정보 출력
        // Debug.Log($"Car Speed: {currentSpeed}, Position: {transform.position}");
    }
    
    public float GetSpeed() => carRigidbody.velocity.magnitude;


    private void Flip()
    {
        if (!AreWheelsOnGround() && Input.GetKeyDown(KeyCode.R))
        {
            carTransform.position = new Vector3(carTransform.position.x, carTransform.position.y + 1f, carTransform.position.z);
            carTransform.rotation = Quaternion.Euler(carTransform.rotation.x, carTransform.rotation.eulerAngles.y, 0);
        }
    }
    private bool AreWheelsOnGround()
    {
        foreach (var wheel in wheels)
        {
            if (Physics.Raycast(wheel.position, -transform.up, out RaycastHit hit, 0.5f))
            {
                if (hit.collider != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Accelerate(float vertical)
    {
        if (!AreWheelsOnGround()) return;
        
        if (vertical > 0.05f)
        {
            if (currentAccelerateForce < 0) { currentAccelerateForce = -0.4f * currentAccelerateForce; }
            currentAccelerateForce += accelerateForcePerSecond * vertical * Time.fixedDeltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, -minAccelerateForce, maxAccelerateForce);
        }
        else if (vertical < -0.05f)
        {
            if (currentAccelerateForce > 0) { currentAccelerateForce = -0.4f * currentAccelerateForce; }
            currentAccelerateForce += deAccelerateForcePerSecond * vertical * Time.fixedDeltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, -minAccelerateForce, maxAccelerateForce);
        }
        else
        {
            currentAccelerateForce = Mathf.Lerp(currentAccelerateForce, 0, Time.fixedDeltaTime * 5f);
        }

        // 속도 제한 확인
        if (currentSpeed < maxSpeed)
        {
            Vector3 forceVector = transform.forward * currentAccelerateForce;
            carRigidbody.AddForce(forceVector, ForceMode.Force);
            Debug.Log($"Applied force: {forceVector.magnitude}");
        }
        else
        {
            // 최대 속도에 도달했을 때 속도를 유지
            Vector3 clampedVelocity = Vector3.ClampMagnitude(carRigidbody.velocity, maxSpeed);
            carRigidbody.velocity = clampedVelocity;
        }

        // Debug.Log($"Accelerate - Vertical: {vertical}, Force: {currentAccelerateForce}");
    }

    public float Steeringconstant()
    {
        float speedFactor = currentSpeed / maxSpeed * 10f;
        float constant = 0f;

        if (speedFactor < 1 && speedFactor >= 0)
        {
            constant = Mathf.Sin(speedFactor * Mathf.PI / 2);
        }
        else if (speedFactor >= 1)
        {
            constant = Mathf.Sin(0.164f * speedFactor + 1.5f);
            Mathf.Max(constant, 0.1f);
        }

        return constant;
    }
    public void Steering(float horizontal)
    {
        float constant = Steeringconstant();

        if (Mathf.Abs(horizontal) >= 0.05f)
        {
            currentSteerAngle = constant * horizontal * steerRotatePerSecond * 10f;
        }
        else
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0, Time.fixedDeltaTime * 5f);
        }

        Quaternion steerRotation = Quaternion.Euler(0, currentSteerAngle * Time.fixedDeltaTime, 0);
        carRigidbody.MoveRotation(carRigidbody.rotation * steerRotation);

        Debug.Log($"Steering - Horizontal: {horizontal}, Angle: {currentSteerAngle}");
    }
    
    public float GetCurrentSpeed { get { return currentSpeed; } }

    public float GetCurrentSteerAngle { get {return currentSteerAngle;} }
    
    // public float GetMaxSteerAngle() => maxSteerAngle;
    // public float GetMaxAccelerateForce() => maxAccelerateForce;

    public void SetCurrentAccelerateForce(float value) => currentAccelerateForce = value;
    public void SetCurrentSteerAngle(float value) => currentSteerAngle = value;

    public void ResetCarValues()
    {

    }
}