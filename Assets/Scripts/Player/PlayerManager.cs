using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] characters;
    public int characterIndex = 0;

    public GameObject player;
    public Vector3 playerPosition;

    public GameObject soul;
    private SoulControl soulController;

    public GameObject virtualCamera;
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    
    private void Awake()
    {
        soulController = soul.GetComponent<SoulControl>();
        cinemachineVirtualCamera = virtualCamera.GetComponent<CinemachineVirtualCamera>();

        player = Instantiate(characters[characterIndex], Vector3.back, Quaternion.identity);

        soulController.player = player;
        cinemachineVirtualCamera.Follow = player.transform;

        playerPosition = player.transform.position;
    }

    private void Update()
    {
        // Update player position;
        playerPosition = player.transform.position;

        if (Input.GetMouseButtonDown(0))
        {
            SwitchCharacter();
            Debug.Log("player switched");
        }
    }

    public void SwitchCharacter()
    {
        characterIndex++;

        if (characterIndex == 3)
        {
            characterIndex = 0;
        }

        Destroy(player);

        player = Instantiate(characters[characterIndex], playerPosition, Quaternion.identity);
        soulController.player = player;
        cinemachineVirtualCamera.Follow = player.transform;
    }
}
