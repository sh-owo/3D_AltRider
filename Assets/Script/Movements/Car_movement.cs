using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_movement : MonoBehaviour
{
    [Header("Car Component")]
    Rigidbody carRigidbody;
    [SerializeField] private float mass = 100f;
    [SerializeField] private Transform carTransform;
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private Transform[] wheelTransforms;

    [Header("Car Max Value")]
    private float maxSpeed = 100f;
    private float maxAccelerateForce = 1000f;
    private float maxBrakeForce = 800f;
    private float maxSteerAngle = 30f;

    [Header("Acceleration Value")]
    private float steerRotatePerSecond = 20f;
    private float accelerateForcePerSecond = 1000f;
    private float brakeForcePerSecond = 40f;

    [Header("Car Current Value")]
    [SerializeField] private float currentSteerAngle = 0f;
    [SerializeField] private float currentAccelerateForce = 0f;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float currentBrakeForce = 0f;

    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carTransform = GetComponent<Transform>();
        wheelColliders = GetComponentsInChildren<WheelCollider>();
        wheelTransforms = GetComponentsInChildren<Transform>();

        carRigidbody.mass = mass;
    }

    private void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        Accelerate(vertical);
        Steering(horizontal);
    }

    public float GetSpeed() => carRigidbody.velocity.magnitude;

    public void Accelerate(float vertical)
    {
        currentSpeed = GetSpeed();
        if (vertical > 0.05f)
        {
            currentAccelerateForce += accelerateForcePerSecond * vertical * Time.deltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, 0, maxAccelerateForce);
        }
        else if (vertical < -0.05f)
        {
            currentBrakeForce += brakeForcePerSecond * Mathf.Abs(vertical) * Time.deltaTime;
            currentBrakeForce = Mathf.Clamp(currentBrakeForce, 0, maxBrakeForce);
        }
        else
        {
            currentAccelerateForce = Mathf.Lerp(currentAccelerateForce, 0, Time.deltaTime * 5f);
            currentBrakeForce = 0f;
        }
        if (currentSpeed < maxSpeed)
        {
            carRigidbody.AddForce(currentAccelerateForce * transform.forward);
        }

        // Debug.Log("currentAccelerateForce: " + currentAccelerateForce + " vertical:" + vertical + " currentSpeed: " + currentSpeed);
    }

    public void Steering(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            currentSteerAngle += steerRotatePerSecond * horizontal * Time.deltaTime;
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteerAngle, maxSteerAngle);
        }
        else
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0, Time.deltaTime * (currentAccelerateForce * 0.02f));
        }

        // wheelColliders[0].steerAngle = currentSteerAngle;
        // wheelColliders[1].steerAngle = currentSteerAngle;
        
        float torque = currentSteerAngle * currentAccelerateForce * 0.1f;

        carRigidbody.AddTorque(carTransform.up * torque);

        // Debug.Log("currentSteerAngle: " + currentSteerAngle + " horizontal:" + Mathf.Abs(horizontal));
    }
    
    public float GetCurrentSpeed { get { return currentSpeed; } }

    public float GetCurrentSteerAngle { get {return currentSteerAngle;} }
    
    // public float GetMaxSteerAngle() => maxSteerAngle;
    // public float GetMaxAccelerateForce() => maxAccelerateForce;

    public void SetCurrentAccelerateForce(float value) => currentAccelerateForce = value;
    public void SetCurrentSteerAngle(float value) => currentSteerAngle = value;

    public void ResetCarValues()
    {
        currentSpeed = 0f;
        currentSteerAngle = 0f;
        currentAccelerateForce = 0f;
    }
}