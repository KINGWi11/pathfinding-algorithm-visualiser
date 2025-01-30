namespace PathfindingAlgorithmVisualiser
{
    public class Node
    {
        public NodeType Type { get; private set; }
        public NodeType PreviousNodeType { get; set; }

        public List<Node> Neighbours { get; set; }
        public Node Parent { get; set; }

        public int Column { get; private set; }
        public int Row { get; private set; }
        public int AdjacentCost { get; private set; }
        public int GCost { get; set; }

        private readonly Label _lbl = new();

        public Node(int size, int x, int y, int col, int row, Form form)
        {
            Type = NodeType.Empty;
            Neighbours = new List<Node>();
            AdjacentCost = 1;
            Column = col;
            Row = row;
            Parent = this;

            CreateLbl(size, x, y, form);
        }

        public void Open()
        {
            _lbl.BackColor = Color.GreenYellow;
        }

        public void Close()
        {
            _lbl.BackColor = Color.Yellow;
        }

        public void Path()
        {
            _lbl.BackColor = Color.Violet;
        }

        public void Dim()
        {
            // Makes label semi-transparent.
            _lbl.BackColor = Color.FromArgb(50, _lbl.BackColor);
        }

        public void Reset()
        {
            Type = NodeType.Empty;
            AdjacentCost = 1;

            _lbl.BackColor = Color.WhiteSmoke;
        }

        public void MakeStart()
        {
            if (Grid.Start is not null)
            {
                if (Grid.Moving is not null && Grid.Start.PreviousNodeType == NodeType.Block)
                    Grid.Start.MakeBlock();
                else if (Grid.Moving is not null && Grid.Start.PreviousNodeType == NodeType.Dense)
                    Grid.Start.MakeDense();
                else
                    Grid.Start.Reset();
            }

            PreviousNodeType = Type;
            Type = NodeType.Start;

            _lbl.BackColor = Color.Green;

            Grid.Start = this;
        }

        public void MakeTarget()
        {
            if (Grid.Target is not null)
            {
                if (Grid.Moving is not null && Grid.Target.PreviousNodeType == NodeType.Block)
                    Grid.Target.MakeBlock();
                else if (Grid.Moving is not null && Grid.Target.PreviousNodeType == NodeType.Dense)
                    Grid.Target.MakeDense();
                else
                    Grid.Target.Reset();
            }

            PreviousNodeType = Type;
            Type = NodeType.Target;

            _lbl.BackColor = Color.Red;

            Grid.Target = this;
        }

        public void MakeBlock()
        {
            Type = NodeType.Block;
            _lbl.BackColor = Color.Black;
        }

        public void MakeDense()
        {
            Type = NodeType.Dense;
            AdjacentCost = 15;

            _lbl.BackColor = Color.SaddleBrown;
        }

        private void MakeDiversion()
        {
            if (Grid.Diversion is not null)
            {
                if (Grid.Moving is not null && Grid.Diversion.PreviousNodeType == NodeType.Block)
                    Grid.Diversion.MakeBlock();
                else if (Grid.Moving is not null && Grid.Diversion.PreviousNodeType == NodeType.Dense)
                    Grid.Diversion.MakeDense();
                else
                    Grid.Diversion.Reset();
            }

            PreviousNodeType = Type;
            Type = NodeType.Diversion;

            _lbl.BackColor = Color.Blue;

            Grid.Diversion = this;
        }

        private void CreateLbl(int size, int x, int y, Form form)
        {
            // Nodes should be square, so label height and width are the same.
            _lbl.Size = new Size(size, size);
            _lbl.Location = new Point(x, y);
            _lbl.BorderStyle = BorderStyle.FixedSingle;
            _lbl.BackColor = Color.WhiteSmoke;

            // Binding label events and handlers.
            _lbl.MouseDown += Lbl_MouseDown;
            _lbl.MouseEnter += Lbl_MouseEnter;
            _lbl.MouseUp += Lbl_MouseUp;

            // Adds the label to the form so that it's visible to the user. 
            form.Controls.Add(_lbl);
        }

        private void Lbl_MouseUp(object? sender, MouseEventArgs e)
        {
            // User has stopped dragging.
            Grid.Dragging = false;
            Grid.ButtonHolding = null;
            Grid.Moving = null;
        }

        private void Lbl_MouseEnter(object? sender, EventArgs e)
        {
            // The label will only be updated if the user is 'dragging' and not a 'core' node.
            if (Grid.Dragging && Type != NodeType.Start && Type != NodeType.Target && Type != NodeType.Diversion)
            {
                if (Grid.Moving == NodeType.Start)
                    MakeStart();
                else if (Grid.Moving == NodeType.Target)
                    MakeTarget();
                else if (Grid.Moving == NodeType.Diversion)
                    MakeDiversion();
                else
                    UpdateLbl(Grid.ButtonHolding);

                if (Simulation.Visualised)
                    Simulation.InstantRecalculation();
            }
        }

        private void Lbl_MouseDown(object? sender, MouseEventArgs e)
        {
            if (Simulation.Running && !Simulation.Visualised)
                return;

            // This makes it so that other labels can listen for 'MouseEnter' event while the mouse is still pressed.
            if (sender is Control control)
                control.Capture = false;

            // Determines if the user is moving or adding a node.
            if (Type == NodeType.Start || Type == NodeType.Target || Type == NodeType.Diversion)
            {
                if (e.Button == MouseButtons.Left)
                    Grid.Moving = Type;
            }
            else if (!Simulation.Running)
            {
                UpdateLbl(e.Button);
                Grid.ButtonHolding = e.Button;
            }

            // Initiates the dragging process.
            Grid.Dragging = true;
        }

        private void UpdateLbl(MouseButtons? button)
        {
            // Edits node based on which mouse button is pressed, and which node type is selected from the toolbar.
            if (button == MouseButtons.Left)
            {
                if (Toolbar.SelectedNodeType == NodeType.Block)
                    MakeBlock();
                else if (Toolbar.SelectedNodeType == NodeType.Dense)
                    MakeDense();
            }
            else if (button == MouseButtons.Right)
                Reset();
        }
    }
}
