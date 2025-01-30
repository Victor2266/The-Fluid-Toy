using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public string url = "https://github.com/Victor2266/The-Fluid-Toy/";

    public void OpenLink()
    {
        Application.OpenURL(url);
    }
}