using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class vSliderSizeControl : MonoBehaviour {
    public Slider slider;
    public RectTransform rectTransform;
    public float multipScale=0.1f;
    float oldMaxValue;
    void OnDrawGizmosSelected()
    {
        UpdateScale();
    }
	public void UpdateScale()
    {
        if (rectTransform && slider)
        {          
            if (slider.maxValue!=oldMaxValue)
            {
                var sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = slider.maxValue * multipScale;
                rectTransform.sizeDelta = sizeDelta;
                oldMaxValue = slider.maxValue;                             
            }
        }
    }
}
