using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_movement : MonoBehaviour
{
    [SerializeField] private Transform carTransform;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = carTransform.position;
        transform.rotation = carTransform.rotation;
    }
}
