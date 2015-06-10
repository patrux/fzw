using UnityEngine;
using System.Collections;

public class FZW : MonoBehaviour
{
    static protected FZW instance;
    void Awake() { instance = this; }
    void OnDestroy() { instance = null; }

    public static bool IsVectorZero(Vector3 _v)
    {
        return (Mathf.Abs(_v.x) + Mathf.Abs(_v.y)) <= 0f;
    }

    public static Vector2 CopyVector2(Vector2 _v)
    {
        return new Vector2(_v.x, _v.y);
    }

    public static string WriteVector(Vector2 _v)
    {
        return "(" + _v.x + ", " + _v.y + ")";
    }

    public static string WriteVector(Vector3 _v)
    {
        return "(" + _v.x + ", " + _v.y + ")";
    }
}
