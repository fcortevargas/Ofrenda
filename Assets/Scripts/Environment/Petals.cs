using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Petals : MonoBehaviour
{
    // References to the grid and the specific tilemap on which operations will be performed
    private GameObject _grid;
    private Tilemap _surface;

    // Variables to hold the player's position and the target position on the tilemap
    private Vector3 _playerPosition;
    private Vector3Int _targetGridPosition;
    private HashSet<Vector3Int> _modifiedTiles;
    public static HashSet<Vector3> ModifiedTiles { get; private set; }

    // Offset to adjust the player's Y position when converting to tilemap coordinates
    private const float PositionYOffset = 0.5f;

    private bool _dropPetals;
    private bool _pickUpPetals;
    private bool _pickUpAllPetals;
    private float _keyHoldTimer;
    public float requiredHoldTime = 2.0f;

    // Color to be applied to the tile
    private readonly Color _orange = new(1f, 0.65f, 0f, 1f);

    private void Awake()
    {
        // Finding the grid GameObject and getting the Tilemap component from its child named "Surface"
        _grid = GameObject.Find("Grid");
        _surface = _grid.transform.Find("Surfaces").GetComponent<Tilemap>();
        _modifiedTiles = new HashSet<Vector3Int>();
        ModifiedTiles = new HashSet<Vector3>();
    }

    private void GetPetalControlInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _dropPetals = !_dropPetals;
        }

        if (Input.GetKey(KeyCode.F) && _modifiedTiles.Count > 0)
        {
            _dropPetals = false;
            
            _pickUpPetals = true;
            
            // Increment the timer
            _keyHoldTimer += Time.deltaTime;

            // Check if the timer has reached the required hold time
            if (_keyHoldTimer >= requiredHoldTime)
            {
                _pickUpAllPetals = true;
            }
        }
        else
        {
            // Reset the timer if the key is released
            _keyHoldTimer = 0f;
        }
    }
    
    private void GetTargetTilePosition()
    {
        // Get the current position of the player
        _playerPosition = transform.position;
        
        // Apply the Y offset to the player's position
        _playerPosition.y += PositionYOffset;
        
        // Convert the player's position to the corresponding cell position in the tilemap
        _targetGridPosition = _surface.WorldToCell(_playerPosition);
        
        // Adjusting Y coordinate to target the tile below the player
        _targetGridPosition.y -= 1;
    }
    
    private void ChangeTileColor()
    {
        // Check if there's a tile at the given position and player has pressed the key for dropping the petals
        if (_dropPetals && _surface.HasTile(_targetGridPosition))
        {
            // Remove any flags (like lock color) on the tile to allow color change
            _surface.SetTileFlags(_targetGridPosition, TileFlags.None);
            // Change the color of the tile
            _surface.SetColor(_targetGridPosition, _orange);
            // Add modified tile to the hash set in the cell frame
            _modifiedTiles.Add(_targetGridPosition);
            // Add modified tile to the hash set in the world frame
            ModifiedTiles.Add(_surface.CellToWorld(_targetGridPosition));
        }
    }
    
    private void ResetTileColor()
    {
        if (_pickUpPetals && _surface.HasTile(_targetGridPosition))
        {
            _surface.SetColor(_targetGridPosition, Color.white); // Resetting to default color
            _modifiedTiles.Remove(_targetGridPosition);
            ModifiedTiles.Remove(_surface.CellToWorld(_targetGridPosition));
            _pickUpPetals = false;
        }
    }
    
    private void ResetAllTileColors()
    {
        if (_pickUpAllPetals)
        {
            foreach (var position in _modifiedTiles)
            {
                _surface.SetColor(position, Color.white); // Resetting to default color, change as needed
            }
            _modifiedTiles.Clear(); // Clear the list after resetting
            ModifiedTiles.Clear();
            
            _pickUpAllPetals = false;
        }
    }

    private void Update()
    {
        // Check conditions - if the player is not on the ground or does not have the correct tag, return
        if (!PlayerControl.IsOnGround || !gameObject.CompareTag("Cempasuchil")) 
            return;

        GetPetalControlInput();

        GetTargetTilePosition();

        ChangeTileColor();

        ResetTileColor();

        ResetAllTileColors();
    }
}
