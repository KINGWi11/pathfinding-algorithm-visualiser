namespace PathfindingAlgorithmVisualiser
{
    public static class Grid
    {
        public static bool Dragging { get; set; }
        public static NodeType? Moving { get; set; }
        public static MouseButtons? ButtonHolding { get; set; }

        public static Node? Start { get; set; }
        public static Node? Target { get; set; }
        public static Node? Diversion { get; set; }

        // This field is null until 'DrawGrid' is called, so a '?' operator is needed.
        private static Node[,]? _nodeGrid;

        public static void ClearVisualisation()
        {
            // Clears all visualisations and back colours.
            Clear(NodeType.Empty);

            if (_nodeGrid is not null)
            {
                foreach (var node in _nodeGrid)
                {
                    if (node.Type == NodeType.Dense)
                        node.MakeDense();
                }
            }
        }

        public static void DisableDenseNodes()
        {
            // Makes all dense nodes in the grid semi-transparent.
            if (_nodeGrid is not null)
            {
                foreach (var node in _nodeGrid)
                {
                    if (node.Type == NodeType.Dense)
                        node.Dim();
                }
            }
        }

        public static void Clear(NodeType nodeType)
        {
            // Resets the nodes in the grid with the node type specified by the parameter.
            if (_nodeGrid is not null)
            {
                foreach (var node in _nodeGrid)
                {
                    if (node.Type == nodeType)
                        node.Reset();
                    else if (node.PreviousNodeType == nodeType && (node.Type == NodeType.Start || node.Type == NodeType.Target || node.Type == NodeType.Diversion))
                        node.PreviousNodeType = NodeType.Empty;
                }
            }
        }

        public static void Reset()
        {
            // Clears the entire grid and resets it to its initial layout.
            if (_nodeGrid is not null)
            {
                foreach (var node in _nodeGrid)
                    node.Reset();

                Toolbar.RemoveDiversion();

                _nodeGrid[_nodeGrid.GetLength(0) / 6, _nodeGrid.GetLength(1) / 2].MakeStart();
                _nodeGrid[(_nodeGrid.GetLength(0) / 6) * 5, _nodeGrid.GetLength(1) / 2].MakeTarget();
            }
        }

        public static void DrawGrid(int cols, int rows, int nodeSize, int offsetY, Form form)
        {
            _nodeGrid = new Node[cols, rows];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    // Creates a node object at each index in the 2D array 'nodeGrid'.
                    // The y-position of each node is offset to leave space for the toolbar.
                    var node = new Node(nodeSize, i * nodeSize, j * nodeSize + offsetY, i, j, form);
                    _nodeGrid[i, j] = node;
                }
            }

            // Sets the neighbours of each node.
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Node node = _nodeGrid[i, j];

                    if (i > 0)
                        node.Neighbours.Add(_nodeGrid[i - 1, j]);

                    if (i < _nodeGrid.GetLength(0) - 1)
                        node.Neighbours.Add(_nodeGrid[i + 1, j]);

                    if (j > 0)
                        node.Neighbours.Add(_nodeGrid[i, j - 1]);

                    if (j < _nodeGrid.GetLength(1) - 1)
                        node.Neighbours.Add(_nodeGrid[i, j + 1]);
                }
            }

            // Setting the initial positions of the start and target nodes.
            _nodeGrid[cols / 6, rows / 2].MakeStart();
            _nodeGrid[(cols / 6) * 5, rows / 2].MakeTarget();
        }

        public static void RandomGeneration()
        {
            if (_nodeGrid is not null)
            {
                var rnd = new Random();

                // Loops through each node in the grid, making most empty.
                foreach (var node in _nodeGrid)
                {
                    // Random number between 0 and 9.
                    int ranNum = rnd.Next(10);

                    if (node.Type != NodeType.Start && node.Type != NodeType.Target && node.Type != NodeType.Diversion)
                    {
                        if (ranNum == 0)
                            node.MakeBlock();
                        else if (ranNum == 1)
                            node.MakeDense();
                        else
                            node.Reset();
                    }
                }
            }
        }

        public static void MazeGeneration(NodeType nodeType)
        {
            // Clears the grid first.
            Clear(NodeType.Block);
            Clear(NodeType.Dense);

            // Creates a border around the grid.
            if (_nodeGrid is not null)
            {
                for (int i = 0; i < _nodeGrid.GetLength(0); i++)
                {
                    Node topNode = _nodeGrid[i, 0];
                    Node bottomNode = _nodeGrid[i, _nodeGrid.GetLength(1) - 1];

                    if (nodeType == NodeType.Block)
                    {
                        if (topNode.Type != NodeType.Start && topNode.Type != NodeType.Target && topNode.Type != NodeType.Diversion)
                            topNode.MakeBlock();

                        if (bottomNode.Type != NodeType.Start && bottomNode.Type != NodeType.Target && bottomNode.Type != NodeType.Diversion)
                            bottomNode.MakeBlock();
                    }
                    else if (nodeType == NodeType.Dense)
                    {
                        if (topNode.Type != NodeType.Start && topNode.Type != NodeType.Target && topNode.Type != NodeType.Diversion)
                            topNode.MakeDense();

                        if (bottomNode.Type != NodeType.Start && bottomNode.Type != NodeType.Target && bottomNode.Type != NodeType.Diversion)
                            bottomNode.MakeDense();
                    }
                }

                for (int i = 0; i < _nodeGrid.GetLength(1); i++)
                {
                    Node leftNode = _nodeGrid[0, i];
                    Node rightNode = _nodeGrid[_nodeGrid.GetLength(0) - 1, i];

                    if (nodeType == NodeType.Block)
                    {
                        if (leftNode.Type != NodeType.Start && leftNode.Type != NodeType.Target && leftNode.Type != NodeType.Diversion)
                            leftNode.MakeBlock();

                        if (rightNode.Type != NodeType.Start && rightNode.Type != NodeType.Target && rightNode.Type != NodeType.Diversion)
                            rightNode.MakeBlock();
                    }
                    else if (nodeType == NodeType.Dense)
                    {
                        if (leftNode.Type != NodeType.Start && leftNode.Type != NodeType.Target && leftNode.Type != NodeType.Diversion)
                            leftNode.MakeDense();

                        if (rightNode.Type != NodeType.Start && rightNode.Type != NodeType.Target && rightNode.Type != NodeType.Diversion)
                            rightNode.MakeDense();
                    }
                }

                var rnd = new Random();
                var gaps = new List<(int X, int Y)>();

                // Starts the recursive division algorithm.
                Division(1, 1, _nodeGrid.GetLength(0) - 2, _nodeGrid.GetLength(1) - 2, true, gaps, rnd, nodeType);
            }
        }

        private static void Division(int colStart, int rowStart, int colEnd, int rowEnd, bool previousVertical, List<(int X, int Y)> gaps, Random rnd, NodeType nodeType)
        {
            // If the area is too small, it stops 'dividing'.
            if (colEnd - colStart == 0 || rowEnd - rowStart == 0 || (colEnd - colStart == 1 && rowEnd - rowStart == 1) || _nodeGrid is null)
                return;

            var possibleWalls = new List<int>();
            bool vertical;

            // Determines which orientation the new 'wall' should be.
            if (colEnd - colStart == rowEnd - rowStart)
                vertical = previousVertical;
            else if (colEnd - colStart > rowEnd - rowStart)
                vertical = true;
            else
                vertical = false;

            // Checks all possible 'wall' placements and randomly chooses one. Creates a random gap in the wall and stores its position. Calls 'Division', inputting the new areas.
            // The 'wall' created is dependent on orientation.
            if (vertical)
            {
                for (int i = colStart + 1; i < colEnd; i++)
                {
                    if (!gaps.Contains((i, rowStart - 1)) && !gaps.Contains((i, rowEnd + 1)))
                        possibleWalls.Add(i);
                }

                if (possibleWalls.Count == 0)
                    return;

                int wallX = possibleWalls[rnd.Next(possibleWalls.Count)];
                int gapY = rnd.Next(rowStart, rowEnd + 1);

                for (int i = rowStart; i <= rowEnd; i++)
                {
                    Node node = _nodeGrid[wallX, i];

                    if (node.Type != NodeType.Start && node.Type != NodeType.Target && node.Type != NodeType.Diversion)
                    {
                        if (nodeType == NodeType.Block)
                            node.MakeBlock();
                        else if (nodeType == NodeType.Dense)
                            node.MakeDense();
                    }
                }

                Node gap = _nodeGrid[wallX, gapY];
                gaps.Add((wallX, gapY));

                if (gap.Type != NodeType.Start && gap.Type != NodeType.Target && gap.Type != NodeType.Diversion)
                    gap.Reset();

                Division(colStart, rowStart, wallX - 1, rowEnd, vertical, gaps, rnd, nodeType);
                Division(wallX + 1, rowStart, colEnd, rowEnd, vertical, gaps, rnd, nodeType);
            }
            else
            {
                for (int i = rowStart + 1; i < rowEnd; i++)
                {
                    if (!gaps.Contains((colStart - 1, i)) && !gaps.Contains((colEnd + 1, i)))
                        possibleWalls.Add(i);
                }

                if (possibleWalls.Count == 0)
                    return;

                int wallY = possibleWalls[rnd.Next(possibleWalls.Count)];
                int gapX = rnd.Next(colStart, colEnd + 1);

                for (int i = colStart; i <= colEnd; i++)
                {
                    Node node = _nodeGrid[i, wallY];

                    if (node.Type != NodeType.Start && node.Type != NodeType.Target && node.Type != NodeType.Diversion)
                    {
                        if (nodeType == NodeType.Block)
                            node.MakeBlock();
                        else if (nodeType == NodeType.Dense)
                            node.MakeDense();
                    }
                }

                Node gap = _nodeGrid[gapX, wallY];
                gaps.Add((gapX, wallY));

                if (gap.Type != NodeType.Start && gap.Type != NodeType.Target && gap.Type != NodeType.Diversion)
                    gap.Reset();

                Division(colStart, rowStart, colEnd, wallY - 1, vertical, gaps, rnd, nodeType);
                Division(colStart, wallY + 1, colEnd, rowEnd, vertical, gaps, rnd, nodeType);
            }
        }
    }
}
