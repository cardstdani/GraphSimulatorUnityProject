using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class tutorialManager : MonoBehaviour
{
    string[] messages = new string[10];
    int currentMessage = 0;
    sceneManager manager;

    void Start()
    {
        manager = GetComponent<sceneManager>();
        messages[0] = "To skip the tutorial you can press J key.";
        messages[1] = "If you want to create a node, you can add one to the scene by pressing the Z key. To delete it, press the X key.";
        messages[2] = "To link nodes, use the right mouse button over the created nodes.";
        messages[3] = "You can delete a link between nodes by creating again the same link, it will automatically disappear.";
        messages[4] = "In the top of the screen, there is a bar where you have some graph algorithms to test in the scene.";
        messages[5] = "Some algorithms require a starting or ending node. You can select them by pressing S and E keys.";
        messages[6] = "When you have selected the starting and ending node, you can execute an algorithm and you will see its result.";
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            manager.popup.SetActive(false);
            Destroy(this);
        }
    }

    public void closeButtonPressed()
    {
        currentMessage++;
        if (currentMessage > 6)
        {
            manager.popup.SetActive(false);
            Destroy(this);
        }
        else
        {
            manager.popup.GetComponentInChildren<TextMeshProUGUI>().text = messages[currentMessage];
            manager.popup.SetActive(true);
        }
    }
}
