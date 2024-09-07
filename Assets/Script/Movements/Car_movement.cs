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
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private Transform[] wheelTransforms;

    [Header("Car Max Value")]
    [SerializeField] private const float maxSpeed = 20.0f;
    private float maxAccelerateForce = 1000f;
    private float maxBrakeForce = 800f;
    private float maxSteerAngle = 30f;

    [Header("Acceleration Value")]
    private float steerRotatePerSecond = 20f;
    private float accelerateForcePerSecond = 700f;
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
        float currentSpeed = carRigidbody.velocity.magnitude;
        float constant = Steeringconstant();

        if (Mathf.Abs(horizontal) >= 0.05f)
        {
            currentSteerAngle = constant * horizontal * steerRotatePerSecond * 10f;
        }
        else
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0, Time.deltaTime * 5f);
        }
        
        // wheelColliders[0].steerAngle = currentSteerAngle;
        // wheelColliders[1].steerAngle = currentSteerAngle;
        // wheelColliders[2].steerAngle = currentSteerAngle;
        // wheelColliders[3].steerAngle = currentSteerAngle;
        //
        carRigidbody.rotation = Quaternion.Euler(carRigidbody.rotation.eulerAngles + new Vector3(0, currentSteerAngle * Time.deltaTime, 0));
        
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