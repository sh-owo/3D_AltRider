using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_TrackMovements : MonoBehaviour
{
    private Renderer carRenderer;
    private MaterialPropertyBlock carBlock;
    private Vector2 carCurrentOffset;
    private Vector2 textureSpeed;
    public Car_movement tank;

    // Start is called before the first frame update
    void Start()
    {
        carBlock = new MaterialPropertyBlock();
        carCurrentOffset = new Vector2(0, 0);
        textureSpeed = new Vector2(0.1f * tank.currentAccelerateForce, 0);
        carRenderer = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        textureSpeed = new Vector2(0.1f * tank.currentAccelerateForce, 0);
        carCurrentOffset += Time.deltaTime * textureSpeed;
        carCurrentOffset.x %= 1;
        carBlock.SetVector("_MainTex_ST", new Vector4(1, 1, carCurrentOffset.x, carCurrentOffset.y)); // 오프셋 적용
        carRenderer.SetPropertyBlock(carBlock);
    }
}