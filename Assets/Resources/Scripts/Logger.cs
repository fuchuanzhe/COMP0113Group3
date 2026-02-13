using UnityEngine;
using System.IO;
using System;

// adb pull /sdcard/Android/data/com.COMP0113Group3.WordBreak/files/log.txt
public class Logger : MonoBehaviour
{
    public static Logger Instance;

    private string logPath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        logPath = Path.Combine(Application.persistentDataPath, "log.txt");

        Log("===========");
    }

    public void Log(string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

        Debug.Log(line);

        try
        {
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write log file: " + e.Message);
        }
    }

    public void Clear()
    {
        if (File.Exists(logPath))
            File.Delete(logPath);
    }
}
