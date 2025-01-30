using System.Diagnostics;

namespace PathfindingAlgorithmVisualiser
{
    public static class Toolbar
    {
        public static NodeType? SelectedNodeType { get; private set; }
        public static Algorithm? SelectedAlgorithm { get; private set; }

        private static readonly Label _messageLbl = new();

        private static readonly ComboBox _speedCbx = new();
        private static readonly ComboBox _clearCbx = new();
        private static readonly ComboBox _algorithmsCbx = new();
        private static readonly ComboBox _nodeTypeCbx = new();
        private static readonly ComboBox _generationsCbx = new();

        private static readonly Button _modeBtn = new();
        private static readonly Button _diversionBtn = new();
        private static readonly Button _simStateBtn = new();

        public static void CreateToolbar(int width, int height, int messageHeight, Form form)
        {
            // Useful values for enabling consistent spacing of controls.
            int mainBarHeight = height - messageHeight;
            int sectionWidth = width / 7;

            // These two labels are just aesthetic and don't have functionality, so they are quickly made here.
            var mainBar = new Label()
            {
                Size = new Size(width, mainBarHeight),
                BackColor = Color.DodgerBlue,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var simControlBar = new Label()
            {
                Size = new Size(sectionWidth, mainBarHeight),
                BackColor = Color.SkyBlue,
                BorderStyle = BorderStyle.FixedSingle,
            };

            // Each method creates a toolbar control.
            CreateMessageLbl(width, messageHeight, 0, mainBarHeight, form);

            CreateModeBtn(sectionWidth - 30, mainBarHeight - 10, (sectionWidth * 3) + 15, 5, form);
            CreateDiversionBtn(sectionWidth - 30, mainBarHeight / 2, (sectionWidth * 6) + 15, mainBarHeight / 4, form);
            CreateSimStateBtn(sectionWidth / 3, mainBarHeight - 4, sectionWidth / 12, 2, form);

            CreateSpeedCbx(sectionWidth / 2, (sectionWidth / 2) - 10, mainBarHeight / 4, form);
            CreateClearCbx(sectionWidth - 20, sectionWidth + 10, mainBarHeight / 4, form);
            CreateAlgorithmsCbx(sectionWidth - 20, (sectionWidth * 2) + 10, mainBarHeight / 4, form);
            CreateNodeTypeCbx(sectionWidth - 20, (sectionWidth * 4) + 10, mainBarHeight / 4, form);
            CreateGenerationsCbx(sectionWidth - 20, (sectionWidth * 5) + 10, mainBarHeight / 4, form);

            // 'simControlBar' is added to the form after all the controls are created so that 'simStateBtn' and 'speedCbx' can be seen.
            form.Controls.Add(simControlBar);
            // 'mainBar' is added to the form last so that it is at the 'back' and all other controls can be seen.
            form.Controls.Add(mainBar);
        }

        public static void RemoveDiversion()
        {
            // Updates the 'diversion button' and removes references to the diversion node.
            if (Grid.Diversion is not null)
            {
                Grid.Dragging = false;
                Grid.Moving = null;
                Grid.Diversion.Reset();
                Grid.Diversion = null;

                _diversionBtn.Text = "Add diversion";
                _diversionBtn.BackColor = Color.LightBlue;
            }
        }

        private static void OutputMessage(string message)
        {
            _messageLbl.Text = message;
        }

        private static void CreateMessageLbl(int width, int height, int xPos, int yPos, Form form)
        {
            _messageLbl.Location = new Point(xPos, yPos);
            _messageLbl.Size = new Size(width, height);
            _messageLbl.BackColor = Color.AliceBlue;
            _messageLbl.Font = new Font(FontFamily.GenericSansSerif, 10);
            _messageLbl.TextAlign = ContentAlignment.MiddleCenter;

            form.Controls.Add(_messageLbl);
            OutputMessage("Edit the grid, select an algorithm to visualise, and then start a simulation!");
        }

        private static void CreateModeBtn(int width, int height, int xPos, int yPos, Form form)
        {
            _modeBtn.Location = new Point(xPos, yPos);
            _modeBtn.Size = new Size(width, height);
            _modeBtn.BackColor = Color.PaleGreen;
            _modeBtn.Font = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);
            _modeBtn.Text = "Visualise";

            _modeBtn.MouseClick += ModeBtn_MouseClick;

            form.Controls.Add(_modeBtn);
        }

        private static void ModeBtn_MouseClick(object? sender, MouseEventArgs e)
        {
            // Starts or stops the simulation and updates the main button.
            if (e.Button == MouseButtons.Left)
            {
                if (Simulation.Running)
                {
                    Simulation.Stop();

                    _simStateBtn.Text = "PAUSE";
                    _simStateBtn.BackColor = Color.Orange;
                    _modeBtn.Text = "Visualise";
                    _modeBtn.BackColor = Color.PaleGreen;

                    OutputMessage("Edit the grid, select an algorithm to visualise, and then start a simulation!");
                }
                else if (SelectedAlgorithm is not null && Simulation.Speed != 0)
                {
                    Simulation.Start();

                    _modeBtn.Text = "End";
                    _modeBtn.BackColor = Color.DarkRed;

                    if (SelectedAlgorithm == Algorithm.BFS)
                        OutputMessage("Visualising Breadth First Search (BFS) - UNWEIGHTED (dense nodes disabled)");
                    else if (SelectedAlgorithm == Algorithm.Dijkstra)
                        OutputMessage("Visualising Dijkstra's Algorithm - WEIGHTED");
                    else if (SelectedAlgorithm == Algorithm.AStar)
                        OutputMessage("Visualising A* Search - WEIGHTED (and uses a heuristic - Manhattan Distance)");
                }
                else if (SelectedAlgorithm is null)
                    OutputMessage("You need to select an algorithm to visualise!");
                else if (Simulation.Speed == 0)
                    OutputMessage("You need to select a simulation speed!");
            }
        }

        private static void CreateDiversionBtn(int width, int height, int xPos, int yPos, Form form)
        {
            _diversionBtn.Location = new Point(xPos, yPos);
            _diversionBtn.Size = new Size(width, height);
            _diversionBtn.BackColor = Color.LightBlue;
            _diversionBtn.Text = "Add Diversion";

            _diversionBtn.MouseClick += DiversionBtn_MouseClick;

            form.Controls.Add(_diversionBtn);
        }

        private static void DiversionBtn_MouseClick(object? sender, MouseEventArgs e)
        {
            // Determines whether the user is adding or removing the diversion node.
            if (e.Button == MouseButtons.Left && !Simulation.Running)
            {
                if (Grid.Diversion is null)
                {
                    Grid.Moving = NodeType.Diversion;
                    Grid.Dragging = true;

                    _diversionBtn.Text = "Remove diversion";
                    _diversionBtn.BackColor = Color.PaleVioletRed;
                }
                else
                    RemoveDiversion();
            }
        }

        private static void CreateSimStateBtn(int width, int height, int xPos, int yPos, Form form)
        {
            _simStateBtn.Location = new Point(xPos, yPos);
            _simStateBtn.Size = new Size(width, height);
            _simStateBtn.BackColor = Color.Orange;
            _simStateBtn.Text = "PAUSE";

            _simStateBtn.MouseClick += SimStateBtn_MouseClick;

            form.Controls.Add(_simStateBtn);
        }

        private static void SimStateBtn_MouseClick(object? sender, MouseEventArgs e)
        {
            // Pauses or resumes the simulation and updates the button's appearance.
            if (e.Button == MouseButtons.Left && Simulation.Running && !Simulation.Visualised)
            {
                if (Simulation.Paused)
                {
                    Simulation.Resume();

                    _simStateBtn.Text = "PAUSE";
                    _simStateBtn.BackColor = Color.Orange;
                }
                else
                {
                    Simulation.Pause();

                    _simStateBtn.Text = "PLAY";
                    _simStateBtn.BackColor = Color.Lime;
                }
            }
        }

        private static void CreateSpeedCbx(int width, int xPos, int yPos, Form form)
        {
            _speedCbx.Location = new Point(xPos, yPos);
            _speedCbx.Width = width;
            _speedCbx.DropDownStyle = ComboBoxStyle.DropDownList;

            _speedCbx.Items.Add("Speed:");
            _speedCbx.Items.Add("Very slow");
            _speedCbx.Items.Add("Slow");
            _speedCbx.Items.Add("Medium");
            _speedCbx.Items.Add("Fast");
            _speedCbx.Items.Add("Very fast");
            _speedCbx.Items.Add("Instant");
            _speedCbx.SelectedIndex = 0;

            _speedCbx.SelectionChangeCommitted += SpeedCbx_SelectionChangeCommitted;

            form.Controls.Add(_speedCbx);
        }

        private static void SpeedCbx_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            // Detects when the user selects a new speed.
            if (_speedCbx.SelectedIndex == 0)
                _speedCbx.SelectedIndex = Simulation.Speed;
            else if (_speedCbx.SelectedIndex != Simulation.Speed)
                Simulation.ChangeSpeed(_speedCbx.SelectedIndex);
        }

        private static void CreateClearCbx(int width, int xPos, int yPos, Form form)
        {
            _clearCbx.Location = new Point(xPos, yPos);
            _clearCbx.Width = width;
            _clearCbx.DropDownStyle = ComboBoxStyle.DropDownList;

            _clearCbx.Items.Add("Clear functions:");
            _clearCbx.Items.Add("Clear block nodes");
            _clearCbx.Items.Add("Clear dense nodes");
            _clearCbx.Items.Add("Reset grid");
            _clearCbx.SelectedIndex = 0;

            _clearCbx.SelectionChangeCommitted += ClearCbx_SelectionChangeCommitted;

            form.Controls.Add(_clearCbx);
        }

        private static void ClearCbx_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            // Decides which clearing function to call.
            if (!Simulation.Running)
            {
                if (_clearCbx.SelectedIndex == 1)
                    Grid.Clear(NodeType.Block);
                else if (_clearCbx.SelectedIndex == 2)
                    Grid.Clear(NodeType.Dense);
                else if (_clearCbx.SelectedIndex == 3)
                    Grid.Reset();
            }

            // Resets the selected index back to the description.
            _clearCbx.SelectedIndex = 0;
        }

        private static void CreateAlgorithmsCbx(int width, int xPos, int yPos, Form form)
        {
            _algorithmsCbx.Location = new Point(xPos, yPos);
            _algorithmsCbx.Width = width;
            _algorithmsCbx.DropDownStyle = ComboBoxStyle.DropDownList;

            _algorithmsCbx.Items.Add("Select an algorithm:");
            _algorithmsCbx.Items.Add("Dijkstra's");
            _algorithmsCbx.Items.Add("A*");
            _algorithmsCbx.Items.Add("Breadth-first");
            _algorithmsCbx.SelectedIndex = 0;

            _algorithmsCbx.SelectionChangeCommitted += AlgorithmsCbx_SelectionChangeCommitted;

            form.Controls.Add(_algorithmsCbx);
        }

        private static void AlgorithmsCbx_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            // Determines which algorithm the user wishes to visualise.
            if (!Simulation.Running)
            {
                if (_algorithmsCbx.SelectedIndex == 1)
                    SelectedAlgorithm = Algorithm.Dijkstra;
                else if (_algorithmsCbx.SelectedIndex == 2)
                    SelectedAlgorithm = Algorithm.AStar;
                else if (_algorithmsCbx.SelectedIndex == 3)
                    SelectedAlgorithm = Algorithm.BFS;
                else
                    SelectedAlgorithm = null;
            }
            else if (SelectedAlgorithm is not null)
                _algorithmsCbx.SelectedIndex = (int)SelectedAlgorithm + 1;
        }

        private static void CreateNodeTypeCbx(int width, int xPos, int yPos, Form form)
        {
            _nodeTypeCbx.Location = new Point(xPos, yPos);
            _nodeTypeCbx.Width = width;
            _nodeTypeCbx.DropDownStyle = ComboBoxStyle.DropDownList;

            _nodeTypeCbx.Items.Add("Add node:");
            _nodeTypeCbx.Items.Add("Block");
            _nodeTypeCbx.Items.Add("Dense");
            _nodeTypeCbx.SelectedIndex = 0;

            _nodeTypeCbx.SelectionChangeCommitted += NodeTypeCbx_SelectionChangeCommitted;

            form.Controls.Add(_nodeTypeCbx);
        }

        private static void NodeTypeCbx_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            // Determines which node the user wants to add to the grid.
            if (_nodeTypeCbx.SelectedIndex == 1)
                SelectedNodeType = NodeType.Block;
            else if (_nodeTypeCbx.SelectedIndex == 2)
                SelectedNodeType = NodeType.Dense;
            else
                SelectedNodeType = null;
        }

        private static void CreateGenerationsCbx(int width, int xPos, int yPos, Form form)
        {
            _generationsCbx.Location = new Point(xPos, yPos);
            _generationsCbx.Width = width;
            _generationsCbx.DropDownStyle = ComboBoxStyle.DropDownList;

            _generationsCbx.Items.Add("Generations:");
            _generationsCbx.Items.Add("Generate block maze");
            _generationsCbx.Items.Add("Generate dense maze");
            _generationsCbx.Items.Add("Random grid layout");
            _generationsCbx.SelectedIndex = 0;

            _generationsCbx.SelectionChangeCommitted += GenerationsCbx_SelectionChangeCommitted;

            form.Controls.Add(_generationsCbx);
        }

        private static void GenerationsCbx_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            // Determines which generation layout the user wants.
            if (!Simulation.Running)
            {
                if (_generationsCbx.SelectedIndex == 1)
                    Grid.MazeGeneration(NodeType.Block);
                else if (_generationsCbx.SelectedIndex == 2)
                    Grid.MazeGeneration(NodeType.Dense);
                else if (_generationsCbx.SelectedIndex == 3)
                    Grid.RandomGeneration();
            }

            // Resets the selected index back to the description.
            _generationsCbx.SelectedIndex = 0;
        }
    }
}
