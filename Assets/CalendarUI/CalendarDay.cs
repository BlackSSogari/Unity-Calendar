using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarDay : MonoBehaviour {

    public Image dayImage;
    public Text dayText;

#if UNITY_EDITOR

    private void Reset()
    {
        dayImage = GetComponent<Image>();
        dayText = GetComponentInChildren<Text>(true);
    }

#endif
}
