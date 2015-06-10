using UnityEngine;
using System.Collections;

public class LocalUI : MonoBehaviour 
{
    public delegate void EventHandler();
    public static event EventHandler OnEnableUI;

    // Local player elements
    Camera camera;
    Minimap minimap;

    public void InitializeLocalUI()
    {
        camera = GameObject.Find("MainCamera").GetComponent<Camera>();

        minimap = (Minimap)InitializeElement("Minimap", "UI/Minimap", "Minimap");

        // UI enabled event
        if (OnEnableUI != null)
            OnEnableUI();
    }

    Component InitializeElement(string _objectName, string _resources, string _component)
    {
        GameObject go = (GameObject)Instantiate(Resources.Load(_resources), Vector3.zero, Quaternion.identity);
        go.transform.parent = gameObject.transform;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        go.name = _objectName;
        return go.GetComponent(_component);
    }
}
