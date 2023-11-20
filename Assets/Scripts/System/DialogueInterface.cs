using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueInterface : MonoBehaviour
{
    public RectTransform dialoguePanel;
    [SerializeField]
    private Image characterImage;
    public GameObject characterImageBox;
    public TMPro.TextMeshProUGUI dialogueText;
    // Start is called before the first frame update
    
    private IEnumerator SizeBox(float start, float end, bool show)
    {
        if (show) dialoguePanel.gameObject.SetActive(true);
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * 5f;
            dialoguePanel.localScale = Vector3.one * Mathf.Lerp(start, end, time);
            yield return null;
        }
        dialoguePanel.localScale = Vector3.one * end;
        if (!show) dialoguePanel.gameObject.SetActive(false);
    }
    public void ShowDialogueBox()
    {
        StartCoroutine(SizeBox(0f, 1f, true));
    }
    public void HideDialogueBox()
    {
        StartCoroutine(SizeBox(1f, 0f, false));
    }
    
    public void SetCharacterImage(Sprite sprite)
    {
        characterImage.sprite = sprite;
        characterImageBox.SetActive(sprite != null);
    }
    void Start()
    {
        dialoguePanel.gameObject.SetActive(false);
    }
}
