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
    private TileBase[] possibleTiles = new TileBase[3];
    public TileBase fillTile;

    public LayerMask groundLayer; // Capa del suelo
    private bool getTile = true;

    void Start()
    {
        if(gameObject.transform.position.y >= 3 && gameObject.transform.position.y < 12)
        {
            fillTile = possibleTiles[1];
        }
        else if(gameObject.transform.position.y >= 12)
        {
            fillTile = possibleTiles[2];
        }
        else
        {
            fillTile = possibleTiles[0];
        }
    }

    void Update()
    {
        Debug.DrawLine(reycastOrigin.position, new Vector2(reycastOrigin.position.x, reycastOrigin.position.y - 0.3f), Color.red);

        // Lanzar el Raycast hacia abajo desde el prefab
        RaycastHit2D hit = Physics2D.Raycast(reycastOrigin.position, Vector2.down, 2f, groundLayer);

        if (tilemap == null && getTile)
        {
            //tilemap = GameObject.FindGameObjectWithTag("Destruible_block").GetComponent<Tilemap>();
            tilemap = hit.collider.gameObject.GetComponent<Tilemap>();
            getTile = false;
        }

        // Si el Raycast NO encuentra suelo, significa que hay un agujero
        if (hit.collider == null)
        {
            Debug.Log("Hay agujero");
            // Obtener la posici�n en el Tilemap donde cay� el hielo
            Vector3Int tilePosition = tilemap.WorldToCell(reycastOrigin.position);

            // Si el espacio est� vac�o, colocar el Tile y destruir el prefab
            if (tilemap.GetTile(tilePosition) == null)
            {
                Debug.Log("Crear tile");
                tilemap.SetTile(tilePosition, fillTile);

                Destroy(gameObject); // Desaparecer el hielo
            }
        }

        /*if(getTile && hit.collider.CompareTag("Destruible_block"))
        {
            Vector3Int tilePosition = tilemap.WorldToCell(reycastOrigin.position);
            fillTile = tilemap.GetTile(tilePosition);
            getTile = false;
        }*/

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
