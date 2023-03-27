using System.Data.SqlClient;
using Advanced_War.Domain.GameDevelopment;
using Advanced_War.Domain.GameTheory;
using Advanced_War.Domain.GameTheory.Interfaces;
using Advanced_War.Domain.WarDeck;
using static System.Windows.Forms.ListView;

namespace WarUI
{
    public partial class MainForm : Form
    {
        private GameEngine? currentGame;
        private Player? firstPlayer;
        private Player? secondPlayer;

        public MainForm()
        {
            InitializeComponent();
            LoadPlayers();
            SetControls();
        }

        private void LoadPlayers()
        {
            var connectionString = "Server=.;Database=CardGameExample;Trusted_Connection=True;";
            IPlayerRepository playerRepository= new RepositoryFactory().CreateRepository(connectionString);
            var playersBoxOne = playerRepository.Get();
            var playersBoxTwo = playersBoxOne.ToList();

            comboPlayerOne.DataSource = playersBoxOne;
            comboPlayerOne.DisplayMember = "Name";
            comboPlayerTwo.DataSource = playersBoxTwo;
            comboPlayerTwo.DisplayMember = comboPlayerOne.DisplayMember;
        }

        #region defaulMenuItems section
        private void MenuItemManagePlayers_Click(object sender, EventArgs e)
        {
            Form form = new ManagePlayersForm();
            form.ShowDialog();
            // reload data
            LoadPlayers();
            SetControls();
        }

        private void MenuItemHighScore_Click(object sender, EventArgs e)
        {
            Form form = new HighScoreForm();
            form.ShowDialog();
            SetControls();
        }

        private void MenuItemAbout_Click(object sender, EventArgs e)
        {
            Form form = new AboutBoxForm();
            form.ShowDialog();
            SetControls();
        }
        #endregion

        private void ButtonCreateGame_Click(object sender, EventArgs e)
        {
            if (comboPlayerOne.SelectedItem is Player firstPlayer
                && comboPlayerTwo.SelectedItem is Player secondPlayer)
            {
                if (firstPlayer.Equals(secondPlayer))
                {
                    MessageBox.Show("Select two different players!");
                }
                else
                {
                    this.firstPlayer = firstPlayer;
                    this.secondPlayer = secondPlayer;
                    currentGame = new GameEngine(new List<Player> { this.firstPlayer, this.secondPlayer });
                    currentGame.GameStarted += this.CurrentGameOnGameStarted;
                    currentGame.GameEnded += this.CurrentGameOnGameEnded;
                    currentGame.TurnStarted += this.CurrentGameOnTurnStarted;
                    currentGame.TurnEnded += this.CurrentGameOnTurnEnded;
                    currentGame.StartNewGame();
                    SetControls();
                }
            }
            else
            {
                MessageBox.Show("No player selected");
            }
        }

        private void CurrentGameOnGameStarted(object? sender, GameEventArgs e)
        {
            buttonStartGame.Enabled = true;
            buttonAutoPlay.Enabled = true;
            buttonPlayTurn.Enabled = true;
            buttonEndGame.Enabled = true;
            SetPlayerInfo();
            SetControls();
        }
        
        private void CurrentGameOnTurnStarted(object? sender, GameEventArgs e)
        {
            SetPlayerInfo();
        }
        
        private void CurrentGameOnTurnEnded(object? sender, GameEventArgs e)
        {
            SetPlayerInfo();
        }

        private void CurrentGameOnGameEnded(object? sender, GameEventArgs e)
        {
            MessageBox.Show(e.Message);
            SetControls();
        }
        private void SetControls()
        {
            // visibility
            labelPlayerOneOnTable.Visible = firstPlayer != null;
            labelPlayerOneCardsOnHand.Visible = firstPlayer != null;
            labelPlayerTwoOnTable.Visible = secondPlayer != null;
            labelPlayerTwoCardsOnHand.Visible = secondPlayer != null;

            comboPlayerOne.Enabled = currentGame == null;
            comboPlayerTwo.Enabled = currentGame == null;
            buttonCreateGame.Enabled = currentGame == null;
        }
        
        private void SetPlayerInfo()
        {
            labelPlayerOneOnTable.Text = $"Cards from {firstPlayer?.Name} on table:";
            labelPlayerOneCardsOnHand.Text = $"Cards on hand {firstPlayer?.CardCount}";
            labelPlayerTwoOnTable.Text = $"Cards from {secondPlayer?.Name} on table:";
            labelPlayerTwoCardsOnHand.Text = $"Cards on hand {secondPlayer?.CardCount}";

            StackToListviewItemCollection(lvCardsPlayerOne.Items, firstPlayer?.PlayedCards);
            StackToListviewItemCollection(lvCardsPlayerTwo.Items, secondPlayer?.PlayedCards);
            
            // refresh
            lvCardsPlayerOne.Refresh();
            lvCardsPlayerTwo.Refresh();
            labelPlayerOneCardsOnHand.Refresh();
            labelPlayerTwoCardsOnHand.Refresh();
        }
        private void ButtonEndGame_Click(object sender, EventArgs e)
        {
            currentGame?.StartNewGame();
            currentGame = null;
           
        }

        private void ButtonPlayTurn_Click(object sender, EventArgs e)
        {
            currentGame?.StartTurn();
        }

        private void ButtonAutoPlay_Click(object sender, EventArgs e)
        {
            if (currentGame == null) return;

            Invoke(async () =>
            {
                await this.currentGame.AutoPlayAsync(100);
            });
        }

        /// <summary>
        /// Helper method to show cards in listview
        /// </summary>
        /// <param name="lvc"></param>
        /// <param name="stack"></param>
        private static void StackToListviewItemCollection(ListViewItemCollection lvc, Stack<Card>? stack)
        {
            lvc.Clear();

            if (stack == null)
                return;

            foreach (var item in stack.Select(c => c.ToString()))
            {
                lvc.Add(item);
            }
        }

        private void ButtonStartGame_Click(object sender, EventArgs e)
        {
            currentGame?.StartNewGame();
        }

    }
}