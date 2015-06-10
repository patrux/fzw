using UnityEngine;
using System.Collections;

public class PlayerUI : MonoBehaviour 
{
    public delegate void EventHandler();
    public static event EventHandler OnUIEnabled;

    // Global player elements
    Castbar castbar;
    // Lifebar lifebar;

    public void InitializeUI()
    {
        castbar = (Castbar)InitializeElement("Castbar", "UI/Castbar", "Castbar");

        // UI enabled event
        if (OnUIEnabled != null)
            OnUIEnabled();
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
