using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();
        
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++){
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++){
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
        /*it is worth noting that mesh point order DOES matter: */
        switch (square.configuration){
            case 0:
                break;
            //One-point config
            case 1:
                MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;
            //Two-point configs (Straight)
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            //Two-point configs (Diagonal)
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;
            //Three-point configs
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;
            //Four-point config
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
            
                
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)    CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)    CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)    CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)    CreateTriangle(points[0], points[4], points[5]);
       
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++){
            if (points[i].vertexIndex == -1){
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }
    
    /// <summary>
    /// Holds 2d Array of squares
    /// </summary>
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0); //gets the first dimension's size
            int nodeCountY = map.GetLength(1); //gets the second dimension's size
            float mapWidth = nodeCountX * squareSize; //fill appropriate dimension bounds based on size of squares
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++){
                for (int y = 0; y < nodeCountY; y++){
                    Vector3 pos = new Vector3(    -mapWidth/2 + x * squareSize + squareSize/2
                                                , 0
                                                , -mapHeight/2 + y * squareSize + squareSize/2); 
                    //positions for squares/map use all 4 quadrants
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize); 
                    //technically, every node in the map is a control node for some square
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1]; //does not attempt to start square on edge of map
            for (int x = 0; x < nodeCountX - 1; x++){
                for (int y = 0; y < nodeCountY - 1; y++){
                    squares[x, y] = new Square( controlNodes[x, y + 1],
                                                controlNodes[x + 1, y + 1],
                                                controlNodes[x + 1, y],
                                                controlNodes[x, y] ); 
                }
            }
        }
    }
    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration; //16 possible configurations (0-15)
        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerLeft = bottomLeft.above;
            centerBottom = bottomLeft.right;
            
            if (topLeft.active) configuration += 8;
            if (topRight.active) configuration += 4;
            if (bottomRight.active) configuration += 2;
            if (bottomLeft.active) configuration += 1;
        }
    }
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1; //Initially have no idea what it's going to be

        public Node(Vector3 _position)
        {
            position = _position;
        }
    }
    
    /// <summary>
    /// A "control node" is one of the four "corner" nodes in a Square - whether this is "active"
    /// or not determines the value of the square, done with binary. Configuration is determined by bits clockwise
    /// in a square based on control nodes. For example, if the bottom right and top left are active, 1010;
    /// the square is configuration 10.
    /// </summary>
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right; //these do not yet exist in grid structure

        public ControlNode(Vector3 _position, bool _active, float squareSize) : base(_position)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
    
}
