using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public Tilemap tilemap; // Tilemap que contiene los tiles
    public TileBase tile; // Tile que se va a mover
    public Vector3Int startPosition; // Posición inicial del tile
    private Vector3Int currentPosition;
    public float speed = 1f; // Velocidad de movimiento
    private GameObject player;

    void Start()
    {
        currentPosition = startPosition;
        tilemap.SetTile(currentPosition, tile);
    }

    void Update()
    {
        Vector3Int newPosition = currentPosition + new Vector3Int(1, 0, 0); // Mover a la derecha

        tilemap.SetTile(currentPosition, null); // Borra el tile antiguo
        tilemap.SetTile(newPosition, tile); // Lo coloca en la nueva posición

        currentPosition = newPosition;

        // Si el jugador está sobre la plataforma, lo mueve con ella
        if (player != null)
        {
            player.transform.position += Vector3.right * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }
}