using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISpecialCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    private SpecialType specialType;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowSpecial);
    }

    public void SetSpecial(SpecialType type)
    {
        specialType = type;
        nameText.text = ResourceManager.Instance.GetSpecialData(specialType).name;
    }

    private void ShowSpecial()
    {
        UIManager.Instance.ShowSpecialCard(specialType);
    }
}
