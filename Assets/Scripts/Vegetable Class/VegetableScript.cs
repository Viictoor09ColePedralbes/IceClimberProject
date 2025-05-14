using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VegetableScript : MonoBehaviour
{
    [SerializeField] private Vegetable vegetable;
    private SpriteRenderer spriteRenderer;
    private TMP_Text pointsText;
    private BoxCollider2D boxCollider;
    [SerializeField] private AudioClip grabVegetableClip;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pointsText = GetComponentInChildren<TMP_Text>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        spriteRenderer.sprite = vegetable.vegetable;
        pointsText.text = vegetable.points.ToString(vegetable.points >= 1000 ? "0000" : "000");
        pointsText.color = new Vector4(1, 1, 1, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.instance.PlaySFX(grabVegetableClip);
            pointsText.color = new Vector4(1, 1, 1, 1);
            boxCollider.enabled = false; // Para evitar una segunda colision
            StartCoroutine(animPoints());
        }
    }

    private IEnumerator animPoints()
    {
        float duration = 0.35f, time = 0;
        Vector3 startPoint = pointsText.transform.position;
        while (duration > time)
        {
            pointsText.transform.position = Vector3.Lerp(startPoint, startPoint + new Vector3(0, 0.85f, 0), time / duration);
            time += Time.deltaTime;
            yield return null; // Esperar siguiente frame
        }
        yield return new WaitForSecondsRealtime(duration);
        GameManager.instance.thingsPoints[0] += 1;
        Destroy(gameObject);
    }

    public Vegetable GetVegetable()
    {
        return vegetable;
    }
}
