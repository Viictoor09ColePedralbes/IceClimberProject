using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public GameObject cloudPrefab;
    public GameObject spawnedCloud;
    private bool doMovement = false;

    [SerializeField]
    float speed;

    [SerializeField]
    Vector2 dir;

    [SerializeField]
    Transform spawnCloud;

    private void Start()
    {
        // Destroy(gameObject, 14f);
    }

    private void Update()
    {
        if(doMovement)
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }
        
        if(GameManager.instance.stageFinished || GameManager.instance.timeBonusStage <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
            Debug.Log("Player sobre plataforma");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("cloudWall"))
        {
            spawnedCloud = Instantiate(cloudPrefab, spawnCloud.position, Quaternion.identity);
            spawnedCloud.GetComponent<CloudController>().DestroyAndMove();
            if (spawnedCloud.GetComponentInChildren<PlayerMovement>()) // Hacemos esto para evitar el bug de duplicado de player
            {
                Destroy(spawnedCloud.GetComponentInChildren<PlayerMovement>().gameObject);
            }
            Destroy(gameObject, 1.5f);
        }
        else if (other.CompareTag("StartCloudMovement"))
        {
            Destroy(gameObject, 14f);
            doMovement = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("vacio"))
        {
            doMovement = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && GetComponentInChildren<PlayerMovement>())
        {
            collision.transform.SetParent(null);
        }
    }

    public void DestroyAndMove()
    {
        Destroy(gameObject,14f);
        doMovement = true;
    }
}