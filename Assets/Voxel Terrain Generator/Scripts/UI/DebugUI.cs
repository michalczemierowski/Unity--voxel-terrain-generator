using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public static DebugUI Instance;

    [SerializeField] private Text debugText;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    public void SetDebugText(string text)
    {
        debugText.text = text;
    }
}
