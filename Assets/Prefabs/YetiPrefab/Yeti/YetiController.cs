using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class YetiController : MonoBehaviour
{
    [SerializeField]
    private float yeti_Life = 1;

    [SerializeField]
    float speed;

    [SerializeField]
    Transform reycastOrigin;

    [SerializeField]
    Transform iceCubeSpawn;

    [SerializeField]
    GameObject bloqueDeHieloPrefab;

    private Vector2 dir = Vector2.right;
    private SpriteRenderer spriteRenderer;
    private GameObject bloqueDeHielo;
    private Boolean noHaySuelo = false;
    private bool bloqueCreado = false;
    [SerializeField] private LayerMask raycastMask;
    private Animator animator;
    private bool goingToDead = false;
    [SerializeField] private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private float time;
    private const float MAX_TIME = 0.15f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if(gameObject.transform.position.x < 0)
        {
            spriteRenderer.flipX = true;
            CambiarDireccion();
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    void Update()
    {

        Debug.DrawLine(reycastOrigin.position, new Vector2(reycastOrigin.position.x, reycastOrigin.position.y - 0.3f), Color.green);

        // Detectar suelo con Raycast
        RaycastHit2D hit = Physics2D.Raycast(reycastOrigin.position, Vector2.down, 2f, raycastMask);

        if (hit.collider == null)
        {
            // No hay suelo, activar modo regreso
            if (!goingToDead)
            {
                CambiarDireccion();
            }
            noHaySuelo = true;
            Debug.Log("NoTieneSuelo: " + noHaySuelo);

        }

        // Mover al personaje en la dirección actual si no está regresando
        transform.Translate(dir * speed * Time.deltaTime);

        if (yeti_Life == 0)
        {
            CambiarDireccion();
            Destroy(gameObject, 15);
            
        }

        if(bloqueCreado && bloqueDeHielo == null)
        {
            bloqueCreado = false;
            CambiarDireccion();
        }

        if (GameManager.instance.isOnBonusStage)
        {
            Destroy(gameObject);
        }

        if(goingToDead)
        {
            if(time < MAX_TIME)
            {
                time += Time.deltaTime;
            }
            else if(time >= MAX_TIME)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
                time = 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {   
        if (collision.collider.CompareTag("yetiWall"))
        {
            CambiarDireccion();
            Debug.Log("Hay colision YetiWall");
            if (noHaySuelo)
            {
                Debug.Log("Se crea hielo");
                CrearBloqueDeHielo();
            }
        }
        
    }

    void CambiarDireccion()
    {
        Debug.Log("Esta cambiando direccion");
        dir = -dir;
        reycastOrigin.localPosition = new Vector3(-reycastOrigin.localPosition.x, reycastOrigin.localPosition.y, 0);
        iceCubeSpawn.localPosition = new Vector3(-iceCubeSpawn.localPosition.x, iceCubeSpawn.localPosition.y, 0);
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    void CrearBloqueDeHielo()
    {
        bloqueDeHielo = Instantiate(bloqueDeHieloPrefab, iceCubeSpawn.position, Quaternion.identity);
        bloqueDeHielo.GetComponent<Rigidbody2D>().velocity = dir * speed;
        noHaySuelo = false;
        bloqueCreado = true;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Hammer") && !goingToDead)
        {
            boxCollider.enabled = false;
            animator.SetBool("isDead", true);
            rb.constraints = RigidbodyConstraints2D.FreezePositionY;
            CambiarDireccion();
            if(bloqueDeHielo)
            {
                Destroy(bloqueDeHielo);
                GameManager.instance.thingsPoints[1] += 1;
            }
            GameManager.instance.enemiesDefeated += 1;
            goingToDead = true;
            Destroy(gameObject, 7f);
            speed += 2;
        }
    }

}
