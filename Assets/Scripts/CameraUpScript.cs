using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUpScript : MonoBehaviour
{
    private Camera playerCamera;
    [HideInInspector] public BoxCollider2D boxCollider;
    private const int UP = 3;
    private const float SPEED = 1.1f;
    private bool animationStarted = false;
    private float time = 0;
    private Vector3 newPos;

    [SerializeField] private bool toBonusStage = false;

    void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (animationStarted)
        {
            if(time < 1)
            {
                time += Time.deltaTime * SPEED;
            }
            else if(time >= 1)
            {
                animationStarted = false;
            }
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, newPos, time);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            time = 0;
            animationStarted = true;
            if (toBonusStage)
            {
                newPos = new Vector3(playerCamera.transform.position.x, 27, playerCamera.transform.position.z);
                GameManager.instance.isOnBonusStage = true;
            }
            else
            {
                newPos = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y + UP, playerCamera.transform.position.z);
            }
            
            boxCollider.enabled = false;
        }
    }
}
