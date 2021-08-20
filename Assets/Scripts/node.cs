using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class node : MonoBehaviour
{
    SpriteRenderer sprite;
    public int identifier = 1;
    public List<GameObject> lineRenderers;

    public void UpdateText() { GetComponentInChildren<TextMeshProUGUI>().text = identifier.ToString(); }

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        lineRenderers = new List<GameObject>();
    }

    private void OnMouseEnter()
    {
        Color tmp = sprite.color;
        tmp.a = 0.5f;
        sprite.color = tmp;
    }

    void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (node key in Camera.main.gameObject.GetComponent<sceneManager>().g.d.Keys)
            {
                key.deletingNode(this);
            }
            Camera.main.gameObject.GetComponent<sceneManager>().g.d.Remove(this);
            Destroy(this.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Camera.main.gameObject.GetComponent<sceneManager>().activateNodeIDChanger(this);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (Camera.main.gameObject.GetComponent<sceneManager>().endNode != null && Camera.main.gameObject.GetComponent<sceneManager>().endNode == this)
            {
                Camera.main.gameObject.GetComponent<sceneManager>().showPopupMessage("The starting node can't be the ending node, try with another node.");
                return;
            }

            if (Camera.main.gameObject.GetComponent<sceneManager>().startNode != null)
            {
                Color c1 = new Color();
                ColorUtility.TryParseHtmlString("#1E629F", out c1);
                Camera.main.gameObject.GetComponent<sceneManager>().startNode.gameObject.GetComponent<SpriteRenderer>().color = c1;
            }

            Camera.main.gameObject.GetComponent<sceneManager>().startNode = this;
            Color c = new Color();
            ColorUtility.TryParseHtmlString("#56b93f", out c);
            gameObject.GetComponent<SpriteRenderer>().color = c;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Camera.main.gameObject.GetComponent<sceneManager>().startNode != null && Camera.main.gameObject.GetComponent<sceneManager>().startNode == this)
            {
                Camera.main.gameObject.GetComponent<sceneManager>().showPopupMessage("The ending node can't be the starting node, try with another node.");
                return;
            }

            if (Camera.main.gameObject.GetComponent<sceneManager>().endNode != null)
            {
                Color c1 = new Color();
                ColorUtility.TryParseHtmlString("#1E629F", out c1);
                Camera.main.gameObject.GetComponent<sceneManager>().endNode.gameObject.GetComponent<SpriteRenderer>().color = c1;
            }

            Camera.main.gameObject.GetComponent<sceneManager>().endNode = this;
            Color c = new Color();
            ColorUtility.TryParseHtmlString("#f35436", out c);
            gameObject.GetComponent<SpriteRenderer>().color = c;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Camera.main.gameObject.GetComponent<sceneManager>().endNode = this;
        }

        if (Input.GetMouseButtonDown(1))
        {
            sceneManager s = Camera.main.gameObject.GetComponent<sceneManager>();
            s.nodePressed(this);
        }
    }
    private void OnMouseExit()
    {
        Color tmp = sprite.color;
        tmp.a = 1f;
        sprite.color = tmp;
    }

    void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        foreach (node key in Camera.main.gameObject.GetComponent<sceneManager>().g.d.Keys)
        {
            key.updateLinePositions();
        }
    }

    public void deletingNode(node n)
    {
        foreach (node key in Camera.main.gameObject.GetComponent<sceneManager>().g.d[this])
        {
            if (key == n)
            {
                GameObject r = lineRenderers[Camera.main.gameObject.GetComponent<sceneManager>().g.d[this].IndexOf(key)].gameObject;
                lineRenderers.Remove(r);
                Camera.main.gameObject.GetComponent<sceneManager>().g.d[this].Remove(key);
                Destroy(r.gameObject);
                return;
            }
        }
    }

    public void updateLinePositions()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            Transform t = Camera.main.gameObject.GetComponent<sceneManager>().g.d[this][i].gameObject.transform;
            Vector3 direction = transform.position - t.position;
            Vector3 perpendicularMultiplier = Vector3.Normalize(direction);
            perpendicularMultiplier = new Vector3(-perpendicularMultiplier.y, perpendicularMultiplier.x, perpendicularMultiplier.z);
            lineRenderers[i].GetComponent<LineRenderer>().SetPosition(0, changeDepthPosition(transform.position, 1));
            lineRenderers[i].GetComponent<LineRenderer>().SetPosition(1, changeDepthPosition(t.position + (Vector3.Normalize(direction) * 0.7f) + perpendicularMultiplier * 0.2f, 1));

            lineRenderers[i].transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90);
            lineRenderers[i].transform.GetChild(0).position = t.position + Vector3.Normalize(direction) * 0.6f + perpendicularMultiplier * 0.2f;
        }
    }

    Vector3 changeDepthPosition(Vector3 i, int depth)
    {
        i = new Vector3(i.x, i.y, depth);
        return i;
    }
}