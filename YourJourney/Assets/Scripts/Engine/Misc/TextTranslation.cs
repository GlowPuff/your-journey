using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TextTranslation : MonoBehaviour
{
    TMP_Text TMPTextObject;
    Text TextObject;
	public string TextKey;
    public string DefaultText;
    public List<string> Values;

	void Start()
    {
        TMPTextObject = GetComponent<TMP_Text>();
        TextObject = GetComponent<Text>();
        OnUpdateTranslation();
        LanguageManager.AddSubscriber(OnUpdateTranslation);
    }

    public void Change(string key)
    {
        TextKey = key;
        OnUpdateTranslation();
    }

    public void Change(List<string> values)
    {
        Values = values;
        OnUpdateTranslation();
    }

    public void Change(string key, List<string> values)
    {
        TextKey = key;
        Values = values;
        OnUpdateTranslation();
    }

    public void OnUpdateTranslation()
    {
        Debug.Log("OnUpdateTranslation for " + TextKey + " / " + DefaultText + " with TextObject class " + TextObject?.GetType());
        if (!string.IsNullOrWhiteSpace(TextKey))
        {
            TextKey = TextKey.Trim();
        }

        string translation = LanguageManager.Translate(TextKey, DefaultText);
        if (Values != null && Values.Count > 0)
        {
            translation = string.Format(translation, Values.ToArray());
        }

        if (TextObject != null)
        {
            TextObject.text = translation;
        }

        if (TMPTextObject != null)
        {
            TMPTextObject.text = translation;
        }
    }

    void OnDestroy()
    {
        LanguageManager.RemoveSubscriber(OnUpdateTranslation);
    }

    void OnDisable()
    {
        LanguageManager.RemoveSubscriber(OnUpdateTranslation);
    }
}
