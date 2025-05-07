using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public GameObject cloudPrefab;
    public GameObject spawnedCloud;

    [SerializeField]
    float speed;

    [SerializeField]
    Vector2 dir;

    [SerializeField]
    Transform spawnCloud;

    private void Start()
    {
        Destroy(gameObject, 15);
    }

    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);
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
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

}