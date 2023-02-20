using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class DynamicText : MonoBehaviour
{
    public enum HorizontalAlignmentEnum { left, middle, right };
    public enum VerticalAlignmentEnum { bottom, middle, top };

    public GameObject CharacterContainerClass = null;
    public GameObject emptyGameObjectSource = null;

    public float fontSize = 0.5f;
    public HorizontalAlignmentEnum horizontalAlignment = HorizontalAlignmentEnum.left;
    public VerticalAlignmentEnum verticalAlignment = VerticalAlignmentEnum.top;
    public float lineSpacing = 0f;
    public float characterSpacing = 0f;
    public float wordSpacing = 0f;
    [TextArea(3, 10)]
    public string text;
    public bool readText = true;
    public float readDelay = 0.2f;
    public AudioClip readSound;

    [HideInInspector]
    public bool reading = false;

    public UnityEvent onFinishReading;

    Coroutine readCouroutine;

    private List<CharacterContainer> containers = new List<CharacterContainer>();
    private List<GameObject> lines = new List<GameObject>();

    private Color color = Color.white;
    private bool bold = false;
    private bool italic = false;

    private float defaultFontSize;
    private float defaultReadDelay;
    private int readProgress = 0;
    private AudioSource audioSource;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource, "Missing AudioSource component.");
        defaultFontSize = fontSize;
        defaultReadDelay = readDelay;

        ResetText();
    }

    public void SetText(string str)
    {
        //Set it's text
        text = str;

        color = Color.white;
        fontSize = defaultFontSize;
        bold = false;
        italic = false;
        readDelay = defaultReadDelay;

        DestroyContainers();
        DestroyLines();

        foreach (Transform child in transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

        //Create the containers that will hold our characters.
        MakeContainers();

        if (readText)
        {
            StartReadingText();
        }
        else
        {
            onFinishReading.Invoke();
        }


        readDelay = defaultReadDelay;
    }

    private void StartReadingText()
    {
        HideContainers();

        readProgress = 0;
        RestartCoroutine(ref readCouroutine, ReadText());
    }

    IEnumerator ReadText()
    {
        reading = true;
        while (true)
        {
            if (readProgress >= containers.Count) break;
            
            yield return new WaitForSecondsRealtime(containers[readProgress].readDelay);

            if (containers[readProgress] != null) containers[readProgress].gameObject.SetActive(true);
            bool isSpace = containers[readProgress].textMesh.text == " ";
            if(readSound && !isSpace) audioSource.PlayOneShot(readSound);
            readProgress++;
        }
        reading = false;
        onFinishReading.Invoke();
    }


    private void RestartCoroutine(ref Coroutine coroutineVariable, IEnumerator coroutineMethod)
    {
        if (coroutineVariable != null) StopCoroutine(coroutineVariable);
        coroutineVariable = StartCoroutine(coroutineMethod);
    }

    public void SkipReading()
    {
        if (readCouroutine != null) StopCoroutine(readCouroutine);
        ShowContainers();
        if (reading)
        {
            reading = false;
            onFinishReading.Invoke();
        }
    }

    private void ShowContainers()
    {
        foreach (CharacterContainer c in containers)
        {
            c.gameObject.SetActive(true);
        }
    }

    private void HideContainers()
    {
        foreach (CharacterContainer c in containers)
        {
            c.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Make all of our child character containers
    /// </summary>
    /// <param name="CharacterContainerClass"></param>
    /// <returns></returns>
    void MakeContainers()
    {
        DestroyContainers();
        DestroyLines();

        containers = new List<CharacterContainer>();

        char[] cArray = text.ToCharArray();

        List<CharacterContainer> newList = new List<CharacterContainer>();

        //escape is used to ignore '<' formatting by adding a '/' before it.
        bool escape = false;

        for (int i = 0; i < cArray.Length; i++)
        {
            char c = cArray[i];
            if (c == '\0')
            {
                continue;
            }

            if (c == '/' && !escape)
            {
                escape = true;
                continue;
            }

            if (c == '<' && !escape)
            {
                int remainingCharCount = text.Length - (i + 1);
                //<d> example <d> resets the formatting to default
                if (ValidTagCheck("default", text, i))
                {
                    i += "default".Length + 1;

                    color = Color.white;
                    fontSize = defaultFontSize;
                    italic = false;
                    bold = false;
                    readDelay = defaultReadDelay;
                }
                //<color.[colorname]> example: <color.blue>
                //Change the font color
                else if (ValidTagCheck("color", text, i))
                {

                    i += "color".Length + 1;

                    int j;
                    for (j = 0; j < text.Length; j++)
                    {
                        if (cArray[i + j] == '>')
                        {
                            break;
                        }
                    }

                    string str = text.Substring(i + 1, j - 1);
                    i += j;

                    switch (str)
                    {
                        case "white":
                            color = Color.white;
                            break;
                        case "blue":
                            color = Color.blue;
                            break;
                        case "red":
                            color = Color.red;
                            break;
                        case "yellow":
                            color = Color.yellow;
                            break;
                        case "green":
                            color = Color.green;
                            break;
                        case "purple":
                            color = Color.magenta;
                            break;
                        case "tan":
                            color = new Color(212, 214, 185);
                            break;
                    }
                }
                //<delay.[delayInSeconds]> example <delay.0.2>
                //changes the read speed
                else if (ValidTagCheck("delay", text, i))
                {

                    i += "delay".Length + 1;

                    int j;
                    for (j = 0; j < text.Length; j++)
                    {
                        if (cArray[i + j] == '>')
                        {
                            break;
                        }
                    }

                    string str = text.Substring(i + 1, j - 1);
                    i += j;
                    
                    readDelay = Convert.ToSingle(str);
                }
                //<size.[fontscale]> example <size.0.58> (about 0.4-0.6 is advised)
                //changes font size.
                else if (ValidTagCheck("size", text, i))
                {
                    i += "size".Length + 1;

                    int j;
                    for (j = 0; j < text.Length; j++)
                    {
                        if (cArray[i + j] == '>')
                        {
                            break;
                        }
                    }

                    string str = text.Substring(i + 1, j - 1);
                    i += j;

                    fontSize = Convert.ToSingle(str);
                }
                //<bold>
                //toggles bolding
                else if (ValidTagCheck("bold", text, i))
                {
                    i += "bold".Length + 1;
                    bold = !bold;
                }
                //<italic>
                //toggles italics
                else if (ValidTagCheck("italic", text, i))
                {
                    i += "italic".Length + 1;
                    italic = !italic;
                }
                continue;
            }

            escape = false;

            CharacterContainer newChar = Instantiate(CharacterContainerClass).GetComponent<CharacterContainer>();
            TextMeshProUGUI tm = newChar.textMesh;
            tm.text = "" + cArray[i];
            tm.color = color;
            tm.fontSize = fontSize;

            if (bold)
            {
                if (italic)
                {
                    tm.fontStyle = FontStyles.Bold | FontStyles.Italic;
                }
                else
                {
                    tm.fontStyle = FontStyles.Bold;
                }

            }
            else if (italic)
            {
                tm.fontStyle = FontStyles.Italic;
            }
            else
            {
                tm.fontStyle = FontStyles.Normal;
            }

            newChar.readDelay = readDelay;

            newList.Add(newChar);
            tm.ForceMeshUpdate();
        }

        containers = newList;
        AddCharacterContainersToLines();
    }

    private bool ValidTagCheck(string tag, string text, int index)
    {
        int remainingCharCount = text.Length - (index + 1);

        return text.Substring(index + 1, Mathf.Min(tag.Length, remainingCharCount)) == tag;
    }

    /// <summary>
    /// This function loops through all of the text containers created by the MakeContainers function positions them. Each text container is parented to an empty game object.
    /// When the text containers overflow the maxSize.x, a new empty game object (a line) is made below and the text wraps to the new game object. 
    /// </summary>
    void AddCharacterContainersToLines()
    {
        RectTransform rectTransform = (RectTransform)transform;

        float boxWidth = rectTransform.rect.width * rectTransform.lossyScale.x;

        //Destroy all of the current lines which contain CharacterContainers
        DestroyLines();
        //Create an empty list of lines.
        lines = new List<GameObject>();

        //Create the first line, and immediately set its local scale to 1 to avoid resizing issues.
        GameObject currentLine = Instantiate(emptyGameObjectSource, this.transform);
        currentLine.name = "Line" + lines.Count;
        currentLine.transform.localScale = new Vector3(1, 1, 1);

        //Add our newly created line to the empty lines list
        lines.Add(currentLine);

        // width is the width of out current line in unity units. Each time a character is added, it's char width is added to this value
        // We use this value to place each character to the right of the last.
        float width = 0;
        
        // this value holds the index of the last "wrapable" character (usually a space) in the line. We try to break at this index to avoid cutting words in half.
        int wrapIndex = 0;

        // this value holds the total width of the characters we have added since the last "wrapable" character. If a word is longer than textbox width, we have to cut it when we wrap.
        float wordWidth = 0;

        for (int i = 0; i < containers.Count; i++)
        {
            // The container at our current i index
            CharacterContainer currentContainer = containers[i];
            // The child text mesh of our current character.
            TextMeshProUGUI tm = currentContainer.textMesh;

            //if there is a new line character, we need to wrap.
            if (tm.text[0] == '\n')
            {
                // Since we are wrapping, we need to create a new line object.
                currentLine = Instantiate<GameObject>(emptyGameObjectSource, this.transform);
                currentLine.name = "Line" + lines.Count;
                // The following line is required to prevent the current container from inheriting the scale of the parent of the currentLine. All of our scaling is coming from resizing the Entry.
                currentLine.transform.localScale = new Vector3(1, 1, 1);
                lines.Add(currentLine);

                // since we are wrapping, reset width and word width.
                width = 0;
                wordWidth = 0;
                continue;
            }

            // Parent our currentContainer to our currentLine
            currentContainer.transform.SetParent(currentLine.transform, true);
            // The following line is required to prevent the current container from inheriting the scale of the parent of the currentLine. All of our scaling is coming from resizing the Entry.
            currentContainer.transform.localScale = new Vector3(1, 1, 1);

            // Set the position of our currentCharacter "width distance" to the right of the currentline's position
            currentContainer.transform.position = currentLine.transform.position + new Vector3(width, 0, 0);

            // Get the char of our current character.
            char c = tm.text.ToCharArray()[0];

            // Get the width of our current character.
            float charWidth;
            if (c == ' ')
            {
                // If this is a wrap character, make it our new wrap index and reset word width
                wrapIndex = i;
                wordWidth = 0;

                charWidth = currentContainer.GetWidth() + wordSpacing;
            }
            else
            {
                charWidth = currentContainer.GetWidth() + characterSpacing;

                // Otherwise continue to add to our current wordWidth
                wordWidth += charWidth * transform.lossyScale.x;
            }

            // If a single character is ever larger than our text box width, something is horribly wrong.
            if (charWidth * transform.lossyScale.x > boxWidth)
            {
                Debug.Log("Text box is smaller than a single character!!!!!!!");
                break;
            }

            // Increase our width by the size of our character
            width += charWidth * transform.lossyScale.x;

            // When our width becomes larger than our maxSize.x, we need to line break
            if (width > boxWidth)
            {
                //If there is at least one ' ' on the line, wrap there, else, wrap at the last character
                // This should prevent infinite loops from happening when a single word is longer than a line.
                if(wordWidth != width)
                {
                    i = wrapIndex;
                }
                else
                {
                    i--;
                }
                
                // Since we are wrapping, we need to create a new line object.
                currentLine = Instantiate<GameObject>(emptyGameObjectSource, this.transform);
                currentLine.name = "Line" + lines.Count;
                // The following line is required to prevent the current container from inheriting the scale of the parent of the currentLine. All of our scaling is coming from resizing the Entry.
                currentLine.transform.localScale = new Vector3(1, 1, 1);
                lines.Add(currentLine);

                // since we are wrapping, reset width and word width.
                width = 0;
                wordWidth = 0;
                continue;
            }
        }

        Align();
    }

    /// <summary>
    /// Return the width of the line supplied.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    float GetLineWidth(GameObject line)
    {
        CharacterContainer[] childContainers = line.GetComponentsInChildren<CharacterContainer>();
        float w = 0;
        foreach(CharacterContainer tc in childContainers)
        {
            float charWidth;
            if (tc.textMesh.text.ToCharArray()[0] == ' ')
            {
                charWidth = tc.GetWidth() + wordSpacing;
            }
            else
            {
                charWidth = tc.GetWidth() + characterSpacing;
            }

            w += charWidth * transform.lossyScale.x;
        }

        //The following part removes the trailing spaces from the length of the line.
        int i = childContainers.Length - 1;
        while (i >= 0 && childContainers[i].textMesh.text[0] == ' ')
        {
            w -= (childContainers[i].GetWidth() + wordSpacing) * transform.lossyScale.x;
            i--;
        }

        return w;
    }

    /// <summary>
    /// Align each line in lines based on the alignment enum.
    /// </summary>
    public void Align()
    {
        RectTransform rectTransform = (RectTransform)transform;

        float boxWidth = rectTransform.rect.width * rectTransform.lossyScale.x;
        float boxHeight = rectTransform.rect.height * rectTransform.lossyScale.y;

        Vector3 topLeftCorner = rectTransform.position + (Vector3)rectTransform.rect.center - Vector3.right * (boxWidth / 2) + Vector3.up * (boxHeight / 2);

        // Vertical Alignment
        float yAlign;
        if (verticalAlignment == VerticalAlignmentEnum.top)
        {
            yAlign = 0;
        }
        else if (verticalAlignment == VerticalAlignmentEnum.middle)
        {
            yAlign = (boxHeight / 2) - (GetPixelHeight() / 2);
        }
        else
        {
            yAlign = boxHeight - GetPixelHeight();
        }

        for ( int i = 0; i < lines.Count; i++)
        {
            // Horizontal Alignment
            float xAlign;
            if (horizontalAlignment == HorizontalAlignmentEnum.left)
            {
                xAlign = 0;
            }
            else if (horizontalAlignment == HorizontalAlignmentEnum.middle)
            {
                xAlign = (boxWidth / 2) - (GetLineWidth(lines[i]) / 2);
            }
            else
            {
                xAlign = boxWidth - (GetLineWidth(lines[i]));
            }


            lines[i].transform.position = topLeftCorner + new Vector3(xAlign, (i * (lineSpacing + fontSize) * transform.lossyScale.y * -1) - yAlign, 0);
        }
    }

    void DestroyLines()
    {
        if (lines == null) return;
        foreach (GameObject o in lines)
        {
            if(o) Destroy(o.gameObject);
        }
        lines.Clear();
    }

    void DestroyContainers()
    {
        if (containers == null) return;
        foreach (CharacterContainer tc in containers)
        {
            if (tc) Destroy(tc.gameObject);
        }
        containers.Clear();
    }

    // Return the height of the message in lines.
    public int GetHeight()
    {
        if (lines == null) return 0;
        return lines.Count;
    }

    // Return the height of the message in pixels.
    public float GetPixelHeight()
    {
        if (lines == null) return 0;
        return lines.Count * (lineSpacing + fontSize) * transform.lossyScale.y;
    }

    public void ResetText()
    {
        defaultFontSize = fontSize;
        defaultReadDelay = readDelay;
        SetText(text);
    }

    void OnDrawGizmosSelected()
    {
        RectTransform rectTransform = (RectTransform)transform;

        float boxWidth = rectTransform.rect.width * rectTransform.lossyScale.x;
        float boxHeight = rectTransform.rect.height * rectTransform.lossyScale.y;

        Vector3 topLeftCorner = rectTransform.position + (Vector3)rectTransform.rect.center - Vector3.right * (boxWidth / 2) + Vector3.up * (boxHeight / 2);// + new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax, 0);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(topLeftCorner, topLeftCorner + new Vector3(boxWidth, 0, 0));
        Gizmos.DrawLine(topLeftCorner, topLeftCorner + new Vector3(0, -boxHeight, 0));
        Gizmos.DrawLine(topLeftCorner + new Vector3(0, -boxHeight, 0), topLeftCorner + new Vector3(boxWidth, -boxHeight, 0));
        Gizmos.DrawLine(topLeftCorner + new Vector3(boxWidth, 0, 0), topLeftCorner + new Vector3(boxWidth, -boxHeight, 0));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(topLeftCorner + new Vector3(boxWidth / 2, 0, 0), topLeftCorner + new Vector3(boxWidth / 2, -boxHeight, 0));
    }
}

[CustomEditor(typeof(DynamicText))]
public class EntryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DynamicText myScript = (DynamicText)target;
        if (GUILayout.Button("Update Text"))
        {
            myScript.ResetText();
        }

        if (GUILayout.Button("Skip Reading"))
        {
            myScript.SkipReading();
        }
    }
}
