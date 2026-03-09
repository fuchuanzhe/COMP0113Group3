using UnityEngine;

public class WordCheckButton : MonoBehaviour
{
    public WordScanManager wordScanManager;

    public void TriggerWordCheck()
    {
        if (wordScanManager == null)
        {
            Debug.LogWarning("WordScanManager not assigned.");
            return;
        }

        Debug.Log("Word check triggered");
        wordScanManager.ScanWords();
    }
}