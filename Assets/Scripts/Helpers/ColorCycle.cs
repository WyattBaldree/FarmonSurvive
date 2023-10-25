using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorCycle : MonoBehaviour
{
    Image image;

    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Color.RGBToHSV(image.color, out float H, out float S, out float V);
        image.color = Color.HSVToRGB(H + speed * Time.deltaTime, S, V); 
    }
}
