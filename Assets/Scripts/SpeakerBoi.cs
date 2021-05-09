using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class SpeakerBoi : NetworkBehaviour
{
    public float WaveSpawnInterval = 0.5f;

    public float MaxVolume = 1;

    public NetworkVariableBool IsMuted = new NetworkVariableBool(s_Settings, false);

    public AudioSource audioSourceMusic;

    SoundSources m_SoundManager;
    float m_CurrentSpawnInterval;

    static readonly NetworkVariableSettings s_Settings = new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 120f
    };

    private void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();

        audioSourceMusic = GetComponent<AudioSource>();

        IsMuted.OnValueChanged += OnToggleChange;
    }

    private void Update()
    {
        if (IsMuted.Value == true)
        {
            return;
        }

        if (!NetworkObject.IsOwner)
        {
            return;
        }

        m_CurrentSpawnInterval -= Time.deltaTime;

        if (m_CurrentSpawnInterval < 0)
        {
            SpawnNewSoundAt(transform.position);
            m_CurrentSpawnInterval = WaveSpawnInterval;
        }
    }

    public void SpawnNewSoundAt(Vector3 point)
    {
        m_SoundManager.SpawnSounceAt(point);
    }

    public void OnToggleChange(bool prevValue, bool newValue)
    {
        audioSourceMusic.volume = newValue ? 0 : MaxVolume;
        GetComponent<Animator>().SetBool("Sound", !newValue);
    }
}
