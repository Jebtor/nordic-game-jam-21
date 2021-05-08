using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    SoundSources m_SoundManager;

    void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();
    }

    void Update()
    {
        if(NetworkObject.IsOwner)
            HandleInput();
    }

    void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            var camera = Camera.main;
            var plane = new Plane(Vector3.up, 0f);
            var ray = camera.ScreenPointToRay(new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue()));

            if (Physics.Raycast(ray, out var hit))
            {
                SpawnNewSoundAt_ServerRPC(hit.point);
            }
        }
    }

    [ServerRpc]
    public void SpawnNewSoundAt_ServerRPC(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }
}
