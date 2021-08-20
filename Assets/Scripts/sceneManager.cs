using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class sceneManager : MonoBehaviour
{
    public float cameraSizeModifier = 10f;
    float animationTime = 0.5f;
    int maxNodeNumber = 1;
    public GameObject nodeObj, triangleObj, nodeIDChanger, animTimeChanger, popup;
    public TMP_InputField idInput, timeInput;
    public graph g;
    public bool creatingLink = false;
    public Material lineMat;
    node currentNode, selectedNode;
    public node startNode, endNode;

    void Start()
    {
        g = this.gameObject.AddComponent<graph>();
        g.d.Add(GameObject.Find("Node").GetComponent<node>(), new List<node>());
        currentNode = GameObject.Find("Node").GetComponent<node>();
        popup.SetActive(true);
    }

    void Update()
    {
        if (Camera.main.orthographicSize > 4)
        {
            Camera.main.orthographicSize += Input.GetAxis("Horizontal") * cameraSizeModifier * Time.deltaTime;
        }
        else { Camera.main.orthographicSize = 4.01f; }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameObject newObj = Instantiate(nodeObj, getMousePosition(0), Quaternion.Euler(new Vector3(0, 0, 0)));

            maxNodeNumber++;
            newObj.GetComponentInChildren<node>().identifier = maxNodeNumber;
            newObj.GetComponentInChildren<node>().UpdateText();
            g.d.Add(newObj.GetComponentInChildren<node>(), new List<node>());
        }

        if (creatingLink)
        {
            if (Input.GetKeyDown(KeyCode.C)) { cancelOutLink(); creatingLink = false; return; }
            Vector3 mousePosition = getMousePosition(1);
            Vector3 direction = currentNode.transform.position - mousePosition;
            currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].GetComponent<LineRenderer>().SetPosition(1, mousePosition + (Vector3.Normalize(direction) * 0.2f));

            currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90);
            currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].transform.GetChild(0).position = mousePosition + Vector3.Normalize(direction) * 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            resetGraphColors();
            startNode = null;
            endNode = null;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (animTimeChanger.activeInHierarchy == true) { animTimeChanger.SetActive(false); return; }
            if (nodeIDChanger.activeInHierarchy != true)
            {
            //    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("InitGraphs");
            }
            nodeIDChanger.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (animTimeChanger.activeInHierarchy != true)
            {
                animTimeChanger.SetActive(true);
            }
            else { animTimeChanger.SetActive(false); }
        }
    }

    public void showPopupMessage(string text)
    {
        popup.GetComponentInChildren<TextMeshProUGUI>().text = text;
        popup.SetActive(true);
    }

    void resetGraphColors()
    {
        Color c = new Color();
        ColorUtility.TryParseHtmlString("#1E629F", out c);
        foreach (node n in g.d.Keys)
        {
            n.gameObject.GetComponent<SpriteRenderer>().color = c;
            foreach (GameObject obj in n.lineRenderers)
            {
                obj.GetComponent<LineRenderer>().startColor = Color.white;
                obj.GetComponent<LineRenderer>().endColor = Color.white;
            }
        }
    }

    Vector3 getMousePosition(int depth)
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        position = new Vector3(position.x, position.y, depth);
        return position;
    }

    public void activateNodeIDChanger(node n)
    {
        nodeIDChanger.SetActive(true);
        selectedNode = n;
        idInput.text = selectedNode.identifier.ToString();
    }

    public void changeNodeID()
    {
        selectedNode.identifier = int.Parse(idInput.text);
        selectedNode.UpdateText();
    }

    public void changeAnimationTime()
    {
        animationTime = float.Parse(timeInput.text);
    }

    public void nodePressed(node n)
    {
        if (creatingLink)
        {
            if (n == currentNode) { cancelOutLink(); creatingLink = false; return; }
            creatingLink = false;
            if (g.d[currentNode].Contains(n))
            {
                GameObject lr = currentNode.lineRenderers[g.d[currentNode].IndexOf(n)];
                currentNode.lineRenderers.Remove(lr);
                Destroy(lr.gameObject);
                cancelOutLink();
                g.d[currentNode].Remove(n);
            }
            else
            {
                Vector3 direction = currentNode.transform.position - n.transform.position;
                Vector3 perpendicularMultiplier = Vector3.Normalize(direction);
                perpendicularMultiplier = new Vector3(-perpendicularMultiplier.y, perpendicularMultiplier.x, perpendicularMultiplier.z);
                currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].GetComponent<LineRenderer>().SetPosition(1, changeDepthPosition(n.transform.position + (Vector3.Normalize(direction) * 0.7f) + 0.2f * perpendicularMultiplier, 1));

                currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].transform.GetChild(0).position = n.transform.position + Vector3.Normalize(direction) * 0.6f + perpendicularMultiplier * 0.2f;
                currentNode.lineRenderers[currentNode.lineRenderers.Count - 1].transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90);
                g.d[currentNode].Add(n);
            }
        }
        else
        {
            creatingLink = true;
            //Setup new line
            GameObject newLr = new GameObject();
            newLr.transform.SetParent(n.transform);
            newLr.AddComponent<LineRenderer>();
            LineRenderer lr = newLr.gameObject.GetComponent<LineRenderer>();
            n.lineRenderers.Add(newLr);
            lr.widthMultiplier = 0.07f;
            lr.positionCount = 2;
            lr.sortingOrder = 0;
            newLr.GetComponent<Renderer>().material = lineMat;
            lr.SetPosition(0, changeDepthPosition(n.transform.position, 1));

            //Setup triangle
            GameObject newTriangle = Instantiate(triangleObj);
            newTriangle.transform.SetParent(newLr.transform);
        }
        currentNode = n;
    }

    Vector3 changeDepthPosition(Vector3 i, int depth)
    {
        i = new Vector3(i.x, i.y, depth);
        return i;
    }

    public void cancelOutLink()
    {
        GameObject last = currentNode.lineRenderers[currentNode.lineRenderers.Count - 1];
        currentNode.lineRenderers.Remove(last);
        Destroy(last);
    }

    public void bfs()
    {
        g.animTime = animationTime;
        if (startNode != null)
        {
            if (endNode != null)
            {
                resetGraphColors();
                StartCoroutine(g.bfs2(startNode, endNode));
            }
            else
            {
                resetGraphColors();
                StartCoroutine(g.bfs(startNode));
            }
        }
        else
        {
            showPopupMessage("Breadth First Search algorithm requires a starting node, please select one pressing the S key.");
        }
    }

    public void dfs()
    {
        if (startNode != null)
        {
            resetGraphColors();
            g.dfsVisited = new bool[g.d.Keys.Count];
            g.animTime = animationTime;
            StartCoroutine(g.dfs(startNode));
        }
        else
        {
            showPopupMessage("Depth First Search algorithm requires a starting node, please select one pressing the S key.");
        }
    }
    public void dijkstra()
    {
        if (startNode != null && endNode != null)
        {
            resetGraphColors();
            g.animTime = animationTime;
            StartCoroutine(g.dijkstra(startNode, endNode));
        }
        else
        {
            showPopupMessage("Dijkstra's algorithm requires a starting node and an ending node, please select them pressing the S and E keys.");
        }
    }

    public void aStar()
    {
        if (startNode != null && endNode != null)
        {
            resetGraphColors();
            g.animTime = animationTime;
            StartCoroutine(g.aStar(startNode, endNode));
        }
        else
        {
            showPopupMessage("A star algorithm requires a starting node and an ending node, please select them pressing the S and E keys.");
        }
    }

    public void tSort()
    {
        resetGraphColors();
        g.animTime = animationTime;
        StartCoroutine(g.tSort());
    }

    public void primMST()
    {
        if (startNode != null)
        {
            resetGraphColors();
            g.animTime = animationTime;
            StartCoroutine(g.pMST(startNode));
        }
        else
        {
            showPopupMessage("Prim's MST algorithm requires a starting node, please select one pressing the S key.");
        }
    }

    public void findCC()
    {
        resetGraphColors();
        g.dfsVisited = new bool[g.d.Keys.Count];
        g.animTime = animationTime;
        StartCoroutine(g.findConnectedComponents());
    }
    public void findSCC()
    {
        resetGraphColors();
        g.dfsVisited = new bool[g.d.Keys.Count];
        g.animTime = animationTime;
        StartCoroutine(g.findStronglyConnectedComponents());
    }

    public void eulerianPath()
    {
        resetGraphColors();
        StartCoroutine(g.ep());
    }

    public void bipartiteGraphCheck()
    {
        if (startNode != null)
        {
            resetGraphColors();
            g.animTime = animationTime;
            StartCoroutine(g.bipartiteGraphCheck(startNode));
        }
        else
        {
            showPopupMessage("Bipartite graph check requires a starting node, please select one pressing the S key.");
        }
    }

    public void edmonsKarp()
    {
        if (startNode != null && endNode != null)
        {
            resetGraphColors();
            g.animTime = animationTime;
            StartCoroutine(g.ek(startNode, endNode));
        }
        else
        {
            showPopupMessage("Edmonds Karp algorithm requires a starting node and an ending node, please select them pressing the S and E keys.");
        }
    }

    public void bridges()
    {
        resetGraphColors();
        g.animTime = animationTime;
        StartCoroutine(g.bridges());
    }
}

public class graph : MonoBehaviour
{
    public Dictionary<node, List<node>> d = new Dictionary<node, List<node>>();
    public bool[] dfsVisited;
    public float animTime = 0.5f;
    string[] colors = new string[10];
    void Start()
    {
        colors[0] = "#F25436";
        colors[1] = "#C8843A";
        colors[2] = "#5db051";
        colors[3] = "#fe72b8";
        colors[4] = "#827ef2";
        colors[5] = "#FACB07";
    }

    public IEnumerator bridges()
    {
        List<node> bridgesList = new List<node>();
        dfsVisited = new bool[d.Keys.Count];
        id = 0;
        ids = new int[d.Keys.Count];
        low = new int[d.Keys.Count];

        for (int i = 0; i < d.Keys.Count; i++)
        {
            if (!dfsVisited[i])
            {
                yield return StartCoroutine(bridgesDFS(i, -1, bridgesList));
            }
        }

        for (int a = 0; a < bridgesList.Count; a += 2)
        {
            node start = bridgesList[a];
            node end = bridgesList[a + 1];

            Color c = new Color();
            ColorUtility.TryParseHtmlString("#C8843A", out c);

            start.lineRenderers[d[start].IndexOf(end)].gameObject.GetComponent<LineRenderer>().startColor = c;
            start.lineRenderers[d[start].IndexOf(end)].gameObject.GetComponent<LineRenderer>().endColor = c;
            start.GetComponent<SpriteRenderer>().color = c;
            end.GetComponent<SpriteRenderer>().color = c;
        }
        yield break;
    }

    IEnumerator bridgesDFS(int at, int parent, List<node> bridgesList)
    {
        dfsVisited[at] = true;
        id++;
        low[at] = id;
        ids[at] = id;

        foreach (node n in d[new List<node>(d.Keys)[at]])
        {
            int to = new List<node>(d.Keys).IndexOf(n);
            if (new List<node>(d.Keys).IndexOf(n) != parent)
            {
                if (!dfsVisited[to])
                {
                    yield return StartCoroutine(bridgesDFS(to, at, bridgesList));
                    low[at] = Mathf.Min(low[at], low[to]);

                    if (ids[at] < low[to])
                    {
                        bridgesList.Add(new List<node>(d.Keys)[at]);
                        bridgesList.Add(new List<node>(d.Keys)[to]);
                    }
                }
                else
                {
                    low[at] = Mathf.Min(low[at], ids[to]);
                }
            }
        }
        yield break;
    }
    public IEnumerator ek(node s, node e)
    {
        int maxFlow = 0;
        Dictionary<node, node>[] parent = new Dictionary<node, node>[d.Keys.Count];

        Queue<node> q = new Queue<node>();
        q.Enqueue(s);

        while (q.Count != 0)
        {
            node curr = q.Dequeue();

            foreach (node n in d[curr])
            {
                if (parent[new List<node>(d.Keys).IndexOf(n)] == null && n != s /*&& n.capacity > n.flow*/)
                {
                    Dictionary<node, node> l = new Dictionary<node, node>();
                    l[curr] = n;
                    parent[new List<node>(d.Keys).IndexOf(n)] = l;
                    q.Enqueue(n);
                }
            }
        }

        if (parent[new List<node>(d.Keys).IndexOf(e)] == null)
        {
            this.gameObject.GetComponent<sceneManager>().showPopupMessage("No augmenting path was found.");
            yield break;
        }

        int pushFlow = (int)Mathf.Pow(10, 10);

        for (Dictionary<node, node> ed = parent[new List<node>(d.Keys).IndexOf(e)]; ed != null; ed = parent[new List<node>(d.Keys).IndexOf(new List<node>(ed.Keys)[0])])
            pushFlow = Mathf.Min(pushFlow, /*ed.capacity - ed.flow*/ 1 - 1);

        for (Dictionary<node, node> ed = parent[new List<node>(d.Keys).IndexOf(e)]; ed != null; ed = parent[new List<node>(d.Keys).IndexOf(new List<node>(ed.Keys)[0])])
        {
            //ed.flow += pushFlow;
            //ed.reverse.flow -= pushFlow;

            Color c = new Color();
            ColorUtility.TryParseHtmlString("#C8843A", out c);

            node a = new List<node>(ed.Keys)[0];
            node b = ed[new List<node>(ed.Keys)[0]];

            a.lineRenderers[d[a].IndexOf(b)].gameObject.GetComponent<LineRenderer>().startColor = c;
            a.lineRenderers[d[a].IndexOf(b)].gameObject.GetComponent<LineRenderer>().endColor = c;
            a.GetComponent<SpriteRenderer>().color = c;
            yield return new WaitForSecondsRealtime(animTime);
            b.GetComponent<SpriteRenderer>().color = c;
        }

        maxFlow += pushFlow;
        yield break;
    }

    public IEnumerator bipartiteGraphCheck(node s)
    {
        Color c1 = new Color();
        ColorUtility.TryParseHtmlString(colors[0], out c1);
        Color c2 = new Color();
        ColorUtility.TryParseHtmlString(colors[1], out c2);

        int[] color = new int[d.Keys.Count];
        Queue<node> q = new Queue<node>();
        q.Enqueue(s);

        for (int i = 0; i < color.Length; i++)
        {
            color[i] = -1;
        }

        color[new List<node>(d.Keys).IndexOf(s)] = 1;

        while (q.Count != 0)
        {
            node n = q.Dequeue();

            yield return new WaitForSecondsRealtime(animTime);
            if (color[new List<node>(d.Keys).IndexOf(n)] == 1)
            {
                n.gameObject.GetComponent<SpriteRenderer>().color = c1;
            }
            else
            {
                n.gameObject.GetComponent<SpriteRenderer>().color = c2;
            }

            foreach (node a in d[n])
            {
                if (color[new List<node>(d.Keys).IndexOf(a)] == -1)
                {
                    color[new List<node>(d.Keys).IndexOf(a)] = 1 - color[new List<node>(d.Keys).IndexOf(n)];
                    q.Enqueue(a);
                }
                else if (color[new List<node>(d.Keys).IndexOf(a)] == color[new List<node>(d.Keys).IndexOf(n)])
                {
                    this.gameObject.GetComponent<sceneManager>().showPopupMessage("This graph is not bipartite!!");
                    yield break;
                }
            }
        }
        yield break;
    }

    //Eulerian Path Algorithm variables
    int n;
    int m = 0;
    int[] ind;
    int[] outd;
    public IEnumerator ep()
    {
        n = d.Keys.Count;
        m = 0;
        ind = new int[n];
        outd = new int[n];

        foreach (node a in d.Keys)
        {
            foreach (node b in d[a])
            {
                m++;
                outd[new List<node>(d.Keys).IndexOf(a)]++;
                ind[new List<node>(d.Keys).IndexOf(b)]++;
            }
        }

        if (!hasEulerianPath())
        {
            this.gameObject.GetComponent<sceneManager>().showPopupMessage("This graph has not an eulerian path.");
            yield break;
        }

        yield return StartCoroutine(epDFS(findStartingNode()));
        yield break;
    }

    IEnumerator epDFS(int at)
    {
        Color col = new Color();
        ColorUtility.TryParseHtmlString("#C8843A", out col);
        new List<node>(d.Keys)[at].GetComponent<SpriteRenderer>().color = col;
        yield return new WaitForSecondsRealtime(animTime);

        while (outd[at] != 0)
        {
            int outLength = d[new List<node>(d.Keys)[at]].Count;
            node c = d[new List<node>(d.Keys)[at]][outLength - (outd[at]--)];
            node a = new List<node>(d.Keys)[at];
            int nextEdge = new List<node>(d.Keys).IndexOf(c);

            a.lineRenderers[d[a].IndexOf(c)].gameObject.GetComponent<LineRenderer>().startColor = col;
            a.lineRenderers[d[a].IndexOf(c)].gameObject.GetComponent<LineRenderer>().endColor = col;

            yield return StartCoroutine(epDFS(nextEdge));
        }
        yield break;
    }

    bool hasEulerianPath()
    {
        int startNodes = 0;
        int endNodes = 0;
        for (int i = 0; i < n; i++)
        {
            if ((outd[i] - ind[i]) > 1 || (ind[i] - outd[i]) > 1)
            {
                return false;
            }
            else if ((outd[i] - ind[i]) == 1)
            {
                startNodes++;
            }
            else if ((ind[i] - outd[i]) == 1)
            {
                endNodes++;
            }
        }
        return (endNodes == 0 && startNodes == 0) || (endNodes == 1 && startNodes == 1);
    }

    int findStartingNode()
    {
        int start = 0;
        for (int i = 0; i < n; i++)
        {
            if (outd[i] - ind[i] == 1)
            {
                return i;
            }

            if (outd[i] > 0)
            {
                start = i;
            }
        }
        return start;
    }

    //SCC variables (and not only SCC, but bridges algorithm)
    int[] ids;
    int[] low;
    bool[] onStack;
    Stack<int> stack = new Stack<int>();
    int count = 0, id = 0;

    public IEnumerator findStronglyConnectedComponents()
    {
        ids = new int[d.Keys.Count];
        low = new int[d.Keys.Count];
        onStack = new bool[d.Keys.Count];

        for (int a = 0; a < d.Keys.Count; a++)
        {
            ids[a] = -1;
        }

        count = 0;
        id = 0;

        for (int i = 0; i < d.Keys.Count; i++)
        {
            if (ids[i] == -1)
            {
                Color c = new Color();
                ColorUtility.TryParseHtmlString(colors[Random.Range(0, 5)], out c);
                yield return StartCoroutine(sccDFS(i, c));
            }
        }
        yield break;
    }
    IEnumerator sccDFS(int i, Color c)
    {
        stack.Push(i);
        onStack[i] = true;
        ids[i] = id++;
        low[i] = id;

        foreach (node n in d[new List<node>(d.Keys)[i]])
        {
            int to = new List<node>(d.Keys).IndexOf(n);
            if (ids[to] == -1)
            {
                yield return StartCoroutine(sccDFS(to, c));
            }

            if (onStack[to])
            {
                low[i] = Mathf.Min(low[i], low[to]);
            }
        }

        if (ids[i] == low[i])
        {
            int node = -1000;

            while (node != (i))
            {
                node = stack.Pop();
                onStack[node] = false;
                low[node] = ids[i];
                yield return new WaitForSecondsRealtime(animTime);
                new List<node>(d.Keys)[node].gameObject.GetComponent<SpriteRenderer>().color = c;
            }
            new List<node>(d.Keys)[stack.Pop()].gameObject.GetComponent<SpriteRenderer>().color = c;

            count++;
        }
        yield break;
    }

    public IEnumerator findConnectedComponents()
    {
        int count = 0;
        for (int i = 0; i < d.Keys.Count; i++)
        {
            if (!dfsVisited[i])
            {
                count++;
                Color c = new Color();
                ColorUtility.TryParseHtmlString(colors[Random.Range(0, 5)], out c);
                node n = new List<node>(d.Keys)[i];
                yield return StartCoroutine(ccDFS(n, c));
            }
        }
        yield break;
    }

    public IEnumerator ccDFS(node s, Color c)
    {
        dfsVisited[new List<node>(d.Keys).IndexOf(s)] = true;
        s.gameObject.GetComponent<SpriteRenderer>().color = c;
        yield return new WaitForSecondsRealtime(animTime);

        foreach (node a in d[s])
        {
            if (!dfsVisited[new List<node>(d.Keys).IndexOf(a)])
            {
                yield return StartCoroutine(ccDFS(a, c));
            }
        }
        yield break;
    }

    public IEnumerator pMST(node s)
    {
        bool[] visited = new bool[d.Keys.Count];
        LinkedList<Dictionary<node, node>> q = new LinkedList<Dictionary<node, node>>();
        visited[new List<node>(d.Keys).IndexOf(s)] = true;
        foreach (node x in d[s])
        {
            if (!visited[new List<node>(d.Keys).IndexOf(x)])
            {
                Dictionary<node, node> l = new Dictionary<node, node>();
                l[s] = x;
                if (q.Count != 0)
                {
                    float d1 = Mathf.Abs(Vector3.Distance(new List<node>(q.First.Value.Keys)[0].transform.position, new List<node>(q.First.Value.Values)[0].transform.position));
                    float d2 = Mathf.Abs(Vector3.Distance(s.transform.position, x.transform.position));
                    if (d2 <= d1)
                    {
                        q.AddFirst(l);
                    }
                    else
                    {
                        q.AddLast(l);
                    }
                }
                else
                {
                    q.AddLast(l);
                }
            }
        }

        int m = d.Keys.Count - 1, edgeCount = 0;
        float mstCost = 0;

        List<Dictionary<node, node>> mstEdges = new List<Dictionary<node, node>>();
        for (int i = 0; i < m; i++)
        {
            mstEdges.Add(null);
        }

        while (q.Count != 0 && edgeCount != m)
        {
            Dictionary<node, node> edge = q.First.Value;
            q.RemoveFirst();
            node n = new List<node>(edge.Values)[0];

            if (!visited[new List<node>(d.Keys).IndexOf(n)])
            {
                mstEdges[edgeCount++] = edge;
                mstCost += 1;//Mathf.Abs(Vector3.Distance(edge.transform.position, n.transform.position));                    

                visited[new List<node>(d.Keys).IndexOf(n)] = true;
                foreach (node x in d[n])
                {
                    if (!visited[new List<node>(d.Keys).IndexOf(x)])
                    {
                        Dictionary<node, node> l = new Dictionary<node, node>();
                        l[n] = x;

                        float d1 = 0;
                        if (q.Count != 0)
                        {
                            d1 = Mathf.Abs(Vector3.Distance(new List<node>(q.First.Value.Keys)[0].transform.position, new List<node>(q.First.Value.Values)[0].transform.position));
                        }
                        float d2 = Mathf.Abs(Vector3.Distance(s.transform.position, x.transform.position));
                        if (d2 <= d1)
                        {
                            q.AddFirst(l);
                        }
                        else
                        {
                            q.AddLast(l);
                        }
                    }
                }
            }

        }

        if (edgeCount != m) { this.gameObject.GetComponent<sceneManager>().showPopupMessage("MST not exists for this node."); yield break; }

        foreach (Dictionary<node, node> u in mstEdges)
        {
            Color c = new Color();
            ColorUtility.TryParseHtmlString("#C8843A", out c);
            node a = new List<node>(u.Keys)[0];
            node b = new List<node>(u.Values)[0];
            a.lineRenderers[d[a].IndexOf(b)].gameObject.GetComponent<LineRenderer>().startColor = c;
            a.lineRenderers[d[a].IndexOf(b)].gameObject.GetComponent<LineRenderer>().endColor = c;
            a.gameObject.GetComponent<SpriteRenderer>().color = c;
            b.gameObject.GetComponent<SpriteRenderer>().color = c;
        }
    }

    public IEnumerator tSort()
    {
        bool[] visited = new bool[d.Keys.Count];
        List<node> ordering = new List<node>();

        for (int i = 0; i < d.Keys.Count; i++)
        {
            if (!visited[i])
            {
                List<node> visitedNodes = new List<node>();
                yield return StartCoroutine(tSortDFS(i, visited, visitedNodes));
                foreach (node n in visitedNodes)
                {
                    ordering.Insert(0, n);
                }
            }
        }

        string r = "[";
        for (int x = 0; x < ordering.Count; x++)
        {
            r += ordering[x].identifier.ToString() + ", ";
        }
        this.gameObject.GetComponent<sceneManager>().showPopupMessage("Top sort result: " + r.Substring(0, r.Length - 2) + "]");
        yield break;
    }

    IEnumerator tSortDFS(int i, bool[] visited, List<node> visitedNodes)
    {
        Color c = new Color();
        ColorUtility.TryParseHtmlString("#C8843A", out c);

        visited[i] = true;
        node s = new List<node>(d.Keys)[i];
        s.gameObject.GetComponent<SpriteRenderer>().color = c;
        foreach (node n in d[s])
        {
            s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().startColor = c;
            s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().endColor = c;
            yield return new WaitForSecondsRealtime(animTime);
            if (!visited[new List<node>(d.Keys).IndexOf(n)])
            {
                StartCoroutine(tSortDFS(new List<node>(d.Keys).IndexOf(n), visited, visitedNodes));
            }
        }
        visitedNodes.Add(new List<node>(d.Keys)[i]);
    }

    public IEnumerator aStar(node s, node e)
    {
        bool[] visited = new bool[d.Keys.Count];
        //In this case, dist is the g(x) function
        float[] dist = new float[d.Keys.Count];
        for (int i = 0; i < dist.Length; i++)
        {
            dist[i] = Mathf.Pow(10, 10);
        }
        dist[new List<node>(d.Keys).IndexOf(s)] = 0;

        //h is the heuristic function
        float[] h = new float[d.Keys.Count];
        for (int i = 0; i < h.Length; i++)
        {
            h[i] = Mathf.Abs(Vector3.Distance(new List<node>(d.Keys)[i].transform.position, e.transform.position));
        }

        Queue<node> queue = new Queue<node>();
        queue.Enqueue(s);
        node temp = s;

        List<node> prev = new List<node>();
        for (int a = 0; a < d.Keys.Count; a++)
        {
            prev.Add(null);
        }

        while (queue.Count != 0)
        {
            s = queue.Dequeue();
            visited[new List<node>(d.Keys).IndexOf(s)] = true;

            foreach (node n in d[s])
            {
                if (!visited[new List<node>(d.Keys).IndexOf(n)])
                {
                    float newDist = dist[new List<node>(d.Keys).IndexOf(s)] + Mathf.Abs(Vector3.Distance(s.transform.position, n.transform.position));
                    if (newDist < dist[new List<node>(d.Keys).IndexOf(n)])
                    {
                        prev[new List<node>(d.Keys).IndexOf(n)] = s;
                        dist[new List<node>(d.Keys).IndexOf(n)] = newDist;
                    }
                    queue.Enqueue(n);
                }
            }
        }

        //Reconstruct path
        List<node> path = new List<node>();
        if (dist[new List<node>(d.Keys).IndexOf(e)] != Mathf.Pow(10, 10))
        {
            for (node at = e; at != null; at = prev[new List<node>(d.Keys).IndexOf(at)])
            {
                path.Add(at);
            }

            path.Reverse();

            Color c = new Color();
            ColorUtility.TryParseHtmlString("#C8843A", out c);
            for (int i = 0; i < (path.Count - 1); i++)
            {
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().startColor = c;
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().endColor = c;
                path[i].gameObject.GetComponent<SpriteRenderer>().color = c;
                yield return new WaitForSecondsRealtime(animTime);
            }
            path[path.Count - 1].gameObject.GetComponent<SpriteRenderer>().color = c;

        }
        yield break;
    }

    public IEnumerator dijkstra(node s, node e)
    {
        bool[] visited = new bool[d.Keys.Count];
        float[] dist = new float[d.Keys.Count];
        for (int i = 0; i < dist.Length; i++)
        {
            dist[i] = Mathf.Pow(10, 10);
        }
        dist[new List<node>(d.Keys).IndexOf(s)] = 0;

        Queue<node> queue = new Queue<node>();
        queue.Enqueue(s);
        node temp = s;

        List<node> prev = new List<node>();
        for (int a = 0; a < d.Keys.Count; a++)
        {
            prev.Add(null);
        }

        while (queue.Count != 0)
        {
            s = queue.Dequeue();
            visited[new List<node>(d.Keys).IndexOf(s)] = true;

            foreach (node n in d[s])
            {
                if (!visited[new List<node>(d.Keys).IndexOf(n)])
                {
                    float newDist = dist[new List<node>(d.Keys).IndexOf(s)] + Mathf.Abs(Vector3.Distance(s.transform.position, n.transform.position));
                    if (newDist < dist[new List<node>(d.Keys).IndexOf(n)])
                    {
                        prev[new List<node>(d.Keys).IndexOf(n)] = s;
                        dist[new List<node>(d.Keys).IndexOf(n)] = newDist;
                    }
                    queue.Enqueue(n);
                }
            }
        }

        //Reconstruct path
        List<node> path = new List<node>();
        if (dist[new List<node>(d.Keys).IndexOf(e)] != Mathf.Pow(10, 10))
        {
            for (node at = e; at != null; at = prev[new List<node>(d.Keys).IndexOf(at)])
            {
                path.Add(at);
            }

            path.Reverse();

            Color c = new Color();
            ColorUtility.TryParseHtmlString("#C8843A", out c);
            for (int i = 0; i < (path.Count - 1); i++)
            {
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().startColor = c;
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().endColor = c;
                path[i].gameObject.GetComponent<SpriteRenderer>().color = c;
                yield return new WaitForSecondsRealtime(animTime);
            }
            path[path.Count - 1].gameObject.GetComponent<SpriteRenderer>().color = c;

        }
        else { this.gameObject.GetComponent<sceneManager>().showPopupMessage("There is no path to the ending node."); }
        yield break;
    }

    public IEnumerator dfs(node s)
    {
        if (dfsVisited[new List<node>(d.Keys).IndexOf(s)]) { yield break; }
        dfsVisited[new List<node>(d.Keys).IndexOf(s)] = true;
        Color c = new Color();
        ColorUtility.TryParseHtmlString("#C8843A", out c);
        s.gameObject.GetComponent<SpriteRenderer>().color = c;

        foreach (node n in d[s])
        {
            yield return new WaitForSecondsRealtime(animTime);
            if (!dfsVisited[new List<node>(d.Keys).IndexOf(n)])
            {
                s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().startColor = c;
                s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().endColor = c;
            }
            StartCoroutine(dfs(n));
        }
    }

    public IEnumerator bfs2(node s, node e)
    {
        Color c = new Color();
        ColorUtility.TryParseHtmlString("#C8843A", out c);

        node temp = s;
        Queue<node> queue = new Queue<node>();
        queue.Enqueue(s);
        bool[] visited = new bool[d.Keys.Count];
        visited[new List<node>(d.Keys).IndexOf(s)] = true;
        List<node> prev = new List<node>();
        for (int a = 0; a < d.Keys.Count; a++)
        {
            prev.Add(null);
        }

        while (queue.Count > 0)
        {
            s = queue.Dequeue();

            foreach (node n in d[s])
            {
                if (!visited[new List<node>(d.Keys).IndexOf(n)])
                {
                    queue.Enqueue(n);
                    visited[new List<node>(d.Keys).IndexOf(n)] = true;
                    prev[new List<node>(d.Keys).IndexOf(n)] = s;
                }
            }
        }

        List<node> path = new List<node>();
        for (node at = e; at != null; at = prev[new List<node>(d.Keys).IndexOf(at)])
        {
            path.Add(at);
        }

        path.Reverse();
        if (path[0] == temp)
        {
            for (int i = 0; i < (path.Count - 1); i++)
            {
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().startColor = c;
                path[i].lineRenderers[d[path[i]].IndexOf(path[i + 1])].gameObject.GetComponent<LineRenderer>().endColor = c;
                path[i].gameObject.GetComponent<SpriteRenderer>().color = c;
                yield return new WaitForSecondsRealtime(animTime);
            }
            path[path.Count - 1].gameObject.GetComponent<SpriteRenderer>().color = c;
        }
        yield break;
    }

    public IEnumerator bfs(node s)
    {
        bool[] visited = new bool[d.Keys.Count];
        visited[new List<node>(d.Keys).IndexOf(s)] = true;
        Color c = new Color();
        ColorUtility.TryParseHtmlString("#C8843A", out c);
        s.gameObject.GetComponent<SpriteRenderer>().color = c;

        Queue<node> queue = new Queue<node>();
        queue.Enqueue(s);

        while (queue.Count > 0)
        {
            s = queue.Dequeue();

            foreach (node n in d[s])
            {
                if (!visited[new List<node>(d.Keys).IndexOf(n)])
                {
                    queue.Enqueue(n);
                    visited[new List<node>(d.Keys).IndexOf(n)] = true;
                    yield return new WaitForSecondsRealtime(animTime);
                    s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().startColor = c;
                    s.lineRenderers[d[s].IndexOf(n)].gameObject.GetComponent<LineRenderer>().endColor = c;
                    n.gameObject.GetComponent<SpriteRenderer>().color = c;
                }
            }
        }

        yield break;
    }

    public void print()
    {
        string s = "";
        foreach (node key in d.Keys)
        {
            string arrayString = "";
            foreach (node n in d[key])
            {
                arrayString += n.identifier.ToString();
            }
            s += key.identifier.ToString() + " : " + arrayString + "\n";
        }
        Debug.Log(s);
    }
}