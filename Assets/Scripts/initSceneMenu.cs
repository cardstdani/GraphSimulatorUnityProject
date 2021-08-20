using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class initSceneMenu : MonoBehaviour
{
    public List<GameObject> nodes;
    public GameObject welcomeText, spawner;
    public float speed = 10f, timer = 0, rate = 10, angularSpeed = 60;

    void Start()
    {

    }
    void Update()
    {
        welcomeText.transform.Translate(Vector3.left * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer > rate)
        {
            timer = 0;
            rate = Random.Range(8, 12);
            welcomeText.transform.position = spawner.transform.position;
            welcomeText.GetComponent<TextMeshProUGUI>().text = "!¡Go!¡";
        }

        foreach (GameObject n in nodes)
        {
            n.transform.Rotate(new Vector3(0, 0, angularSpeed * Time.deltaTime * Random.Range(0, 3)));
        }
    }

    public void play()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Graphs");
    }
}
