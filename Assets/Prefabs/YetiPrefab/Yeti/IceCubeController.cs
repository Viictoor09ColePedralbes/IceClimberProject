using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class IceCubeController : MonoBehaviour
{
    [SerializeField]
    Transform reycastOrigin;

    public Tilemap tilemap;

    [SerializeField]
    public TileBase fillTile;


    public LayerMask groundLayer; // Capa del suelo
    private bool getTile = true;

    void Start()
    {
        if (tilemap == null)
        {
            tilemap = GameObject.Find("M1_tilemap_destruible_blocks").GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        Debug.DrawLine(reycastOrigin.position, new Vector2(reycastOrigin.position.x, reycastOrigin.position.y - 0.3f), Color.red);

        // Lanzar el Raycast hacia abajo desde el prefab
        RaycastHit2D hit = Physics2D.Raycast(reycastOrigin.position, Vector2.down, 2f);

        // Si el Raycast NO encuentra suelo, significa que hay un agujero
        if (hit.collider == null)
        {
            Debug.Log("Hay agujero");
            // Obtener la posición en el Tilemap donde cayó el hielo
            Vector3Int tilePosition = tilemap.WorldToCell(reycastOrigin.position);

            // Si el espacio está vacío, colocar el Tile y destruir el prefab
            if (tilemap.GetTile(tilePosition) == null)
            {
                Debug.Log("Crear tile");
                tilemap.SetTile(tilePosition, fillTile);

                Destroy(gameObject); // Desaparecer el hielo
            }
        }

        if(getTile && hit.collider.CompareTag("Destruible_block"))
        {
            Vector3Int tilePosition = tilemap.WorldToCell(reycastOrigin.position);
            fillTile = tilemap.GetTile(tilePosition);
            getTile = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            GameManager.instance.thingsPoints[1] += 1;
            Destroy(gameObject);
        }
    }

}
