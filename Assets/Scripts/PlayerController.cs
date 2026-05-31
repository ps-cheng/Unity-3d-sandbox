using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -20f;

    CharacterController controller;
    Vector2 moveInput;
    Vector3 velocity; //垂直速度
    bool isGrounded;

    Animator animator;
    bool isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("IsJumping", true);
        }
    }

    void OnSprint(InputValue value)
    {
        if (value.isPressed)
            isSprinting = !isSprinting;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        //移動
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        //旋轉面向
        if(move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        //重力
        if (velocity.y < 0)
            velocity.y += gravity * 2f * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //移動
        float targetSpeed = moveInput.magnitude * (isSprinting ? 2f : 1f);
        animator.SetFloat("Speed", Mathf.MoveTowards(animator.GetFloat("Speed"), targetSpeed, Time.deltaTime * 5f));
        if(controller.isGrounded) animator.SetBool("IsJumping", false);
        animator.SetBool("IsFalling", !controller.isGrounded && velocity.y < 0f);
        Debug.Log($"{controller.isGrounded}, {velocity.y}");
    }
}
