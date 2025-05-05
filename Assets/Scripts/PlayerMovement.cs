using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask layerMask;
    private float checkRadius = 0.05f;

    public Tilemap destruibleTiles;

    private bool isImpulsed = false, hasHammer = true, destroyedTile = true;
    private float horizontalValue;
    [SerializeField] private float speed = 1f, jumpForce = 1f, jumpMovementPenalization = 1f;
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
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, -raycastOrigin.up, 0.3f, layerMask);
        Debug.DrawLine(raycastOrigin.position, new Vector2(raycastOrigin.position.x, raycastOrigin.position.y - 0.3f), Color.green);

        if (hit && rb.velocity.y <= 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isImpulsed = false;
            animator.SetBool("isJumping", false);
            playerState = PLAYER_STATES.IDLE;
            destroyedTile = false;
            Idle();
        }
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
            Debug.Log("Tile pos: " + tilePos + " Has tile: " + destruibleTiles.HasTile(tilePos));

            if (destruibleTiles.HasTile(tilePos) && !destroyedTile)
            {
                destroyedTile = true;
                destruibleTiles.SetTile(tilePos, null);
            }
        }
    }
}
