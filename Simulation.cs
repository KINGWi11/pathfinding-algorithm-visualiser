namespace PathfindingAlgorithmVisualiser
{
    using System.Timers;
    using System.Xml.Linq;

    public static class Simulation
    {
        public static bool Running { get; private set; }
        public static bool Paused { get; private set; }
        public static bool Visualised { get; private set; }
        public static int Speed { get; private set; }

        private static bool _pathFound;

        private static readonly List<(Node Node, string Action)> _steps = new();
        private static readonly List<Node> _path = new();
        private static readonly Timer _timer = new();

        static Simulation()
        {
            _timer.Elapsed += Timer_Elapsed;
        }

        public static void Start()
        {
            Running = true;
            Paused = false;
            Grid.Dragging = false;
            Grid.Moving = null;

            // Runs an algorithm then visualises its processes.
            RunAlgorithms();

            if (Speed == 6)
                InstantVisualise();
            else
                _timer.Start();
        }

        public static void Stop()
        {
            _timer.Stop();

            // Resetting variables.
            _path.Clear();
            _steps.Clear();

            Visualised = false;
            Grid.ClearVisualisation();
            Running = false;
        }

        public static void Pause()
        {
            _timer.Stop();
            Paused = true;
        }

        public static void Resume()
        {
            _timer.Start();
            Paused = false;
        }

        public static void InstantRecalculation()
        {
            // Instantly shows new visuals and shortest path.
            Grid.ClearVisualisation();

            _path.Clear();
            _steps.Clear();

            RunAlgorithms();
            InstantVisualise();
        }

        public static void ChangeSpeed(int speed)
        {
            // Timer interval is changed depending on the selected speed.
            switch (speed)
            {
                case 1:
                    Speed = 1;
                    _timer.Interval = 1000;
                    break;
                case 2:
                    Speed = 2;
                    _timer.Interval = 100;
                    break;
                case 3:
                    Speed = 3;
                    _timer.Interval = 50;
                    break;
                case 4:
                    Speed = 4;
                    _timer.Interval = 10;
                    break;
                case 5:
                    Speed = 5;
                    _timer.Interval = 1;
                    break;
                case 6:
                    Speed = 6;

                    if (Running)
                    {
                        _timer.Stop();
                        InstantVisualise();
                    }
                    break;
            }
        }

        private static void AddPath(Node start, Node end)
        {
            Node node = end.Parent;

            // Backtracks and stores the shortest path.
            while (!ReferenceEquals(node, start))
            {
                if (node.Type != NodeType.Start && node.Type != NodeType.Target && node.Type != NodeType.Diversion)
                    _path.Add(node);

                node = node.Parent;
            }
        }

        private static void RunAlgorithms()
        {
            bool firstPathFound = false;
            bool? secondPathFound = null;

            // Calls the necessary pathfinding algorithms and stores their results.
            if (Grid.Start is not null && Grid.Target is not null)
            {
                if (Toolbar.SelectedAlgorithm == Algorithm.Dijkstra)
                {
                    if (Grid.Diversion is null)
                    {
                        firstPathFound = Dijkstra(Grid.Start, Grid.Target);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Target);
                    }
                    else
                    {
                        firstPathFound = Dijkstra(Grid.Start, Grid.Diversion);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Diversion);

                        secondPathFound = Dijkstra(Grid.Diversion, Grid.Target);
                        if (secondPathFound == true)
                            AddPath(Grid.Diversion, Grid.Target);
                    }
                }
                else if (Toolbar.SelectedAlgorithm == Algorithm.AStar)
                {
                    if (Grid.Diversion is null)
                    {
                        firstPathFound = AStar(Grid.Start, Grid.Target);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Target);
                    }
                    else
                    {
                        firstPathFound = AStar(Grid.Start, Grid.Diversion);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Diversion);

                        secondPathFound = AStar(Grid.Diversion, Grid.Target);
                        if (secondPathFound == true)
                            AddPath(Grid.Diversion, Grid.Target);
                    }
                }
                else if (Toolbar.SelectedAlgorithm == Algorithm.BFS)
                {
                    Grid.DisableDenseNodes();

                    if (Grid.Diversion is null)
                    {
                        firstPathFound = BFS(Grid.Start, Grid.Target);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Target);
                    }
                    else
                    {
                        firstPathFound = BFS(Grid.Start, Grid.Diversion);
                        if (firstPathFound)
                            AddPath(Grid.Start, Grid.Diversion);

                        secondPathFound = BFS(Grid.Diversion, Grid.Target);
                        if (secondPathFound == true)
                            AddPath(Grid.Diversion, Grid.Target);
                    }
                }
            }

            // The path should only be drawn if the whole path is found.
            if (firstPathFound && (secondPathFound is null || secondPathFound == true))
                _pathFound = true;
            else
                _pathFound = false;
        }

        private static int Heuristic(Node current, Node end)
        {
            return Math.Abs(current.Column - end.Column) + Math.Abs(current.Row - end.Row);
        }

        private static bool Dijkstra(Node start, Node end)
        {
            // Nodes to be evaluated.
            var open = new List<Node>();
            // Nodes evaluated.
            var closed = new List<Node>();

            start.GCost = 0;
            open.Add(start);

            // Node in 'open' with the lowest g-cost is evaluated next.
            while (open.Count > 0)
            {
                Node current = open[0];

                foreach (var node in open)
                {
                    if (node.GCost < current.GCost)
                        current = node;
                }

                // Removes it from 'open' and adds it to the 'closed' set.
                open.Remove(current);
                closed.Add(current);
                _steps.Add((current, "close"));

                // Path found if this node is the 'end node'.
                if (ReferenceEquals(current, end))
                    return true;

                // Loops through the neighbours of the node.
                foreach (var neighbour in current.Neighbours)
                {
                    // Skips the neighbour if it's not 'traversable' or has already been evaluated.
                    if (neighbour.Type == NodeType.Block || closed.Contains(neighbour))
                        continue;

                    // If a neighbour is not in the open set, or there is a shorter path to it, it's properties are updated.
                    if (!open.Contains(neighbour) || current.GCost + neighbour.AdjacentCost < neighbour.GCost)
                    {
                        neighbour.GCost = current.GCost + neighbour.AdjacentCost;
                        neighbour.Parent = current;

                        // If a neighbour is not in the open set, it's added.
                        if (!open.Contains(neighbour))
                        {
                            open.Add(neighbour);
                            _steps.Add((neighbour, "open"));
                        }
                    }
                }
            }

            return false;
        }

        private static bool AStar(Node start, Node end)
        {
            // Nodes to be evaluated.
            var open = new List<Node>();
            // Nodes evaluated.
            var closed = new List<Node>();

            start.GCost = 0;
            open.Add(start);

            // Node in 'open' with the lowest f-cost (g-cost + h-cost) is evaluated next.
            // If the f-cost is the same, the node with the lowest h-cost is prioritised.
            while (open.Count > 0)
            {
                Node current = open[0];

                foreach (var node in open)
                {
                    int currentFCost = current.GCost + Heuristic(current, end);
                    int nodeFCost = node.GCost + Heuristic(node, end);

                    if (nodeFCost < currentFCost)
                        current = node;
                    else if (nodeFCost == currentFCost && Heuristic(node, end) < Heuristic(current, end))
                        current = node;
                }

                // Removes it from 'open' and adds it to the 'closed' set.
                open.Remove(current);
                closed.Add(current);
                _steps.Add((current, "close"));

                // Path found if this node is the 'end node'.
                if (ReferenceEquals(current, end))
                    return true;

                // Loops through the neighbours of the node.
                foreach (var neighbour in current.Neighbours)
                {
                    // Skips the neighbour if it's not 'traversable' or has already been evaluated.
                    if (neighbour.Type == NodeType.Block || closed.Contains(neighbour))
                        continue;

                    // If a neighbour is not in the open set, or there is a shorter path to it, it's properties are updated.
                    if (!open.Contains(neighbour) || current.GCost + neighbour.AdjacentCost < neighbour.GCost)
                    {
                        neighbour.GCost = current.GCost + neighbour.AdjacentCost;
                        neighbour.Parent = current;

                        // If a neighbour is not in the open set, it's added.
                        if (!open.Contains(neighbour))
                        {
                            open.Add(neighbour);
                            _steps.Add((neighbour, "open"));
                        }
                    }
                }
            }

            return false;
        }

        private static bool BFS(Node start, Node end)
        {
            // Queue of nodes to evaluate next.
            var toCheck = new Queue<Node>();
            // List of all visited nodes.
            var visited = new List<Node>();

            toCheck.Enqueue(start);
            visited.Add(start);

            while (toCheck.Count > 0)
            {
                // Next node in the queue to evaluate.
                Node current = toCheck.Dequeue();

                // If it is the end node, the path is found.
                if (ReferenceEquals(current, end))
                    return true;

                // Loops through the neighbours of the node.
                foreach (var neighbour in current.Neighbours)
                {
                    // If the neighbour is 'traversable' and hasn't been 'visited', it's added to the queue.
                    if (neighbour.Type != NodeType.Block && !visited.Contains(neighbour))
                    {
                        toCheck.Enqueue(neighbour);
                        visited.Add(neighbour);
                        neighbour.Parent = current;
                        _steps.Add((neighbour, "close"));
                    }
                }
            }

            return false;
        }

        private static void DrawPath()
        {
            // Draws the path.
            foreach (var node in _path)
                node.Path();
        }

        private static void InstantVisualise()
        {
            // Instantly displays visualisation results.
            foreach (var step in _steps)
            {
                if (step.Node.Type != NodeType.Start && step.Node.Type != NodeType.Target && step.Node.Type != NodeType.Diversion)
                {
                    if (step.Action == "close")
                        step.Node.Close();
                    else
                        step.Node.Open();

                    if (Toolbar.SelectedAlgorithm != Algorithm.BFS && step.Node.Type == NodeType.Dense)
                        step.Node.Dim();
                }
            }

            if (_pathFound)
                DrawPath();

            Visualised = true;
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (_steps.Count == 0)
            {
                // Draws the path and stops the timer if all the steps have been shown.
                if (_pathFound)
                    DrawPath();

                _timer.Stop();
                Visualised = true;
            }
            else
            {
                // 'Visualises' the next algorithmic step.
                Node node = _steps[0].Node;

                if (node.Type != NodeType.Start && node.Type != NodeType.Target && node.Type != NodeType.Diversion)
                {
                    if (_steps[0].Action == "close")
                        node.Close();
                    else
                        node.Open();

                    if (Toolbar.SelectedAlgorithm != Algorithm.BFS && node.Type == NodeType.Dense)
                        node.Dim();
                }

                _steps.RemoveAt(0);
            }
        }
    }
}
