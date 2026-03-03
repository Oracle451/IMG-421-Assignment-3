using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LookAtAttractor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Attractor.POS);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
