using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float timeScale = 1.0f;
    public float time = 0.0f;
    public static GameManager Instance { get; private set; }
    public GameManager()
    {
        Instance = this;
    }
    void Start()
    {
        Time.timeScale = timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
    }
}
