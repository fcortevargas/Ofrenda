using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Petals : MonoBehaviour
{
    // References to the grid and the specific tilemap on which operations will be performed
    private GameObject _grid;
    private Tilemap _surfaceTilemap;

    // Variables to hold the player's position and the target position on the tilemap
    private Vector3 _playerPosition;
    private Vector3Int _targetGridPosition;

    // Offset to adjust the player's Y position when converting to tilemap coordinates
    private float _positionYOffset = 0.5f;

    // Color to be applied to the tile
    Color _orange = new(1f, 0.65f, 0f, 1f);

    private void Awake()
    {
        // Finding the grid GameObject and getting the Tilemap component from its child named "Surface"
        _grid = GameObject.Find("Grid");
        _surfaceTilemap = _grid.transform.Find("Surface").GetComponent<Tilemap>();
    }

    private void Update()
    {
        // Check conditions - if the player is not on the ground or does not have the correct tag, return
        if (!PlayerControl.IsOnGround || !gameObject.CompareTag("Cempasuchil")) return;

        // Get the current position of the player
        _playerPosition = transform.position;
        // Apply the Y offset to the player's position
        _playerPosition.y += _positionYOffset;

        // Convert the player's position to the corresponding cell position in the tilemap
        _targetGridPosition = _surfaceTilemap.WorldToCell(_playerPosition);
        // Adjusting Y coordinate to target the tile below the player
        _targetGridPosition.y -= 1;

        // Check if there's a tile at the calculated position
        if (_surfaceTilemap.HasTile(_targetGridPosition))
        {
            // Remove any flags (like lock color) on the tile to allow color change
            _surfaceTilemap.SetTileFlags(_targetGridPosition, TileFlags.None);
            // Change the color of the tile
            _surfaceTilemap.SetColor(_targetGridPosition, _orange);
        }
    }
}
