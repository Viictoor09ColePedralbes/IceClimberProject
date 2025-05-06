using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBehaviour : MonoBehaviour
{
    private Transform mainCameraTransform;
    private Vector3 newPoint;

    private Vector3 initialPos;
    private float elapsedTime = 0;
    private const float X_MAX = 8.2f, Y_MAX = 4.3f, MAX_TIME = 6;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateNewPosition();
    }

    void Update()
    {
        Movement();
    }

    private void CreateNewPosition()
    {
        newPoint = new Vector3(Random.Range(-X_MAX, X_MAX), Random.Range(-Y_MAX + mainCameraTransform.position.y, Y_MAX + mainCameraTransform.position.y), 0);
        initialPos = gameObject.transform.position;
    }

    private void Movement()
    {
        if (elapsedTime < MAX_TIME)
        {
            gameObject.transform.position = Vector3.Lerp(initialPos, newPoint, elapsedTime / MAX_TIME);
            elapsedTime += Time.deltaTime;

        }
        else if (elapsedTime >= MAX_TIME)
        {
            CreateNewPosition();
            elapsedTime = 0;
        }

        if (newPoint.x > initialPos.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            GameManager.instance.BirdKilled();
        }
    }
}
