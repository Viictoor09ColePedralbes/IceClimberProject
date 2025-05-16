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
    [SerializeField] private AudioClip defeatBirdClip;
    private Animator birdAnimator;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        CreateNewPosition();
    }

    void Update()
    {
        Movement();
        if (GameManager.instance.isOnBonusStage)
        {
            Destroy(gameObject);
        }
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
            boxCollider.enabled = false;
            birdAnimator.SetBool("isDead", true);
            AudioManager.instance.PlaySFX(defeatBirdClip);
            GameManager.instance.thingsPoints[2] += 1;
            GameManager.instance.birdAlive = false;
            GameManager.instance.enemiesDefeated += 1;
            StartCoroutine(DeadAnimation());
        }
    }

    private IEnumerator DeadAnimation()
    {
        float elapsedTime = 0, maxTime = 0.75f;
        Vector2 deadPoint = new Vector2(gameObject.transform.position.x, mainCameraTransform.position.y - 6f), initialPos = gameObject.transform.position;
        while (elapsedTime < maxTime)
        {
            gameObject.transform.position = Vector2.Lerp(initialPos, deadPoint, elapsedTime / maxTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
