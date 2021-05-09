using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] bool m_CaptureMouse = true;

    [SerializeField] float m_StepSoundCoolDown = 1f;
    [SerializeField] float m_ShootCooldown = .1f;

    public bool IsAlive => Health.Value > 0;

    float m_StepCountDown;
    float m_ShootCountDown;

    public GameObject animatedAvatar;
    Animator animator;

    SoundSources m_SoundManager;
    KinematicBody m_KinematicBody;
    UIWiring m_UIWiring;

    bool m_IsGrounded;

    NetworkVariableInt Health = new NetworkVariableInt(s_Settings, 10);

    static readonly NetworkVariableSettings s_Settings = new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 120f
    };

    void Start()
    {
        animator = animatedAvatar.GetComponent<Animator>();

        m_SoundManager = FindObjectOfType<SoundSources>();
        m_UIWiring = FindObjectOfType<UIWiring>();
        m_KinematicBody = GetComponent<KinematicBody>();

        m_IsGrounded = m_KinematicBody.isGrounded;

        if (m_CaptureMouse)
            Cursor.lockState = CursorLockMode.Locked;

        Health.OnValueChanged += OnHealthChanged;

        m_StepCountDown = m_StepSoundCoolDown;
        m_ShootCountDown = m_ShootCooldown;

        if (NetworkObject.IsLocalPlayer)
        {
            m_UIWiring.SetHealth(Health.Value);
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
    }

    void OnHealthChanged(int prevValue, int newValue)
    {
        if (NetworkObject.IsLocalPlayer)
            m_UIWiring.SetHealth(newValue);
    }

    void Update()
    {
        if (NetworkObject.IsOwner)
            HandleInput();

        m_StepCountDown -= Time.deltaTime;
        if(m_StepCountDown <= 0f)
        {
            var ray = new Ray(transform.position, Vector3.down);
            if(m_KinematicBody.isGrounded && Physics.Raycast(ray, out var hit, 1.3f))
            {
                SpawnNewSoundAt_ServerRPC(hit.point);
            }

            m_StepCountDown = m_StepSoundCoolDown;
        }

        if(!m_IsGrounded && m_KinematicBody.isGrounded)
        {
            Land();
        }

        m_IsGrounded = m_KinematicBody.isGrounded;
    }

    void Land()
    {
        var ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out var hit, 1.3f))
            SpawnNewSoundAt_ServerRPC(hit.point);
        animator.SetTrigger("Land");
    }

    void HandleInput()
    {
        m_ShootCountDown -= Time.deltaTime;

        var mouse = Mouse.current;
        if (mouse.leftButton.isPressed && m_ShootCountDown <= 0f)
        {
            var camera = Camera.main;
            var ray = new Ray(camera.transform.position, camera.transform.forward);//camera.ScreenPointToRay(new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue()));

            if (Physics.Raycast(ray, out var hit))
            {
                SpawnNewSoundAt_ServerRPC(hit.point);
                Shoot_ServerRPC(ray.origin, ray.direction);
                m_ShootCountDown = m_ShootCooldown;
            }
        }
    }

    [ServerRpc]
    public void Shoot_ServerRPC(Vector3 origin, Vector3 direction)
    {
        Shoot_ClientRPC();

        var ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var player = hit.transform.GetComponent<Player>();

                if (player == this)
                    return;

                Debug.Log($"Hit player {player.name}");
                player.GetHit_ClientRPC();
                player.Health.Value--;

                if (player.Health.Value <= 0)
                    player.Die_clientRPC();
            }
        }
    }

    [ClientRpc]
    public void Shoot_ClientRPC()
    {

    }

    [ServerRpc]
    public void SpawnNewSoundAt_ServerRPC(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }

    [ClientRpc]
    public void GetHit_ClientRPC()
    {
        if(NetworkObject.IsLocalPlayer)
            Debug.Log("I got hit");
    }

    [ClientRpc]
    public void Die_clientRPC()
    {
        if (NetworkObject.IsLocalPlayer)
            Debug.Log("I died");
    }
}
