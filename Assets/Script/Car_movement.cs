using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_movement : MonoBehaviour
{
    [Header("Car Component")]
    Rigidbody carRigidbody;
    [SerializeField] private float mass = 10f;
    [SerializeField] private Transform carTransform;
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private Transform[] wheelTransforms;

    [Header("Car Max Value")]
    private float maxSpeed = 100f;
    private float maxAccelerateForce = 1000f;
    private float maxBrakeForce = 800f;
    private float maxSteerAngle = 30f;

    [Header("Acceleration Value")]
    private float SteerRotatePerSecond = 20f;
    private float AccelerateForcePerSecond = 1000f;
    private float BrakeForcePerSecond = 40f;

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
        Accelerate();
        Steering();
    }

    public float GetSpeed() => carRigidbody.velocity.magnitude;

    public void Accelerate()
    {
        currentSpeed = GetSpeed();

        float vertical = Input.GetAxis("Vertical");
        if (vertical > 0.05f)
        {
            currentAccelerateForce += AccelerateForcePerSecond * vertical * Time.deltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, 0, maxAccelerateForce);
        }
        else if (vertical < -0.05f)
        {
            currentBrakeForce += BrakeForcePerSecond * Mathf.Abs(vertical) * Time.deltaTime;
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

        Debug.Log("currentAccelerateForce: " + currentAccelerateForce + " vertical:" + vertical + " currentSpeed: " + currentSpeed);
    }

    private void Steering()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            currentSteerAngle += SteerRotatePerSecond * horizontal * Time.deltaTime;
            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteerAngle, maxSteerAngle);
        }
        else
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0, Time.deltaTime * (currentAccelerateForce * 0.02f));
        }

        wheelColliders[0].steerAngle = currentSteerAngle;
        wheelColliders[1].steerAngle = currentSteerAngle;

        carRigidbody.AddTorque(currentSteerAngle * carTransform.up * currentAccelerateForce * 0.1f);

        Debug.Log("currentSteerAngle: " + currentSteerAngle + " horizontal:" + Mathf.Abs(horizontal));
    }

    public float GetCurrentSpeed() => currentSpeed;
    public float GetCurrentSteerAngle() => currentSteerAngle;
    public float GetCurrentAccelerateForce() => currentAccelerateForce;
    public float GetMaxSteerAngle() => maxSteerAngle;
    public float GetMaxAccelerateForce() => maxAccelerateForce;

    public void SetCurrentAccelerateForce(float value) => currentAccelerateForce = value;
    public void SetCurrentSteerAngle(float value) => currentSteerAngle = value;

    public void ResetCarValues()
    {
        currentSpeed = 0f;
        currentSteerAngle = 0f;
        currentAccelerateForce = 0f;
    }
}