namespace PathfindingAlgorithmVisualiser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            const int NODE_SIZE = 25;
            const int TOOLBAR_HEIGHT = 100;
            const int MESSAGE_HEIGHT = 30;

            // Calculates how many columns and rows there should be in the grid.
            int gridCols = ClientSize.Width / NODE_SIZE;
            int gridRows = (ClientSize.Height - TOOLBAR_HEIGHT) / NODE_SIZE;

            Toolbar.CreateToolbar(ClientSize.Width, TOOLBAR_HEIGHT, MESSAGE_HEIGHT, this);
            Grid.DrawGrid(gridCols, gridRows, NODE_SIZE, TOOLBAR_HEIGHT, this);
        }
    }
}
