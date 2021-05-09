using UnityEngine;
using UnityEngine.UI;

public class UIWiring : MonoBehaviour
{
    [SerializeField] private Text m_HealthText;
    [SerializeField] private Text m_DeadText;

    public void SetHealth(int newValue)
    {
        m_HealthText.text = newValue.ToString();
        m_DeadText.enabled = newValue == 0;
    }
}
