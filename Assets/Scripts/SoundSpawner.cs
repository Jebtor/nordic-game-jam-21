using UnityEngine;

public class SoundSpawner : MonoBehaviour
{
    [SerializeField] float m_Interval = 1f;

    float m_CountDown;

    SoundSources m_SoundManager;

    void Start()
    {
        m_SoundManager = FindObjectOfType<SoundSources>();
        m_CountDown = m_Interval;
    }

    void Update()
    {
        m_CountDown -= Time.deltaTime;

        if(m_CountDown <= 0f)
        {
            m_SoundManager.SpawnSounceAt(transform.position);
            m_CountDown = m_Interval;
        }
    }
}
