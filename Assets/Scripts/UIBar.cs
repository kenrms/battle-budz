using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;
    public Image FillBar;

    [Range(0f, 1f)]
    private float barPercent;

    public void SetBarPercent(float percent)
    {
        barPercent = percent;
    }

    private void OnGUI()
    {
        FillBar.fillAmount = barPercent;
        TextMesh.text = $"{barPercent * 100f}%";
    }
}
