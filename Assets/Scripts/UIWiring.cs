using UnityEngine;
using UnityEngine.UI;

public class UIWiring : MonoBehaviour
{
    [SerializeField] private Text m_HealthText;

    public void SetHealth(int newValue)
    {
        m_HealthText.text = newValue.ToString();
    }
}
