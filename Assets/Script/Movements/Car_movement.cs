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
    private float maxSpeed = 40f;
    private float maxAccelerateForce = 1000f;
    private float minAccelerateForce = 800f;

    [Header("Acceleration Value")]
    private float steerRotatePerSecond = 30f;
    private float accelerateForcePerSecond = 7000f;
    private float deAccelerateForcePerSecond = 300f;

    [Header("Car Current Value")]
    public float currentSteerAngle = 0f;
    public float currentAccelerateForce = 0f;
    public float currentSpeed = 0f;
    public bool isAIControlled = false;
    private Vector3 adjustTrackPosition = new Vector3(0, 2.75f, -4);
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
            // Debug.DrawRay(wheel.position + adjustTrackPosition, -transform.up * 0.5f, Color.yellow, 300f);
            if (Physics.Raycast(wheel.position + adjustTrackPosition, -transform.up * 0.5f, out RaycastHit hit, 300f))
            {
                if (hit.collider != null)
                {
                    // Debug.Log("wheels on the ground");
                    return true;
                }
            }
        }
        // Debug.Log("No wheels on the ground");
        return false;
    }

    public void Accelerate(float vertical)
    {
        if (!AreWheelsOnGround()) return;

        currentSpeed = GetSpeed();
        if (vertical > 0.05f)
        {
            if (currentAccelerateForce < 0) { currentAccelerateForce = -0.4f * currentAccelerateForce; }
            currentAccelerateForce += accelerateForcePerSecond * vertical * Time.deltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, -minAccelerateForce, maxAccelerateForce);
        }
        else if (vertical < -0.05f)
        {
            if (currentAccelerateForce > 0) { currentAccelerateForce = -0.4f * currentAccelerateForce; }
            currentAccelerateForce += deAccelerateForcePerSecond * vertical * Time.deltaTime;
            currentAccelerateForce = Mathf.Clamp(currentAccelerateForce, -minAccelerateForce, maxAccelerateForce);
        }
        else
        {
            currentAccelerateForce = Mathf.Lerp(currentAccelerateForce, 0, Time.deltaTime * 5f);
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
            constant = Mathf.Sin(0.14f * speedFactor + 1.5f);
            Mathf.Max(constant, 0.5f);
        }

        return constant;
    }

    public void Steering(float horizontal)
    {
        float currentSpeed = carRigidbody.velocity.magnitude;
        float constant = Steeringconstant();
        


        if (Mathf.Abs(horizontal) >= 0.05f) { currentSteerAngle = constant * horizontal * steerRotatePerSecond * 10f; }
        else { currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0, Time.deltaTime * 5f); }
        this.carRigidbody.rotation = Quaternion.Euler(carRigidbody.rotation.eulerAngles + new Vector3(0, currentSteerAngle * Time.deltaTime, 0));
    }
    
    public void AiControl(float vertical, float horizontal)
    {
        Accelerate(vertical);
        Steering(horizontal);
    }
    
    public float GetCurrentSpeed { get { return currentSpeed; } }

    public float GetCurrentSteerAngle { get {return currentSteerAngle;} }

    public void SetCurrentAccelerateForce(float value) => currentAccelerateForce = value;
    public void SetCurrentSteerAngle(float value) => currentSteerAngle = value;

    public void ResetCarValues()
    {
        currentSpeed = 0f;
        currentSteerAngle = 0f;
        currentAccelerateForce = 0f;
        carRigidbody.rotation = Quaternion.Euler(0, 0, 0);
        carRigidbody.velocity = Vector3.zero;
    }
}