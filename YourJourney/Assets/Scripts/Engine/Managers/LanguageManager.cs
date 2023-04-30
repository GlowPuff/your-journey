using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LanguageManager
{
	public static List<string> languageFiles = new List<string>(); //list of language files in the Languages folder

	public static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

	public static string currentLanguage = "English";

	public static List<Action> translationUpdateSubscribers = new List<Action>();

	public static List<String> LoadLanguageFiles()
	{
		languageFiles.Clear();
		string languagesPath = Path.Combine(FileManager.BasePath(true), "Languages");
		if (!Directory.Exists(languagesPath))
		{
			Directory.CreateDirectory(languagesPath);
		}

		string[] files = Directory.GetFiles(languagesPath, "*.json");
		for (int i = 0; i<files.Length; i++)
		{
			files[i] = new DirectoryInfo(files[i]).Name;
		}

		languageFiles.Clear();
		languageFiles.AddRange(files);
		return languageFiles;
	}

	public static string LanguageNameFromFilename(string filename)
    {
		return Path.GetFileNameWithoutExtension(filename);
    }

	public static void LoadLanguage(string languageName)
	{
		Debug.Log("LoadLanguage: " + languageName);
		string languagePath = Path.Combine(FileManager.BasePath(false), "Languages", languageName + ".json");
		if(File.Exists(languagePath))
        {
			Debug.Log(languageName + ".json exists");
			var json = File.ReadAllText(languagePath);
			var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			foreach(KeyValuePair<string, string> pair in keyValuePairs)
            {
				Debug.Log(pair.Key + " => " + pair.Value);
            }
			translations.Remove(languageName);
			translations.Add(languageName, keyValuePairs);
        }
	}

	public static string Translate(string key, string defval=null)
    {
		if(translations.ContainsKey(currentLanguage))
        {
			var keyval = translations[currentLanguage];
			if(keyval.ContainsKey(key))
            {
				return keyval[key];
            }
        }

		if(defval != null)
		{
			return defval;
		}
		return key;
    }

	public static string Translate(string key, List<string> values)
    {
		string translation = Translate(key);
		if (values != null && values.Count > 0)
		{
			translation = string.Format(translation, values.ToArray());
		}
		return translation;
	}

	public static string Translate(string key, string defval, List<string> values)
	{
		string translation = Translate(key, defval);
		if (values != null && values.Count > 0)
		{
			translation = string.Format(translation, values.ToArray());
		}
		return translation;
	}

	public static void AddSubscriber(Action onUpdateTranslation)
    {
		translationUpdateSubscribers.Add(onUpdateTranslation);
    }

	public static void RemoveSubscriber(Action onUpateTranslation)
    {
		translationUpdateSubscribers.Remove(onUpateTranslation);
    }

	public static void CallSubscribers()
    {
		Debug.Log("LanguageManager.CallSubscribers: " + translationUpdateSubscribers.Count);
		foreach(Action onUpdateTranslation in translationUpdateSubscribers)
        {
			onUpdateTranslation();
        }
    }
}
