using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        CheckingForReshuffle,
        Idle,
        Select,
        Matching,
        Reorganize
    }

    public static GameManager instance;

    public Board nodeBoard;

    public GameState gameState;
    Node currentSelectedNode;

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            nodeBoard.ShuffleBoard();
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
                    gameState = GameState.Idle;
                    currentSelectedNode = null;
                    nodeBoard.ReorganizeLoop();
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

}
