using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class ParaMeter : MonoBehaviour
{
    public GameObject testTank;
    public TextMeshProUGUI speedText;
    private Rigidbody testTankRb;

    void Start()
    {
        testTankRb = testTank.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = testTankRb.velocity.magnitude;
        
        speedText.text = speed.ToString("F2") + "KpH";
    }
}