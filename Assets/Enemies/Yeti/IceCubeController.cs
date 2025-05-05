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

    float reyLength = 1f;

    public LayerMask groundLayer; // Capa del suelo

    // Start is called before the first frame update
    void Start()
    {
        if (tilemap == null)
        {
            tilemap = GameObject.Find("M1_tilemap_destruible_blocks").GetComponent<Tilemap>();
        }
    }

    // Update is called once per frame
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

    }

}
