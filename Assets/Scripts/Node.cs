using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public Gem gem;
    public SpriteRenderer spriteRenderer;
    public bool active = true;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize()
    {
        spriteRenderer.sprite = gem.sprite;
        active = true;
    }

    void Update()
    {

    }


    private void OnMouseDown()
    {
        GameManager.instance.InteractNode(this);
    }

}
