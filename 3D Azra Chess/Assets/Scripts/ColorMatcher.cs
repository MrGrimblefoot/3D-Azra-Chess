using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorMatcher : MonoBehaviour
{
    [SerializeField] Image imageToMatch;
    TextMeshProUGUI text;
    float tick;

    private void Start() { text = GetComponent<TextMeshProUGUI>(); }

    void Update()
    {
        /*if (tick >= 50) { */text.color = imageToMatch.color; /*tick = 0; }*/
        //else { tick += Time.deltaTime; }
    }
}
