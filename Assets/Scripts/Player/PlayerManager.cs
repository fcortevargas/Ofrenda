using Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] characters;
    public int characterIndex;

    public GameObject player;
    public Vector3 playerPosition;

    public GameObject soul;
    private Soul _soulController;

    public GameObject virtualCamera;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    
    private void Awake()
    {
        _soulController = soul.GetComponent<Soul>();
        _cinemachineVirtualCamera = virtualCamera.GetComponent<CinemachineVirtualCamera>();

        player = Instantiate(characters[characterIndex], Vector3.back, Quaternion.identity);

        _soulController.player = player;
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
        _soulController.player = player;
        _cinemachineVirtualCamera.Follow = player.transform;
    }
}
