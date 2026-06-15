using System;
using UnityEngine;

public class LaunchWebpage : MonoBehaviour
{
    public void OnOpenWebpage(string url)
    {
        Application.OpenURL(url);
    }
}