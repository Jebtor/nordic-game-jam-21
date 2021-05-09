using Unity.Mathematics;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public int SpanwnPointCount => m_SpawnPoints.Length;

    [SerializeField] Transform[] m_SpawnPoints;

    public float3 GetSpawnLocation(int index)
    {
        if (index >= m_SpawnPoints.Length)
            index = 0;

        return m_SpawnPoints[index].position;
    }
}
