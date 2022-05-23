using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;
    public Image FillBar;
    public bool IsShowingText;

    [Range(0f, 1f)]
    private float barPercent;

    public void SetBarPercent(float percent)
    {
        barPercent = percent;
    }

    private void OnGUI()
    {
        FillBar.fillAmount = barPercent;

        if (IsShowingText)
        {
            TextMesh.text = $"{barPercent * 100f:0}%";
        }
    }
}
