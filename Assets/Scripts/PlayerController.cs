using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -20f;
    [SerializeField] LayerMask groundLayer;

    CharacterController controller;
    Vector2 moveInput;
    Vector3 velocity; //垂直速度
    bool isGrounded;
    float groundCheckDistance = 0.2f;

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
        if (CanJump())
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
        isGrounded = CanJump();

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
        animator.SetBool("IsFalling", !isGrounded && velocity.y < 0f);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("IsJumping", false);
        }
        Debug.Log($"isGrounded: {isGrounded}, velocity.y: {velocity.y}, IsFalling: {animator.GetBool("IsFalling")}");
    }

    bool CanJump()
    {
        //用Controller底部的半球做SphereCast
        Vector3 origin = transform.position
                       + controller.center
                       + Vector3.down * (controller.height * 0.5f - controller.radius);
        return Physics.SphereCast(origin,
            controller.radius,
            Vector3.down,
            out RaycastHit hit, 
            groundCheckDistance,
            groundLayer);
    }
}
