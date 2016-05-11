using UnityEngine;
using System.Collections;
using System.IO;

[ExecuteInEditMode]
public class LevelReader : MonoBehaviour {

    [Header("Level Reader")]
    public int resolution = 5;
    private int ResolutionGrid = 5;
    private int ResolutionJP = 5;
    private int ResolutionGB = 5;

    [Range(0f, 1f)]
    public float floorOfffset = 0.01f;

    public Transform start;
    public Transform end;

    public bool drawGrid = false;

    [Header("Texture Creator")]
    public bool binary = true;

    [Header("Jump Point Computer")]
    public Texture2D levelMap;
    public TextAsset jumpPointsFile;
    public bool drawJumpPoints = false;

    [Header("Goal Bound Computer")]
    public TextAsset goalBoundsFile;
    public int GB_gridNode = 0;
    public bool drawGoalBounds = false;

    private struct node
    {
        public Vector3 position;
        public int size;

        public node(Vector3 p, int s)
        {
            position = p;
            size = s;
        }
    }

    private struct primaryJPNode
    {
        public bool wall;
        public bool left;
        public bool right;
        public bool up;
        public bool down;
    }

    private struct JPGridNode
    {
        public bool wall;
        public float left;
        public float right;
        public float up;
        public float down;
        public float topLeft;
        public float topRight;
        public float bottomLeft;
        public float bottomRight;

        public JPGridNode(bool w, float l, float r, float u, float d, float tl, float tr, float bl, float br)
        {
            wall = w;
            left = l;
            right = r;
            up = u;
            down = d;
            topLeft = tl;
            topRight = tr;
            bottomLeft = bl;
            bottomRight = br;
        }
    }

    private struct GBNode
    {
        public Vector2 position;
        public string direction;
        public string goalBound;

        public GBNode(Vector2 pos, string dir, string gb)
        {
            position = pos;
            direction = dir;
            goalBound = gb;
        }
    }

    private struct GoalBound
    {
        public Vector2 gridPosition;
        public Vector2 maxLeft, minLeft, maxTopLeft, minTopLeft, maxBottomLeft, minBottomLeft, maxRight, minRight, maxTopRight, minTopRight, maxBottomRight, minBottomRight, maxUp, minUp, maxDown, minDown;

        public GoalBound(Vector2 pos, Vector2 maxL, Vector2 minL, Vector2 maxTL, Vector2 minTL, Vector2 maxBL, Vector2 minBL, Vector2 maxR, Vector2 minR, Vector2 maxTR, Vector2 minTR, Vector2 maxBR, Vector2 minBR, Vector2 maxU, Vector2 minU, Vector2 maxD, Vector2 minD)
        {
            gridPosition = pos;
            maxLeft = maxL; minLeft = minL;
            maxTopLeft = maxTL; minTopLeft = minTL;
            maxBottomLeft = maxBL; minBottomLeft = minBL;
            maxRight = maxR; minRight = minR;
            maxTopRight = maxTR; minTopRight = minTR;
            maxBottomRight = maxBR; minBottomRight = minBR;
            maxUp = maxU; minUp = minU;
            maxDown = maxD; minDown = minD;
        }
    }

    private ArrayList gridNodes = new ArrayList();
    private ArrayList primaryJP = new ArrayList();
    private ArrayList JPValues = new ArrayList();
    private ArrayList GoalBounds = new ArrayList();

    public void ReadLevel()
    {
        ResolutionGrid = resolution;

        gridNodes.Clear();

        float size = Mathf.Abs(start.position.x - end.position.x);

        for (int i = 0; i < ResolutionGrid; i++)
        {
            float z = size / 2 - (size / ResolutionGrid) / 2 - (size / ResolutionGrid) * i;

            for (int j = 0; j < ResolutionGrid; j++)
            {
                float x = -size / 2 + (size / ResolutionGrid) / 2 + (size / ResolutionGrid) * j;

                RaycastHit geometry;
                Ray readingRay = new Ray(new Vector3(x, size, z), Vector3.down);
                Physics.Raycast(readingRay, out geometry, size + 1, LayerMask.GetMask("Level", "Floor"));

                int h = Mathf.FloorToInt(geometry.point.y / (size / ResolutionGrid) - floorOfffset);
                float y = (size / ResolutionGrid)/2 + ((size / ResolutionGrid) * h);

                node newNode = new node(new Vector3(x, y, z), h);
                gridNodes.Add(newNode);
            }
        }

        Debug.Log("Grid values computed successfully!");
    }

    public void CreateLevelTexture() {

        Texture2D levelmap = new Texture2D(ResolutionGrid, ResolutionGrid, TextureFormat.RGB24, false, false);
        levelmap.filterMode = FilterMode.Point;
        Color[] colors = new Color[ResolutionGrid * ResolutionGrid];

        for (int i=0; i<ResolutionGrid; i++){
            for(int j=0; j<ResolutionGrid; j++){
                node newNode = (node)gridNodes[i * ResolutionGrid + j];

                float colorSample = binary ? (newNode.size >= 0 ? 0.0f : 1.0f) : 1f - (float)newNode.size / ResolutionGrid;

                colors[(ResolutionGrid - i - 1) * ResolutionGrid + j] = new Color(colorSample, colorSample, colorSample);
            }
        }

        levelmap.SetPixels(colors);
        levelmap.Apply();

        byte[] bytes = levelmap.EncodeToJPG();

        //File.WriteAllBytes(Application.dataPath + "/Resources/Levelmap.jpg", bytes);
        File.WriteAllBytes(Application.persistentDataPath + "/Levelmap.jpg", bytes);

        levelMap = levelmap;

        Debug.Log("Texture Created!");
    }

    private primaryJPNode[,] primaryJPSearch()
    {
        primaryJP.Clear();

        Color[] auxArrayColor = levelMap.GetPixels();
        Color[,] levelNodes = new Color[ResolutionJP, ResolutionJP];
        for (int i = 0; i < ResolutionJP; i++)
        {
            for (int j = 0; j < ResolutionJP; j++)
            {
                levelNodes[ResolutionJP - i - 1, j] = auxArrayColor[i * ResolutionJP + j];
            }
        }

        primaryJPNode[,] primaryJPs = new primaryJPNode[ResolutionJP, ResolutionJP];

        for (int i = 0; i < ResolutionJP; i++)
        {
            for (int j = 0; j < ResolutionJP; j++)
            {
                primaryJPNode node;
                node.wall = levelNodes[i, j] == Color.black ? true : false;

                if (!node.wall)
                {
                    Color left = j - 1 >= 0 ? levelNodes[i, j - 1] : Color.red;
                    Color right = j + 1 < ResolutionJP ? levelNodes[i, j + 1] : Color.red;
                    Color up = i - 1 >= 0 ? levelNodes[i - 1, j] : Color.red;
                    Color down = i + 1 < ResolutionJP ? levelNodes[i + 1, j] : Color.red;

                    Color topLeft = i - 1 >= 0 && j - 1 >= 0 ? levelNodes[i - 1, j - 1] : Color.red;
                    Color topRight = i - 1 >= 0 && j + 1 < ResolutionJP ? levelNodes[i - 1, j + 1] : Color.red;
                    Color bottomLeft = i + 1 < ResolutionJP && j - 1 >= 0 ? levelNodes[i + 1, j - 1] : Color.red;
                    Color bottomRight = i + 1 < ResolutionJP && j + 1 < ResolutionJP ? levelNodes[i + 1, j + 1] : Color.red;

                    node.left = (topLeft == Color.black && (left == Color.white && up == Color.white)) || (bottomLeft == Color.black && (left == Color.white && down == Color.white)) ? true : false;
                    node.right = (topRight == Color.black && (right == Color.white && up == Color.white)) || (bottomRight == Color.black && (right == Color.white && down == Color.white)) ? true : false;
                    node.up = (topLeft == Color.black && (left == Color.white && up == Color.white)) || (topRight == Color.black && (right == Color.white && up == Color.white)) ? true : false;
                    node.down = (bottomLeft == Color.black && (left == Color.white && down == Color.white)) || (bottomRight == Color.black && (right == Color.white && down == Color.white)) ? true : false;
                }
                else
                {
                    node.left = node.right = node.up = node.down = false;
                }

                primaryJPs[i, j] = node;
            }
        }

        return primaryJPs;
    }

    public void ComputeJumpPoints() {

        ResolutionJP = resolution;

        primaryJPNode[,] primaryJPs = primaryJPSearch();

        JPValues.Clear();
        JPGridNode[,] JPGridValues = new JPGridNode[ResolutionJP, ResolutionJP];

        //STRAIGHT
        for(int i=0; i < ResolutionJP; i++)
        {
            for(int j=0; j < ResolutionJP; j++)
            {
                JPGridValues[i, j] = new JPGridNode();
                JPGridValues[i, j].wall = primaryJPs[i, j].wall;

                if (!JPGridValues[i, j].wall)
                {
                    float leftDist, rightDist, upDist, downDist;
                    leftDist = rightDist = upDist = downDist = 0f;
                    bool leftFound, rightFound, upFound, downFound;
                    leftFound = rightFound = upFound = downFound = false;

                    //LEFT
                    int x = -1;
                    while (j + x >= 0 && !leftFound)
                    {
                        if(primaryJPs[i, j + x].wall) { JPGridValues[i, j].left = leftDist; leftFound = true; }
                        else
                        {
                            leftDist += 1;
                            if (primaryJPs[i, j + x].right) { JPGridValues[i, j].left = leftDist; leftFound = true; }
                            else x -= 1;
                        }
                    }
                    if (!leftFound || primaryJPs[i, j + x].wall) JPGridValues[i, j].left = -leftDist;

                    //RIGHT
                    x = 1;
                    while (j + x < ResolutionJP && !rightFound)
                    {
                        if (primaryJPs[i, j + x].wall) { JPGridValues[i, j].right = rightDist; rightFound = true; }
                        else
                        {
                            rightDist += 1;
                            if (primaryJPs[i, j + x].left) { JPGridValues[i, j].right = rightDist; rightFound = true; }
                            else x += 1;
                        }
                    }
                    if (!rightFound || primaryJPs[i, j + x].wall) JPGridValues[i, j].right = -rightDist;

                    //UP
                    int z = -1;
                    while (i + z >= 0 && !upFound)
                    {
                        if (primaryJPs[i + z, j].wall) { JPGridValues[i, j].up = upDist; upFound = true; }
                        else
                        {
                            upDist += 1;
                            if (primaryJPs[i + z, j].down) { JPGridValues[i, j].up = upDist; upFound = true; }
                            else z -= 1;
                        }
                    }
                    if (!upFound || primaryJPs[i + z, j].wall) JPGridValues[i, j].up = -upDist;

                    //DOWN
                    z = +1;
                    while (i + z < ResolutionJP && !downFound)
                    {
                        if (primaryJPs[i + z, j].wall) { JPGridValues[i, j].down = downDist; downFound = true; }
                        else
                        {
                            downDist += 1;
                            if (primaryJPs[i + z, j].up) { JPGridValues[i, j].down = downDist; downFound = true; }
                            else z += 1;
                        }
                    }
                    if (!downFound || primaryJPs[i + z, j].wall) JPGridValues[i, j].down = -downDist;
                }
                else
                {
                    JPGridValues[i, j] = new JPGridNode(primaryJPs[i, j].wall, 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }
        }

        for (int i = 0; i < ResolutionJP; i++)
        {
            for (int j = 0; j < ResolutionJP; j++)
            {
                if (!JPGridValues[i, j].wall)
                {
                    float topLeftDist, topRightDist, bottomLeftDist, bottomRightDist;
                    topLeftDist = topRightDist = bottomLeftDist = bottomRightDist = 0f;
                    bool topLeftFound, topRightFound, bottomLeftFound, bottomRightFound;
                    topLeftFound = topRightFound = bottomLeftFound = bottomRightFound = false;

                    //TOPLEFT
                    int x = -1;
                    int z = -1;
                    while (i + z >= 0 && j + x >= 0 && !topLeftFound)
                    {
                        if (primaryJPs[i + z, j + x].wall || primaryJPs[i, j + x].wall || primaryJPs[i + z, j].wall) { JPGridValues[i, j].topLeft = topLeftDist; topLeftFound = true; }
                        else
                        {
                            topLeftDist += 1;
                            if ((primaryJPs[i + z, j + x].right || primaryJPs[i + z, j + x].down) && !primaryJPs[i, j + x].wall && !primaryJPs[i + z, j].wall) { JPGridValues[i, j].topLeft = topLeftDist; topLeftFound = true; }
                            else if (JPGridValues[i + z, j + x].left > 0 || JPGridValues[i + z, j + x].up > 0) { JPGridValues[i, j].topLeft = topLeftDist; topLeftFound = true; }
                            else { x -= 1; z -= 1; }
                        }
                    }
                    if (!topLeftFound || primaryJPs[i + z, j + x].wall) JPGridValues[i, j].topLeft = -topLeftDist;

                    //TOPRIGHT
                    x = 1;
                    z = -1;
                    while (i + z >= 0 && j + x < ResolutionJP && !topRightFound)
                    {
                        if (primaryJPs[i + z, j + x].wall || primaryJPs[i, j + x].wall || primaryJPs[i + z, j].wall) { JPGridValues[i, j].topRight = topRightDist; topRightFound = true; }
                        else
                        {
                            topRightDist += 1;
                            if ((primaryJPs[i + z, j + x].left || primaryJPs[i + z, j + x].down) && !primaryJPs[i, j + x].wall && !primaryJPs[i + z, j].wall) { JPGridValues[i, j].topRight = topRightDist; topRightFound = true; }
                            else if (JPGridValues[i + z, j + x].right > 0 || JPGridValues[i + z, j + x].up > 0) { JPGridValues[i, j].topRight = topRightDist; topRightFound = true; }
                            else { x += 1; z -= 1; }
                        }
                    }
                    if (!topRightFound || primaryJPs[i + z, j + x].wall) JPGridValues[i, j].topRight = -topRightDist;

                    //BOTTOMLEFT
                    x = -1;
                    z = 1;
                    while (i + z < ResolutionJP && j + x >= 0 && !bottomLeftFound)
                    {
                        if (primaryJPs[i + z, j + x].wall || primaryJPs[i, j + x].wall || primaryJPs[i + z, j].wall) { JPGridValues[i, j].bottomLeft = bottomLeftDist; bottomLeftFound = true; }
                        else
                        {
                            bottomLeftDist += 1;
                            if ((primaryJPs[i + z, j + x].right || primaryJPs[i + z, j + x].up) && !primaryJPs[i, j + x].wall && !primaryJPs[i + z, j].wall) { JPGridValues[i, j].bottomLeft = bottomLeftDist; bottomLeftFound = true; }
                            else if (JPGridValues[i + z, j + x].left > 0 || JPGridValues[i + z, j + x].down > 0) { JPGridValues[i, j].bottomLeft = bottomLeftDist; bottomLeftFound = true; }
                            else { x -= 1; z += 1; }
                        }
                    }
                    if (!bottomLeftFound || primaryJPs[i + z, j + x].wall) JPGridValues[i, j].bottomLeft = -bottomLeftDist;

                    //BOTTOMRIGHT
                    x = 1;
                    z = 1;
                    while (i + z < ResolutionJP && j + x < ResolutionJP && !bottomRightFound)
                    {
                        if (primaryJPs[i + z, j + x].wall || primaryJPs[i, j + x].wall || primaryJPs[i + z, j].wall) { JPGridValues[i, j].bottomRight = bottomRightDist; bottomRightFound = true; }
                        else
                        {
                            bottomRightDist += 1;
                            if ((primaryJPs[i + z, j + x].left || primaryJPs[i + z, j + x].up) && !primaryJPs[i, j + x].wall && !primaryJPs[i + z, j].wall) { JPGridValues[i, j].bottomRight = bottomRightDist; bottomRightFound = true; }
                            else if (JPGridValues[i + z, j + x].right > 0 || JPGridValues[i + z, j + x].down > 0) { JPGridValues[i, j].bottomRight = bottomRightDist; bottomRightFound = true; }
                            else { x += 1; z += 1; }
                        }
                    }
                    if (!bottomRightFound || primaryJPs[i + z, j + x].wall) JPGridValues[i, j].bottomRight = -bottomRightDist;
                }
            }
        }

        primaryJPNode[] tempArray1 = new primaryJPNode[ResolutionJP * ResolutionJP];
        JPGridNode[] tempArray2 = new JPGridNode[ResolutionJP * ResolutionJP];

        for (int i = 0; i < ResolutionJP; i++)
        {
            for (int j = 0; j < ResolutionJP; j++)
            {
                tempArray1[i * ResolutionJP + j] = primaryJPs[i, j];
                tempArray2[i * ResolutionJP + j] = JPGridValues[i, j];
            }
        }

        primaryJP.AddRange(tempArray1);
        JPValues.AddRange(tempArray2);

        CreateJumpPointsValuesFile();

        Debug.Log("Jump Points's Values succesfully computed!");
    }

    private void CreateJumpPointsValuesFile(){
        //StreamWriter jpValues = File.CreateText(Application.dataPath + "/Resources/JPValues.txt");
        StreamWriter jpValues = File.CreateText(Application.persistentDataPath + "/JPValues.txt");

        foreach (JPGridNode node in JPValues)
        {
            jpValues.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", node.wall, node.left, node.right, node.up, node.down, node.topLeft, node.topRight, node.bottomLeft, node.bottomRight);
        }
        jpValues.Close();

        TextAsset aux = Resources.Load("JPValues") as TextAsset;
        jumpPointsFile = aux;
    }


    public void ComputeGoalBounds(){
        ResolutionGB = resolution;
        GoalBounds.Clear();

        for(int y = 0; y < ResolutionGB; y++)
        {
            for (int x = 0; x < ResolutionGB; x++)
            {
                Queue nodes = new Queue(ResolutionGB);
                JPGridNode jumpPointValues = (JPGridNode) JPValues[y * ResolutionGB + x];
                bool[,] nodeFound = new bool[ResolutionGB, ResolutionGB];

                if (!jumpPointValues.wall)
                {
                    if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(x - 1, y), "left", "left"));
                    if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(x + 1, y), "right", "right"));
                    if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(x, y - 1), "up", "up"));
                    if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(x, y + 1), "down", "down"));

                    if (jumpPointValues.topLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(x - 1, y - 1), "topLeft", "topLeft"));
                    if (jumpPointValues.bottomLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(x - 1, y + 1), "bottomLeft", "bottomLeft"));
                    if (jumpPointValues.topRight != 0 && jumpPointValues.right != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(x + 1, y - 1), "topRight", "topRight"));
                    if (jumpPointValues.bottomRight != 0 && jumpPointValues.right != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(x + 1, y + 1), "bottomRight", "bottomRight"));
                }

                Vector2 maxLeft, minLeft, maxTopLeft, minTopLeft, maxBottomLeft, minBottomLeft, maxRight, minRight, maxTopRight, minTopRight, maxBottomRight, minBottomRight, maxUp, minUp, maxDown, minDown;
                bool maxLeftInit, minLeftInit, maxTopLeftInit, minTopLeftInit, maxBottomLeftInit, minBottomLeftInit, maxRightInit, minRightInit, maxTopRightInit, minTopRightInit, maxBottomRightInit, minBottomRightInit, maxUpInit, minUpInit, maxDownInit, minDownInit;
                maxLeft = maxTopLeft = maxBottomLeft = maxRight = maxTopRight = maxBottomRight = maxUp = maxDown = minLeft = minTopLeft = minBottomLeft = minRight = minTopRight = minBottomRight = minUp = minDown = new Vector2(x, y);
                maxLeftInit = minLeftInit = maxTopLeftInit = minTopLeftInit = maxBottomLeftInit = minBottomLeftInit = maxRightInit = minRightInit = maxTopRightInit = minTopRightInit = maxBottomRightInit = minBottomRightInit = maxUpInit = minUpInit = maxDownInit = minDownInit = false;

                while (nodes.Count > 0)
                {
                    GBNode node = (GBNode)nodes.Dequeue();
                    jumpPointValues = (JPGridNode)JPValues[(int)node.position.y * ResolutionGB + (int)node.position.x];

                    if (!nodeFound[(int)node.position.y, (int)node.position.x])
                    {
                        switch (node.goalBound)
                        {
                            case "left":
                                if (!minLeftInit) { minLeft = node.position; minLeftInit = true; }
                                else minLeft = new Vector2(Mathf.Min(node.position.x, minLeft.x), Mathf.Min(node.position.y, minLeft.y));

                                if (!maxLeftInit) { maxLeft = node.position; maxLeftInit = true; }
                                maxLeft = new Vector2(Mathf.Max(node.position.x, maxLeft.x), Mathf.Max(node.position.y, maxLeft.y));
                                break;

                            case "right":
                                if (!minRightInit) { minRight = node.position; minRightInit = true; }
                                else minRight = new Vector2(Mathf.Min(node.position.x, minRight.x), Mathf.Min(node.position.y, minRight.y));

                                if (!maxRightInit) { maxRight = node.position; maxRightInit = true; }
                                maxRight = new Vector2(Mathf.Max(node.position.x, maxRight.x), Mathf.Max(node.position.y, maxRight.y));
                                break;

                            case "up":
                                if (!minUpInit) { minUp = node.position; minUpInit = true; }
                                else minUp = new Vector2(Mathf.Min(node.position.x, minUp.x), Mathf.Min(node.position.y, minUp.y));

                                if (!maxUpInit) { maxUp = node.position; maxUpInit = true; }
                                maxUp = new Vector2(Mathf.Max(node.position.x, maxUp.x), Mathf.Max(node.position.y, maxUp.y));
                                break;

                            case "down":
                                if (!minDownInit) { minDown = node.position; minDownInit = true; }
                                else minDown = new Vector2(Mathf.Min(node.position.x, minDown.x), Mathf.Min(node.position.y, minDown.y));

                                if (!maxDownInit) { maxDown = node.position; maxDownInit = true; }
                                maxDown = new Vector2(Mathf.Max(node.position.x, maxDown.x), Mathf.Max(node.position.y, maxDown.y));
                                break;

                            case "topLeft":
                                if (!minTopLeftInit) { minTopLeft = node.position; minTopLeftInit = true; }
                                else minTopLeft = new Vector2(Mathf.Min(node.position.x, minTopLeft.x), Mathf.Min(node.position.y, minTopLeft.y));

                                if (!maxTopLeftInit) { maxTopLeft = node.position; maxTopLeftInit = true; }
                                maxTopLeft = new Vector2(Mathf.Max(node.position.x, maxTopLeft.x), Mathf.Max(node.position.y, maxTopLeft.y));
                                break;

                            case "bottomLeft":
                                if (!minBottomLeftInit) { minBottomLeft = node.position; minBottomLeftInit = true; }
                                else minBottomLeft = new Vector2(Mathf.Min(node.position.x, minBottomLeft.x), Mathf.Min(node.position.y, minBottomLeft.y));

                                if (!maxBottomLeftInit) { maxBottomLeft = node.position; maxBottomLeftInit = true; }
                                maxBottomLeft = new Vector2(Mathf.Max(node.position.x, maxBottomLeft.x), Mathf.Max(node.position.y, maxBottomLeft.y));
                                break;

                            case "topRight":
                                if (!minTopRightInit) { minTopRight = node.position; minTopRightInit = true; }
                                else minTopRight = new Vector2(Mathf.Min(node.position.x, minTopRight.x), Mathf.Min(node.position.y, minTopRight.y));

                                if (!maxTopRightInit) { maxTopRight = node.position; maxTopRightInit = true; }
                                maxTopRight = new Vector2(Mathf.Max(node.position.x, maxTopRight.x), Mathf.Max(node.position.y, maxTopRight.y));
                                break;

                            case "bottomRight":
                                if (!minBottomRightInit) { minBottomRight = node.position; minBottomRightInit = true; }
                                else minBottomRight = new Vector2(Mathf.Min(node.position.x, minBottomRight.x), Mathf.Min(node.position.y, minBottomRight.y));

                                if (!maxBottomRightInit) { maxBottomRight = node.position; maxBottomRightInit = true; }
                                maxBottomRight = new Vector2(Mathf.Max(node.position.x, maxBottomRight.x), Mathf.Max(node.position.y, maxBottomRight.y));
                                break;

                        }

                        switch (node.direction)
                        {
                            case "left":
                                if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y), "left", node.goalBound));
                                if (jumpPointValues.topRight == 0 && jumpPointValues.up != 0)
                                {
                                    if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y - 1), "up", node.goalBound));
                                    if (jumpPointValues.topLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y - 1), "topLeft", node.goalBound));
                                }
                                if (jumpPointValues.bottomRight == 0 && jumpPointValues.down != 0)
                                {
                                    if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y + 1), "down", node.goalBound));
                                    if (jumpPointValues.bottomLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y + 1), "bottomLeft", node.goalBound));
                                }
                                break;

                            case "right":
                                if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y), "right", node.goalBound));
                                if (jumpPointValues.topLeft == 0 && jumpPointValues.up != 0)
                                {
                                    if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y - 1), "up", node.goalBound));
                                    if (jumpPointValues.topRight != 0 && jumpPointValues.right != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y - 1), "topRight", node.goalBound));
                                }
                                if (jumpPointValues.bottomLeft == 0 && jumpPointValues.down != 0)
                                {
                                    if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y + 1), "down", node.goalBound));
                                    if (jumpPointValues.bottomRight != 0 && jumpPointValues.right != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y + 1), "bottomRight", node.goalBound));
                                }
                                break;

                            case "up":
                                if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y - 1), "up", node.goalBound));
                                if (jumpPointValues.bottomRight == 0 && jumpPointValues.right != 0)
                                {
                                    if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y), "right", node.goalBound));
                                    if (jumpPointValues.topRight != 0 && jumpPointValues.right != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y - 1), "topRight", node.goalBound));
                                }
                                if (jumpPointValues.bottomLeft == 0 && jumpPointValues.left != 0)
                                {
                                    if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y), "left", node.goalBound));
                                    if (jumpPointValues.topLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y - 1), "topLeft", node.goalBound));
                                }
                                break;

                            case "down":
                                if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y + 1), "down", node.goalBound));
                                if (jumpPointValues.topLeft == 0 && jumpPointValues.left != 0)
                                {
                                    if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y), "left", node.goalBound));
                                    if (jumpPointValues.bottomLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y + 1), "bottomLeft", node.goalBound));
                                }
                                if (jumpPointValues.topRight == 0 && jumpPointValues.right != 0)
                                {
                                    if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y), "right", node.goalBound));
                                    if (jumpPointValues.bottomRight != 0 && jumpPointValues.right != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y + 1), "bottomRight", node.goalBound));
                                }
                                break;

                            case "topLeft":
                                if (jumpPointValues.topLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y - 1), "topLeft", node.goalBound));
                                if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y), "left", node.goalBound));
                                if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y - 1), "up", node.goalBound));
                                break;

                            case "bottomLeft":
                                if (jumpPointValues.bottomLeft != 0 && jumpPointValues.left != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y + 1), "bottomLeft", node.goalBound));
                                if (jumpPointValues.left != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x - 1, node.position.y), "left", node.goalBound));
                                if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y + 1), "down", node.goalBound));
                                break;

                            case "topRight":
                                if (jumpPointValues.topRight != 0 && jumpPointValues.right != 0 && jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y - 1), "topRight", node.goalBound));
                                if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y), "right", node.goalBound));
                                if (jumpPointValues.up != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y - 1), "up", node.goalBound));
                                break;

                            case "bottomRight":
                                if (jumpPointValues.bottomRight != 0 && jumpPointValues.right != 0 && jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y + 1), "bottomRight", node.goalBound));
                                if (jumpPointValues.right != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x + 1, node.position.y), "right", node.goalBound));
                                if (jumpPointValues.down != 0) nodes.Enqueue(new GBNode(new Vector2(node.position.x, node.position.y + 1), "down", node.goalBound));
                                break;
                        }

                        nodeFound[(int)node.position.y, (int)node.position.x] = true;
                    }

                }

                GoalBounds.Add(new GoalBound(new Vector2(x, y), maxLeft, minLeft, maxTopLeft, minTopLeft, maxBottomLeft, minBottomLeft, maxRight, minRight, maxTopRight, minTopRight, maxBottomRight, minBottomRight, maxUp, minUp, maxDown, minDown));

            }
        }

        CreateGoalBoundsValuesFile();

        Debug.Log("Goal Bounds Calculated!");
    }

    private void CreateGoalBoundsValuesFile()
    {
        //StreamWriter gbValues = File.CreateText(Application.dataPath + "/Resources/GBValues.txt");
        StreamWriter gbValues = File.CreateText(Application.persistentDataPath + "/GBValues.txt");

        foreach (GoalBound gb in GoalBounds)
        {
            gbValues.WriteLine("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/{9}/{10}/{11}/{12}/{13}/{14}/{15}/{16}", gb.gridPosition, gb.maxLeft, gb.minLeft, gb.maxTopLeft, gb.minTopLeft, gb.maxBottomLeft, gb.minBottomLeft, gb.maxRight, gb.minRight, gb.maxTopRight, gb.minTopRight, gb.maxBottomRight, gb.minBottomRight, gb.maxUp, gb.minUp, gb.maxDown, gb.minDown);
        }
        gbValues.Close();

        TextAsset aux = Resources.Load("GBValues") as TextAsset;
        goalBoundsFile = aux;
    }

    void OnDrawGizmos()
    {
        if (drawGrid) {
            float size = Mathf.Abs(start.position.x - end.position.x);

            Gizmos.color = Color.blue;

            for (int i = 0; i <= resolution; i++)
            {
                float x = -size / 2 + (size / resolution) * i;
                float y = size / 2;
                float z = 0;

                Gizmos.DrawWireCube(GetComponent<Transform>().position + new Vector3(x, y, z), new Vector3(0.01f, size, size));

                x = 0;
                y = size / 2;
                z = -size / 2 + (size / resolution) * i;

                Gizmos.DrawWireCube(GetComponent<Transform>().position + new Vector3(x, y, z), new Vector3(size, size, 0.01f));

                x = 0;
                y = size / 2 + -size / 2 + (size / resolution) * i;
                z = 0;

                Gizmos.DrawWireCube(GetComponent<Transform>().position + new Vector3(x, y, z), new Vector3(size, 0.01f, size));
            }

            Gizmos.color = Color.red;

            foreach (node n in gridNodes)
            {
                if(n.size >= 0)
                    Gizmos.DrawWireCube(GetComponent<Transform>().position + n.position, new Vector3(size / ResolutionGrid, size / ResolutionGrid, size / ResolutionGrid));
            }
        }

        if (drawJumpPoints /*&& jumpPointsFile!=null*/){
            float size = Mathf.Abs(start.position.x - end.position.x);

            if(JPValues.Count == 0)
            {
                primaryJPNode[,] primaryJPs = primaryJPSearch();

                primaryJPNode[] tempArray1 = new primaryJPNode[ResolutionJP * ResolutionJP];

                for (int i = 0; i < ResolutionJP; i++)
                {
                    for (int j = 0; j < ResolutionJP; j++)
                    {
                        tempArray1[i * ResolutionJP + j] = primaryJPs[i, j];
                    }
                }

                primaryJP.AddRange(tempArray1);

                //StreamReader sr = File.OpenText(Application.dataPath + "/Resources/JPValues.txt");
                StreamReader sr = File.OpenText(Application.persistentDataPath + "/JPValues.txt");

                string nodeInfo;

                for (int i = 0; i < ResolutionJP; i++)
                {
                    for (int j = 0; j < ResolutionJP; j++)
                    {
                        nodeInfo = sr.ReadLine();

                        JPGridNode node;
                        string[] aux = nodeInfo.Split(',');

                        node.wall = bool.Parse(aux[0]);
                        node.left = int.Parse(aux[1]);
                        node.right = int.Parse(aux[2]);
                        node.up = int.Parse(aux[3]);
                        node.down = int.Parse(aux[4]);
                        node.topLeft = int.Parse(aux[5]);
                        node.topRight = int.Parse(aux[6]);
                        node.bottomLeft = int.Parse(aux[7]);
                        node.bottomRight = int.Parse(aux[8]);

                        JPValues.Add(node);

                    }
                }
            }

            for(int i=0; i < ResolutionJP * ResolutionJP; i++){
                float x = -size / 2 + (size / ResolutionJP) / 2 + (size / ResolutionJP) * (i % ResolutionJP);
                float y = (size / ResolutionJP) / 2;
                float z = size / 2 - (size / ResolutionJP) / 2 - (size / ResolutionJP) * Mathf.FloorToInt(i / ResolutionJP);

                Vector3 position = new Vector3(x, y, z);

                primaryJPNode jp = (primaryJPNode) primaryJP[i];
                Gizmos.color = jp.down || jp.up || jp.right || jp.left && !jp.wall ? Color.grey : Color.clear;
                Gizmos.DrawSphere(GetComponent<Transform>().position + position, (size / ResolutionJP) / 4);

                JPGridNode node = (JPGridNode)JPValues[i];

                Gizmos.color = node.left > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(-5f/ResolutionJP, 0, 0));
                Gizmos.color = node.right > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(5f/ResolutionJP, 0, 0));
                Gizmos.color = node.up > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(0, 0, 5f / ResolutionJP));
                Gizmos.color = node.up > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(0, 0, -5f / ResolutionJP));

                Gizmos.color = node.topLeft > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(-5f / ResolutionJP, 0, 5f / ResolutionJP));
                Gizmos.color = node.topRight > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(5f / ResolutionJP, 0, 5f / ResolutionJP));
                Gizmos.color = node.bottomLeft > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(-5f / ResolutionJP, 0, -5f / ResolutionJP));
                Gizmos.color = node.bottomRight > 0 ? Color.black : Color.clear;
                Gizmos.DrawLine(GetComponent<Transform>().position + position, GetComponent<Transform>().position + position + new Vector3(5f / ResolutionJP, 0, -5f / ResolutionJP));
            }
        }

        if (drawGoalBounds/* && goalBoundsFile != null*/)
        {
            float size = Mathf.Abs(start.position.x - end.position.x);

            if(GoalBounds.Count == 0)
            {
                ResolutionGB = resolution;
                GoalBounds.Clear();

                //StreamReader sr = File.OpenText(Application.dataPath + "/Resources/GBValues.txt");
                StreamReader sr = File.OpenText(Application.persistentDataPath + "/GBValues.txt");
                string goalbound;

                for (int i=0; i < ResolutionGB; i++)
                {
                    for (int j = 0; j < ResolutionGB; j++)
                    {
                        goalbound = sr.ReadLine();

                        Vector2[] gbValues = new Vector2[17];

                        string[] aux = goalbound.Split('/');

                        for (int k = 0; k < 17; k++) {
                            int startInd = aux[k].IndexOf("(") + 1; ;
                            float aXPosition = float.Parse(aux[k].Substring(startInd, aux[k].IndexOf(",") - startInd));
                            startInd = aux[k].IndexOf(",") + 1;
                            float aYPosition = float.Parse(aux[k].Substring(startInd, aux[k].IndexOf(")") - startInd));

                            gbValues[k] = new Vector2(aXPosition, aYPosition);
                        }

                        GoalBounds.Add(new GoalBound(gbValues[0], gbValues[1], gbValues[2], gbValues[3], gbValues[4], gbValues[5], gbValues[6], gbValues[7], gbValues[8], gbValues[9], gbValues[10], gbValues[11], gbValues[12], gbValues[13], gbValues[14], gbValues[15], gbValues[16]));
                    }
                }   
            }

            if (GB_gridNode < 0) GB_gridNode = ResolutionGB*ResolutionGB - 1;
            else if (GB_gridNode >= ResolutionGB * ResolutionGB) GB_gridNode = 0;

            GoalBound gb = (GoalBound)GoalBounds[GB_gridNode];

            float width = (size / ResolutionGB) * ((gb.maxLeft.x + 1)- gb.minLeft.x);
            float height = (size / ResolutionGB) * ((gb.maxLeft.y + 1) - gb.minLeft.y);
            Vector3 position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minLeft.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minLeft.y - height / 2);
            Gizmos.color = Color.red * new Color(1f,1f,1f,0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxTopLeft.x + 1) - gb.minTopLeft.x);
            height = (size / ResolutionGB) * ((gb.maxTopLeft.y + 1) - gb.minTopLeft.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minTopLeft.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minTopLeft.y - height / 2);
            Gizmos.color = Color.magenta * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxUp.x + 1) - gb.minUp.x);
            height = (size / ResolutionGB) * ((gb.maxUp.y + 1) - gb.minUp.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minUp.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minUp.y - height / 2);
            Gizmos.color = Color.white * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxTopRight.x + 1) - gb.minTopRight.x);
            height = (size / ResolutionGB) * ((gb.maxTopRight.y + 1) - gb.minTopRight.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minTopRight.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minTopRight.y - height / 2);
            Gizmos.color = Color.cyan * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxRight.x + 1) - gb.minRight.x);
            height = (size / ResolutionGB) * ((gb.maxRight.y + 1) - gb.minRight.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minRight.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minRight.y - height / 2);
            Gizmos.color = Color.blue * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxBottomRight.x + 1) - gb.minBottomRight.x);
            height = (size / ResolutionGB) * ((gb.maxBottomRight.y + 1) - gb.minBottomRight.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minBottomRight.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minBottomRight.y - height / 2);
            Gizmos.color = Color.gray * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxDown.x + 1) - gb.minDown.x);
            height = (size / ResolutionGB) * ((gb.maxDown.y + 1) - gb.minDown.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minDown.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minDown.y - height / 2);
            Gizmos.color = Color.black * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));

            width = (size / ResolutionGB) * ((gb.maxBottomLeft.x + 1) - gb.minBottomLeft.x);
            height = (size / ResolutionGB) * ((gb.maxBottomLeft.y + 1) - gb.minBottomLeft.y);
            position = new Vector3(-size / 2 + (size / ResolutionGB) * gb.minBottomLeft.x + width / 2, 10f, size / 2 - (size / ResolutionGB) * gb.minBottomLeft.y - height / 2);
            Gizmos.color = Color.yellow * new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(GetComponent<Transform>().position + position, new Vector3(width, 0.1f, height));
        }
    }
}