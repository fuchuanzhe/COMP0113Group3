using UnityEngine;

public class WordCheckButton : MonoBehaviour
{
    public WordScanManager wordScanManager;

    public void TriggerWordCheck()
    {
        if (wordScanManager == null)
        {
            // WordScanManager not assigned.
            return;
        }

        wordScanManager.ScanWords();
    }
}