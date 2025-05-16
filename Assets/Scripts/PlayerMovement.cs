using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private InputAction horizontal_ia, jump_ia, attack_ia, pause_ia;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D hammerCollider;
    [SerializeField] private CapsuleCollider2D jumpCollider;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask layerMask;
    private float checkRadius = 0.05f;
    private int lifes;

    public Tilemap destruibleTiles;

    private bool isImpulsed = false, hasHammer = true, destroyedTile = false, isInmune = false;
    private float horizontalValue;
    [SerializeField] private float speed = 1f, jumpForce = 1f, jumpMovementPenalization = 0.6f;

    [SerializeField] private GameObject fallPointPrefab;
    private GameObject fallPoint;
    private float timeFallPoint, maxTimeFallPoint = 2f, elapsedInmTime = 0, alphaInmune = 1, attackingTime = 0;
    private const float MAX_INM_TIME = 2, MAX_ATTACKING_TIME = 3;
    [SerializeField] private Image[] lifesImages;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private AudioClip jumpClip, destroyBlockClip, aerodactyl_clip, loseLifeClip, gameOverClip;

    // Variables para pause
    [SerializeField] private CanvasGroup pauseMenu;
    private bool inPause = false;

    void Awake()
    {
        lifes = 3;
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
        pause_ia = inputMap.FindActionMap("Other").FindAction("Pause");
    }

    void Update()
    {
        if (!destroyedTile)
        {
            destruibleTiles = GameObject.FindGameObjectWithTag("Destruible_block").GetComponent<Tilemap>();
        }
            
        if (playerState != PLAYER_STATES.ATTACKING)
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

        if (pause_ia.triggered)
        {
            inPause = !inPause;
            OnPause(inPause);
        }

        if(isInmune)
        {
            if(elapsedInmTime < MAX_INM_TIME)
            {
                alphaInmune = Mathf.PingPong(Time.time * 12, 1);
                spriteRenderer.color = new Vector4(1, 1, 1, alphaInmune);
                elapsedInmTime += Time.deltaTime;
            }
            else if(elapsedInmTime >= MAX_INM_TIME)
            {
                spriteRenderer.color = new Vector4(1, 1, 1, 1);
                isInmune = false;
                elapsedInmTime = 0;
            }
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
            AudioManager.instance.PlaySFX(jumpClip);
            playerState = PLAYER_STATES.JUMPING;
            Jumping();
        }
        else if(attack_ia.triggered && animator.GetBool("hasHammer"))
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
            AudioManager.instance.PlaySFX(jumpClip);
            playerState = PLAYER_STATES.JUMPING;
            Jumping();
        }
        else if(attack_ia.triggered && animator.GetBool("hasHammer"))
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
        if (!hammerCollider.enabled && animator.GetBool("isAttacking") == false)
        {
            hammerCollider.enabled = true;
            animator.SetBool("isJumping", false);
            animator.SetBool("isAttacking", true);
        }

        if(attackingTime < MAX_ATTACKING_TIME)
        {
            attackingTime += Time.deltaTime;
        }
        else if(attackingTime >= MAX_ATTACKING_TIME)
        {
            attackingTime = 0;
            FinishedAttack();
        }
    }

    private void DetectFloor()
    {
        if (IsTouchingFloor() && rb.velocity.y <= 0)
        {
            Debug.Log("Is touching floor");
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
        animator.SetBool("isJumping", false);
        animator.SetBool("isWalking", false);
        playerState = PLAYER_STATES.IDLE;
        Idle();
    }

    private void DestroyBlock()
    {
        Vector2 checkPos = (Vector2)transform.position + Vector2.up * 0.7f;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, checkRadius, layerMask);

        if (hit != null)
        {
            Vector3Int tilePos = destruibleTiles.WorldToCell(checkPos);

            if (destruibleTiles.HasTile(tilePos) && !destroyedTile && hit.CompareTag("Destruible_block"))
            {
                destroyedTile = true;
                destruibleTiles.SetTile(tilePos, null);
                AudioManager.instance.PlaySFX(destroyBlockClip);
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
            AudioManager.instance.PlaySFX(aerodactyl_clip);
        }
        else if (collision.CompareTag("vacio"))
        {
            if (GameManager.instance.isOnBonusStage)
            {
                GameManager.instance.timeBonusStage = 0;
                GameManager.instance.timeToShowPoints = 5;
            }
            else
            {
                LoseLife(true);
                gameObject.transform.position = fallPoint.transform.position;
                rb.velocity = Vector2.zero;
            }
            
        }
        else if (collision.CompareTag("enemy") && playerState != PLAYER_STATES.JUMPING && playerState != PLAYER_STATES.ATTACKING && !isInmune)
        {
            LoseLife(true);
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

    private void LoseLife(bool doInmuneAndSound)
    {
        if (doInmuneAndSound)
        {
            isInmune = true;
            AudioManager.instance.PlaySFX(loseLifeClip);
        }
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
        AudioManager.instance.PlaySFX(gameOverClip);

        while (time < maxTime)
        {
            gameOverTransf.position = Vector3.Lerp(initialPos, finalPos, time / maxTime);
            time += Time.deltaTime;
            yield return null; // Esperar siguiente frame
        }

        yield return new WaitForSecondsRealtime(2.5f);
        GameManager.instance.PlayerHasDead();
    }

    public void OneLifeGamemode()
    {
        LoseLife(false);
        LoseLife(false);
    }

    public void OnPause(bool inPauseT)
    {
        inPause = inPauseT;
        Debug.Log("Entra en OnPause");
        FreezingControl(inPauseT);
        pauseMenu.blocksRaycasts = inPauseT;
        pauseMenu.alpha = inPauseT ? 1 : 0;
        pauseMenu.interactable = inPauseT;
        Time.timeScale = inPauseT ? 0 : 1;
    }

    public void ClickBotonGoToMenu()
    {
        pauseMenu.blocksRaycasts = false;
        pauseMenu.alpha = 0;
        pauseMenu.interactable = false;
        GameManager.instance.GoToMenuButton();
    }
}
