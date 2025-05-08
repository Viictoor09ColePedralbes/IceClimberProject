using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private enum PLAYER_STATES
    {
        IDLE, WALKING, JUMPING, ATTACKING
    }
    private PLAYER_STATES playerState;

    [HideInInspector] public Animator animator;
    private Rigidbody2D rb;
    [SerializeField] private InputActionAsset inputMap;
    private InputAction horizontal_ia, jump_ia, attack_ia;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D hammerCollider;
    [SerializeField] private CapsuleCollider2D jumpCollider;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask layerMask;
    private float checkRadius = 0.05f;
    private int lifes;

    public Tilemap destruibleTiles;

    private bool isImpulsed = false, hasHammer = true, destroyedTile = false;
    private float horizontalValue;
    [SerializeField] private float speed = 1f, jumpForce = 1f, jumpMovementPenalization = 1f;

    [SerializeField] private GameObject fallPointPrefab;
    private GameObject fallPoint;
    private float timeFallPoint, maxTimeFallPoint = 2f;
    [SerializeField] private Image[] lifesImages;
    [SerializeField] private Image gameOverImage;
    void Awake()
    {
        playerState = PLAYER_STATES.IDLE;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputMap.Enable();
    }

    void Start()
    {
        horizontal_ia = inputMap.FindActionMap("Movement").FindAction("Horizontal");
        jump_ia = inputMap.FindActionMap("Movement").FindAction("Jump");
        attack_ia = inputMap.FindActionMap("Movement").FindAction("Attack");
        lifes = 3;
    }

    void Update()
    {
        if(playerState != PLAYER_STATES.ATTACKING)
        {
            horizontalValue = horizontal_ia.ReadValue<float>();
        }
        
        switch (playerState)
        {
            case PLAYER_STATES.IDLE:
                Idle();
                break;
            case PLAYER_STATES.WALKING:
                Walking();
                break;
            case PLAYER_STATES.JUMPING:
                Jumping();
                DetectFloor();
                DestroyBlock();
                break;
            case PLAYER_STATES.ATTACKING:
                Attacking();
                break;
        }

        if(horizontalValue != 0)
        {
            spriteRenderer.flipX = horizontalValue >= 1 ? true : false;
            hammerCollider.offset = horizontalValue >= 1 ? new Vector2(0.6f, -0.1451442f) : new Vector2(-0.6f, -0.1451442f);
        }
        
        if(playerState != PLAYER_STATES.JUMPING)
        {
            FallControl();
        }
    }

    private void Idle()
    {
        if (animator.GetBool("isWalking"))
        {
            animator.SetBool("isWalking", false);
        }

        if(horizontalValue != 0)
        {
            playerState = PLAYER_STATES.WALKING;
            Walking();
        }

        if(jump_ia.triggered)
        {
            playerState = PLAYER_STATES.JUMPING;
            Jumping();
        }

        if(attack_ia.triggered)
        {
            playerState = PLAYER_STATES.ATTACKING;
            Attacking();
        }
    }

    private void Walking()
    {
        rb.velocity = new Vector2((speed * horizontalValue), rb.velocity.y);

        if (!animator.GetBool("isWalking"))
        {
            animator.SetBool("isWalking", true);
        }

        if (horizontalValue == 0)
        {
            playerState = PLAYER_STATES.IDLE;
            Idle();
        }

        if (jump_ia.triggered)
        {
            playerState = PLAYER_STATES.JUMPING;
            Jumping();
        }

        if (attack_ia.triggered)
        {
            rb.velocity = Vector2.zero;
            playerState = PLAYER_STATES.ATTACKING;
            Attacking();
        }
    }

    private void Jumping()
    {
        if(!isImpulsed)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            GameManager.instance.pressedJump += 1;
            jumpCollider.enabled = true;
            isImpulsed = true;
            animator.SetBool("isJumping", true);
        }
        rb.velocity = new Vector2(((speed - jumpMovementPenalization) * horizontalValue), rb.velocity.y);
    }

    private void Attacking()
    {
        if(!hammerCollider.enabled && animator.GetBool("hasHammer") == true)
        {
            hammerCollider.enabled = true;
            animator.SetBool("isAttacking", true);
        }
        else if(animator.GetBool("hasHammer") == false)
        {
             FinishedAttack();
        }
    }

    private void DetectFloor()
    {
        if (IsTouchingFloor() && rb.velocity.y <= 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isImpulsed = false;
            jumpCollider.enabled = false;
            animator.SetBool("isJumping", false);
            playerState = PLAYER_STATES.IDLE;
            destroyedTile = false;
            timeFallPoint = maxTimeFallPoint;
            Idle();
        }
    }

    private bool IsTouchingFloor()
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, -raycastOrigin.up, 0.3f, layerMask);
        Debug.DrawLine(raycastOrigin.position, new Vector2(raycastOrigin.position.x, raycastOrigin.position.y - 0.3f), Color.green);

        if (hit.collider == null)
            return false;

        return hit.collider.CompareTag("Not_destruible_block") || hit.collider.CompareTag("Destruible_block");
    }

    public void FinishedAttack()
    {
        hammerCollider.enabled = false;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isWalking", false);
        playerState = PLAYER_STATES.IDLE;
        Idle();
    }

    private void DestroyBlock()
    {
        Vector2 checkPos = (Vector2)transform.position + Vector2.up * 0.6f;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, checkRadius, layerMask);

        if (hit != null)
        {
            Vector3Int tilePos = destruibleTiles.WorldToCell(checkPos);
            Debug.Log("Tile pos: " + tilePos + " Has tile: " + destruibleTiles.HasTile(tilePos) + "Destroyed Tile: " + destroyedTile);

            if (destruibleTiles.HasTile(tilePos) && !destroyedTile && hit.CompareTag("Destruible_block"))
            {
                destroyedTile = true;
                destruibleTiles.SetTile(tilePos, null);
                GameManager.instance.thingsPoints[3] += 1;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("aerodactyl"))
        {
            GameManager.instance.gainedPoints += 6000;
            GameManager.instance.stageFinished = true;
            Debug.Log("Aerodactyl tocado");
        }
        else if (collision.CompareTag("vacio"))
        {
            LoseLife();
            gameObject.transform.position = fallPoint.transform.position;
            rb.velocity = Vector2.zero;
        }
        else if (collision.CompareTag("enemy") && playerState != PLAYER_STATES.JUMPING && playerState != PLAYER_STATES.ATTACKING)
        {
            LoseLife();
        }
    }

    public void FreezingControl(bool isFreezed)
    {
        rb.gravityScale = isFreezed ? 0 : 1;
        rb.velocity = isFreezed ? Vector2.zero : rb.velocity;
        if (isFreezed)
        {
            inputMap.Disable();
        }
        else
        {
            inputMap.Enable();
        }
    }

    private void FallControl() // Esta función se encarga de crear un punto de respawn por si el jugador se cae al vacío
    {
        if (timeFallPoint < maxTimeFallPoint)
        {
            timeFallPoint += Time.deltaTime;
        }
        else if(timeFallPoint >= maxTimeFallPoint && IsTouchingFloor())
        {
            Destroy(fallPoint);
            fallPoint = Instantiate(fallPointPrefab, gameObject.transform.position, Quaternion.identity);
            timeFallPoint = 0;
        }
    }

    private void LoseLife()
    {
        lifes--;
        for (int i = 0; i < lifesImages.Length; i++)
        {
            Vector4 invisibleColor = new Vector4(1, 1, 1, 0);
            switch (lifes)
            {
                case 0:
                    FreezingControl(true);
                    lifesImages[2].color = invisibleColor;
                    gameOverImage.color = new Vector4(1,1,1,1);
                    StartCoroutine(KillPlayer());
                    break;
                case 1:
                    lifesImages[1].color = invisibleColor;
                    break;
                case 2:
                    lifesImages[0].color = invisibleColor;
                    break;
            }
        }
    }

    private IEnumerator KillPlayer()
    {
        RectTransform gameOverTransf = gameOverImage.GetComponent<RectTransform>();
        Vector3 initialPos = gameOverTransf.position;
        Vector3 finalPos = new Vector3(gameOverTransf.position.x, 887.5f, gameOverTransf.position.z);
        float time = 0, maxTime = 1.5f;

        while(time < maxTime)
        {
            gameOverTransf.position = Vector3.Lerp(initialPos, finalPos, time / maxTime);
            time += Time.deltaTime;
            yield return null; // Esperar siguiente frame
        }

        yield return new WaitForSecondsRealtime(2.5f);
        GameManager.instance.PlayerHasDead();
    }
}
