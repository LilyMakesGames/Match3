using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Vector2Int nodeBoardSize;
    public GameObject nodePrefab, boardParent;
    public Node[,] nodeBoard;
    Vector2[,] nodeWorldPositions;
    Object[] availableGems;
    Bounds cameraBounds;

    Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.right
    };
    Vector2Int[] checkMatchesDirections = new Vector2Int[]
    {
        Vector2Int.down,
        Vector2Int.right
    };

    void Start()
    {
        cameraBounds = GetCameraBounds(Camera.main);
        availableGems = Resources.LoadAll("Gems", typeof(Gem));

        nodeBoard = new Node[nodeBoardSize.x, nodeBoardSize.y];
        nodeWorldPositions = new Vector2[nodeBoardSize.x, nodeBoardSize.y];

    }

    public void CreateBoard()
    {
        for (int i = 0; i < nodeBoard.GetLength(0); i++)
        {
            for (int j = 0; j < nodeBoard.GetLength(1); j++)
            {
                GameObject currentNode = Instantiate(nodePrefab, boardParent.transform);
                nodeBoard[i, j] = currentNode.GetComponent<Node>();
                nodeBoard[i, j].gem = availableGems[Random.Range(0, availableGems.Length)] as Gem;
                currentNode.name = "(" + i + "," + j + ")";
                currentNode.transform.position = new Vector3(-cameraBounds.extents.x + (i * cameraBounds.extents.x / (nodeBoardSize.x / 2)) + cameraBounds.extents.x / nodeBoardSize.x, cameraBounds.extents.y - ((j * cameraBounds.extents.y / (nodeBoardSize.y/2)) + cameraBounds.extents.y / nodeBoardSize.y)/1.3f, 0);
                nodeWorldPositions[i, j] = currentNode.transform.position;
                nodeBoard[i, j].Initialize();
            }
        }
    }

    public void FirstReorganize()
    {
        List<List<Node>> matchList = CheckForCombination();
        while (matchList.Count > 0)
        {
            foreach (List<Node> listNodes in matchList)
            {
                ClearCombinations(listNodes);
            }
            OrganizeBoard();
            FillEmptySpaces();
            matchList = CheckForCombination();
        }

    }

    Bounds GetCameraBounds(Camera camera)
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds cameraBounds = new Bounds(camera.transform.position, new Vector3(cameraHeight * aspectRatio, cameraHeight, 0));
        return cameraBounds;
    }

    public Vector2Int GetNodeBoardPosition(Node node)
    {
        for (int i = 0; i < nodeBoard.GetLength(0); i++)
        {
            for (int j = 0; j < nodeBoard.GetLength(1); j++)
            {
                if (node == nodeBoard[i, j])
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    Node GetAdjacentNode(Vector2Int node, Vector2Int direction)
    {
        if (node.x + direction.x > nodeBoard.GetLength(0) - 1 || node.x + direction.x < 0 || node.y + direction.y > nodeBoard.GetLength(1) - 1 || node.y + direction.y < 0)
        {
            return null;
        }
        return nodeBoard[node.x + direction.x, node.y + direction.y];
    }

    int GetAdjacentNodeID(Vector2Int node, Vector2Int direction)
    {
        Node adjacentNode = GetAdjacentNode(node, direction);
        if (adjacentNode == null)
        {
            return -99;
        }
        return adjacentNode.gem.ID;
    }

    public List<Node> GetAllAdjacentNodes(Vector2Int node)
    {
        List<Node> adjacentNodes = new List<Node>();
        for (int i = 0; i < directions.Length; i++)
        {
            adjacentNodes.Add(GetAdjacentNode(node, directions[i]));
        }
        return adjacentNodes;
    }

    List<Node> GetMatches(Vector2Int node, Vector2Int direction)
    {
        List<Node> matchingNodes = new List<Node>();
        if (!nodeBoard[node.x, node.y].active)
        {
            return matchingNodes;
        }
        matchingNodes.Add(nodeBoard[node.x, node.y]);
        while (nodeBoard[node.x, node.y].gem.ID == GetAdjacentNodeID(node, direction))
        {
            node = new Vector2Int((node.x + direction.x), (node.y + direction.y));
            if (node.x + direction.x > nodeBoard.GetLength(0) || node.y + direction.y > nodeBoard.GetLength(1))
            {
                break;
            }
            matchingNodes.Add(nodeBoard[node.x, node.y]);
        }
        return matchingNodes;
    }

    public void OrganizeBoard()
    {
        for (int i = nodeBoard.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = nodeBoard.GetLength(1) - 1; j > 0; j--)
            {
                if (!nodeBoard[i, j].active)
                {
                    Node upNode = GetAdjacentNode(new Vector2Int(i, j), Vector2Int.down);
                    while (upNode != null && !upNode.active)
                    {
                        Node aux = GetAdjacentNode(GetNodeBoardPosition(upNode), Vector2Int.down);
                        if (aux != null)
                        {
                            upNode = aux;
                        }
                        else
                        {
                            break;
                        }
                    }
                    SwapNodes(upNode, nodeBoard[i, j]);
                }
            }
        }
    }

    public List<List<Node>> CheckForCombination()
    {
        List<List<Node>> listMatches = new List<List<Node>>();
        foreach (var item in nodeBoard)
        {
            for (int i = 0; i < checkMatchesDirections.Length; i++)
            {
                List<Node> nodes = GetMatches(GetNodeBoardPosition(item), checkMatchesDirections[i]);
                if (nodes.Count >= 3)
                {
                    listMatches.Add(nodes);
                }
            }
        }
        return listMatches;
    }

    public bool CheckForPossibleCombinations()
    {
        List<List<Node>> listMatches = new List<List<Node>>();
        foreach (var item in nodeBoard)
        {
            for (int i = 0; i < directions.Length - 1; i++)
            {
                List<Node> nodes = GetMatches(GetNodeBoardPosition(item), directions[i]);
                if (nodes.Count >= 2)
                {
                    List<Node> adjacentNextNode = GetAllAdjacentNodes(GetNodeBoardPosition(nodes[nodes.Count - 1]));
                    foreach (var nodeInAdjacentList in adjacentNextNode)
                    {
                        if (nodeInAdjacentList != null && nodeInAdjacentList.gem.ID == item.gem.ID && !nodes.Contains(nodeInAdjacentList))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void ClearCombinations(List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            node.active = false;
            node.spriteRenderer.sprite = null;
        }
    }

    public void ReorganizeLoop()
    {
        StartCoroutine(FallingBlocks());
    }

    public void ShuffleBoard()
    {
        List<Node> nodeList = new List<Node>();
        foreach (var node in nodeBoard)
        {
            nodeList.Add(node);
        }
        for (int i = nodeList.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i);
            Node result = nodeList[k];
            nodeList[k] = nodeList[i];
            nodeList[i] = result;
        }
        for (int i = 0; i < nodeBoard.GetLength(0); i++)
        {
            for (int j = 0; j < nodeBoard.GetLength(1); j++)
            {
                nodeBoard[i, j] = nodeList[(i * nodeBoard.GetLength(1)) + j];
                nodeBoard[i, j].transform.position = nodeWorldPositions[i, j];
            }
        }
    }

    public void FillEmptySpaces()
    {
        foreach (var item in nodeBoard)
        {
            if (!item.active)
            {
                item.gem = availableGems[Random.Range(0, availableGems.Length)] as Gem;
                item.Initialize();
            }
        }
    }

    public void SwapNodes(Node node1, Node node2)
    {
        Vector2Int node1Location = GetNodeBoardPosition(node1), node2Location = GetNodeBoardPosition(node2);

        Node aux = nodeBoard[node1Location.x, node1Location.y];
        nodeBoard[node1Location.x, node1Location.y] = nodeBoard[node2Location.x, node2Location.y];
        nodeBoard[node2Location.x, node2Location.y] = aux;

        nodeBoard[node1Location.x, node1Location.y].transform.position = nodeWorldPositions[node1Location.x, node1Location.y];
        nodeBoard[node2Location.x, node2Location.y].transform.position = nodeWorldPositions[node2Location.x, node2Location.y];
        nodeBoard[node1Location.x, node1Location.y].gameObject.name = $"({node1Location.x},{node1Location.y})";
        nodeBoard[node2Location.x, node2Location.y].gameObject.name = $"({node2Location.x},{node2Location.y})";
    }

    IEnumerator FallingBlocks()
    {
        List<List<Node>> matchList = CheckForCombination();
        if (matchList.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            GameManager.instance.comboCount++;
            foreach (List<Node> listNodes in matchList)
            {
                GameManager.instance.AddScore(listNodes.Count);
                ClearCombinations(listNodes);
            }
            GameManager.instance.PlaySound(GameManager.instance.clearSound);
            OrganizeBoard();
            FillEmptySpaces();
            StartCoroutine(FallingBlocks());
        }
        else
        {
            //GameManager.instance.StageBeat();
            GameManager.instance.comboCount = 0;
            if (!CheckForPossibleCombinations())
            {
                ShuffleBoard();
                StartCoroutine(FallingBlocks());
            }
        }
    }
}