using UnityEditor;
using UnityEngine;
public class temp : MonoBehaviour
{
    [MenuItem("Microsoft/Reset Playerprefs")]
    public static void DeletePlayerPrefs() { PlayerPrefs.DeleteAll(); Debug.Log("PlayerPrefs.DeleteAll"); }
}