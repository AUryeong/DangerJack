using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    [SerializeField] private string text;

    public string Text
    {
        set
        {
            text = value + ".";
            textMeshPro.text = text;
            OnEnable();
        }
    }

    private float duration;
    [SerializeField] private float createDuration = 1f;
    private float idx;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (string.IsNullOrEmpty(text))
            text = textMeshPro.text;

        text += ".";
    }

    private void OnEnable()
    {
        idx = 0;
        duration = 0;
        textMeshPro.text = text;
    }

    private void Update()
    {
        duration += Time.deltaTime;
        if (duration >= createDuration)
        {
            duration -= createDuration;
            idx = (idx + 1) % 3;

            if (idx == 0)
                textMeshPro.text = text;
            else
                textMeshPro.text += ".";
        }
    }
}