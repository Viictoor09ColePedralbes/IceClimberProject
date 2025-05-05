using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    [SerializeField]
    float speed;

    private Vector2 dir = Vector2.right;

    public Collider2D wrapCollider; // Asigna el Collider que define los límites
    public float buffer = 0.5f; // Espacio antes de duplicarse
    private GameObject clone;

    void Update()
    {
        CheckWrap();
        transform.Translate(dir * speed * Time.deltaTime);
    }

    void CheckWrap()
    {
        if (!wrapCollider.bounds.Contains(transform.position))
        {
            Vector3 newPosition = transform.position;

            if (transform.position.x > wrapCollider.bounds.max.x + buffer)
                CloneObject(new Vector3(wrapCollider.bounds.min.x, transform.position.y, transform.position.z));
            if (transform.position.x < wrapCollider.bounds.min.x - buffer)
                CloneObject(new Vector3(wrapCollider.bounds.max.x, transform.position.y, transform.position.z));

            if (transform.position.y > wrapCollider.bounds.max.y + buffer)
                CloneObject(new Vector3(transform.position.x, wrapCollider.bounds.min.y, transform.position.z));
            if (transform.position.y < wrapCollider.bounds.min.y - buffer)
                CloneObject(new Vector3(transform.position.x, wrapCollider.bounds.max.y, transform.position.z));

            DestroyClone();
        }
    }

    void CloneObject(Vector3 clonePosition)
    {
        if (clone == null)
            clone = Instantiate(gameObject, clonePosition, Quaternion.identity);
    }

    void DestroyClone()
    {
        if (clone != null && wrapCollider.bounds.Contains(transform.position))
            Destroy(clone);
    }
}