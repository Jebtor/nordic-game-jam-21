using Cinemachine;
using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KinematicController : MonoBehaviour
{
    public float speed = 12.0f;
    public float jumpSpeed = 15.0f;
    public float gravity = 35.0f;
    public float snapForce = 10.0f;

    public GameObject animatedAvatar;
    Animator animator;

    KinematicBody m_KinematicBody;
    PlayerInput m_PlayerInput;

    Vector2 m_Look;
    Vector2 m_Move;
    bool m_Jump;
    bool m_ExecutedJump;

    Transform m_Camera;
    
    void Start()
    {
        animator = animatedAvatar.GetComponent<Animator>();

        if (!GetComponent<NetworkObject>().IsLocalPlayer)
        { 
            Destroy(this);
            return;
        }

        m_PlayerInput = FindObjectOfType<PlayerInput>();
        m_KinematicBody = GetComponent<KinematicBody>();

        var camera = FindObjectOfType<CinemachineVirtualCamera>();
        camera.Follow = transform;

        m_Camera = Camera.main.transform;

        m_ExecutedJump = false;
    }

    void Update()
    {
        m_Look = m_PlayerInput.actions["Look"].ReadValue<Vector2>();
        m_Move = m_PlayerInput.actions["Move"].ReadValue<Vector2>();

        if (m_Move.sqrMagnitude > .1f)
            animator.SetBool("Walking", true);
        else
            animator.SetBool("Walking", false);

        var jump = m_PlayerInput.actions["Jump"].triggered;
        
        if (jump && !m_ExecutedJump)
        {
            m_Jump = true;
            animator.SetTrigger("Jump");
            m_ExecutedJump = true;
        }
    }

    void FixedUpdate()
    {
        var rotationDelta = m_Look;
        var rotation = Quaternion.Euler(0f, m_Camera.rotation.eulerAngles.y, 0f);// m_KinematicBody.rigidbody.rotation * Quaternion.Euler(0f, .5f * rotationDelta.x, 0f);
        m_KinematicBody.Rotate(rotation);

        //  Always deriv your motion from the kinematic body velocity
        //  otherwise your movement will be incorrect, like when you
        //  jump against a ceiling and don't fall since your motion 
        //  still being applied against its direction.
        Vector3 moveDirection = m_KinematicBody.velocity;

        if (m_KinematicBody.isGrounded)
        {
            var direction = m_Move;// m_PlayerInput.actions["Move"].ReadValue<Vector2>();
            moveDirection = new Vector3(direction.x, 0, direction.y);
            moveDirection = rotation * moveDirection.normalized;
            moveDirection *= speed;

            //  If te kinematic body is grounded you should always apply velocity downward
            //  to keep the isGrounded status as true and avoid the "bunny hop" effect when
            //  walking down steep ground surfaces.
            moveDirection.y = -snapForce;

            if (m_Jump)
            {
                moveDirection.y = jumpSpeed;
                m_ExecutedJump = false;
                m_Jump = false;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        // Call the Move method from Kinematic Body with the desired motion.
        m_KinematicBody.Move(moveDirection);
    }
}
