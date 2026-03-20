using UnityEngine;
using System.Linq;

public class TableManager : MonoBehaviour
{
    public WordValidator validator; // 在 Inspector 里把 WordManager 拖进来
    public MeshRenderer tableRenderer; // 把桌子物体拖进来

    public void ScanTable()
    {
        // 1. 获取 DetectionZone 这个 BoxCollider 范围内的所有物体
        BoxCollider area = GetComponent<BoxCollider>();
        Collider[] colliders = Physics.OverlapBox(area.bounds.center, area.bounds.extents, transform.rotation);

        // 2. 找出带 "Letter" 标签的物体，按 X 轴从左到右排好，拼成字符串
        string result = string.Join("", colliders
            .Where(c => c.CompareTag("Letter"))
            .OrderBy(c => c.transform.position.x)
            .Select(c => c.gameObject.name.Replace("(Clone)", "").Trim()));

        // 3. 调用WordValidator
        if (validator.CheckWord(result))
        {
            tableRenderer.material.color = Color.green;
            Debug.Log("<color=cyan>[Success]</color> Word found: " + result);
        }
        else
        {
            tableRenderer.material.color = Color.red;
            Debug.Log("<color=yellow>[Failed]</color> Current sequence: " + result);
        }
    }

}