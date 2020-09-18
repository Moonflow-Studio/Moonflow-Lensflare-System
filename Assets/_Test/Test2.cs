using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    public URPFlareLauncher launcher;

    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        bool isnull = !(launcher.asset == null);
        text.text = isnull.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
