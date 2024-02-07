using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] characters;
    public int characterIndex;

    public GameObject player;
    public Vector3 playerPosition;

    public GameObject virtualCamera;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    
    private void Awake()
    {
        _cinemachineVirtualCamera = virtualCamera.GetComponent<CinemachineVirtualCamera>();

        player = Instantiate(characters[characterIndex], Vector3.back, Quaternion.identity);

        AssignPlayerToSoul();
        
        _cinemachineVirtualCamera.Follow = player.transform;

        playerPosition = player.transform.position;
    }

    private void Update()
    {
        // Update player position;
        playerPosition = player.transform.position;

        if (Input.GetMouseButtonDown(0))
        {
            SwitchCharacter();
        }
    }

    private void SwitchCharacter()
    {
        characterIndex++;

        if (characterIndex == 3)
        {
            characterIndex = 0;
        }

        Destroy(player);

        player = Instantiate(characters[characterIndex], playerPosition, Quaternion.identity);

        AssignPlayerToSoul();
        
        _cinemachineVirtualCamera.Follow = player.transform;
    }

    private void AssignPlayerToSoul()
    {
        var isXolo = player.CompareTag("Xolo");
        
        if (isXolo)
        {
            Soul.Soul.Player = player;
        }
    }
}
