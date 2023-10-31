using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerController playerController;
    Animator playerAnimator;

    // Control variables
    [SerializeField] private float maxSpeed = 40f;
    bool jump = false;
    const bool crouch = false;

    // Animator parameters
    float horizontalInput = 0f;
    bool isMaxHorizontalInput = false;
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontalInput) == 1)
        {
            isMaxHorizontalInput = true;
        }
        else
        {
            isMaxHorizontalInput = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            playerController.SwitchCharacter("fire");
        }
    }

    void FixedUpdate()
    {
        playerController.Move(horizontalInput * maxSpeed * Time.fixedDeltaTime, crouch, jump);
        jump = false;

        HandleAnimations();
    }

    void HandleAnimations()
    {
        playerAnimator.SetFloat("horizontalInput", Mathf.Abs(horizontalInput));
        playerAnimator.SetBool("isMaxHorizontalInput", isMaxHorizontalInput);
    }
}
 