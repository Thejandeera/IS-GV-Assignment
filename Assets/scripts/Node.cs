using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool isWalkable;
    public bool isMudPath; // NEW: Helps distinguish terrain types for the graph
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    // Adjacency List to store connected neighbors (Required by your rubric!)
    public List<Node> neighbors;

    // Constructor to build a node
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