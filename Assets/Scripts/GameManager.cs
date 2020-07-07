using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        GameStart,
        Idle,
        Select,
        CheckEndGame,
        GameEnd
    }

    public static GameManager instance;

    public Board nodeBoard;

    public GameState gameState;
    Node currentSelectedNode;

    public int comboCount;
    int score = 0;
    int scoreToBeat;

    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    TextMeshProUGUI scoreToBeatText;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        gameState = GameState.Idle;
    }

    public void Start()
    {
        UpdateScore();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(nodeBoard.CheckForPossibleCombinations());
        }
    }

    public void InteractNode(Node node)
    {
        switch (gameState)
        {
            case GameState.Idle:
                gameState = GameState.Select;
                node.spriteRenderer.color = Color.gray;
                currentSelectedNode = node;
                break;
            case GameState.Select:
                if (nodeBoard.GetAllAdjacentNodes(nodeBoard.GetNodeBoardPosition(currentSelectedNode)).Contains(node))
                {
                    nodeBoard.SwapNodes(node, currentSelectedNode);
                    currentSelectedNode.spriteRenderer.color = Color.white;
                    if (nodeBoard.CheckForCombination().Count > 0)
                    {
                        nodeBoard.ReorganizeLoop();
                    }
                    else
                    {
                        nodeBoard.SwapNodes(node, currentSelectedNode);
                    }
                    gameState = GameState.Idle;
                    currentSelectedNode = null;
                }
                else
                {
                    currentSelectedNode.spriteRenderer.color = Color.white;
                    node.spriteRenderer.color = Color.gray;
                    currentSelectedNode = node;
                }
                break;
        }
    }

    public void AddScore(int nodesDeleted)
    {
        score += nodesDeleted * comboCount * 100;
        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.text = $"Score: {score}";
    }


}
