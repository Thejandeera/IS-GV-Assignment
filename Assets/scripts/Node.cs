using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool isWalkable;
    public bool isMudPath;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    
    public List<Node> neighbors;

    
    public Node(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY, bool _isMudPath = false)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        isMudPath = _isMudPath;
        neighbors = new List<Node>();
    }
}