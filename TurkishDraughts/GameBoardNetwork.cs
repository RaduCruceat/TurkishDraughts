﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using TurkishDraughts.Properties;

namespace TurkishDraughts
{
    public partial class GameBoardNetwork : Form
    {
        private PieceClass[][] pictureBoxButtons;
        private PlayerClass player1, player2, currentPlayer;
        private MultipleMovesClass multipleMovesClass;
        private PictureBoxPressedClass pictureBoxPressedClass;
        private PlayerTurnClass playerTurnClass;
        private int i_firstMove, j_firstMove;
        private const int ServerPort = 8888; // Port to communicate
        private TcpClient client;
        private TcpListener server;
        private bool isServer = false;
        private String playerNameGlobal = "";
        List<Tuple<int, int>> redPiecesWhoCanCapture = new List<Tuple<int, int>>();
        List<Tuple<int, int>> blackPiecesWhoCanCapture = new List<Tuple<int, int>>();

        public GameBoardNetwork(String playerName)
        {
            MaximizeBox = false;
            playerNameGlobal = playerName;
            initStartState();
            //initBoardButtons();
            InitializeComponent();
            initPlayerNames();
            choseColorButtonBlink();
        }

        private async Task choseColorButtonBlink()
        {
            int timesToBlink = 3;
            while (timesToBlink > 0)
            {
                serverStartButton.BackColor = Color.FromArgb(241, 217, 181);
                clientButton.BackColor = Color.FromArgb(241, 217, 181);
                await Task.Delay(300);
                serverStartButton.BackColor = Color.FromArgb(181, 136, 99);
                clientButton.BackColor = Color.FromArgb(181, 136, 99);
                await Task.Delay(300);
                timesToBlink--;
            }
        }

        private void initPlayerNames()
        {
            String name1 = "Red";
            String name2 = "Black";
            player1 = new PlayerClass(name1);
            player1TextBox.Text = player1.getName();
            player2 = new PlayerClass(name2);
            player2TextBox.Text = player2.getName();
            currentPlayer = player1;
            currentPlayerTextBox.Text = "Red moves";
            currentPlayerTextBox.ForeColor = Color.Red;
            player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
            player2TextBox.ForeColor = Color.FromArgb(49, 46, 43);
        }

        private void initLocalNames(String playerName)
        {
            if (isServer)
            {
                player1.setName(playerName);
                player1TextBox.Text = player1.getName();
                currentPlayer = player1;
                currentPlayerTextBox.Text = "Red moves";
            }
            else
            {
                player2.setName(playerName);
                player2TextBox.Text = player2.getName();
            }
        }

        private void initClientNames(String playerName)
        {
            if (isServer)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((Action)(() => initClientNames(playerName)));
                    return;
                }

                player2.setName(playerName); ;
                player2TextBox.Text = player2.getName();
                player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
                player2TextBox.ForeColor = Color.FromArgb(49, 46, 43);
            }
        }

        private void initServerNames(String playerName)
        {
            if (!isServer)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((Action)(() => initServerNames(playerName)));
                    return;
                }

                player1.setName(playerName); ;
                player1TextBox.Text = player1.getName();
                currentPlayer = player1;
                currentPlayerTextBox.Text = "Red moves";
            }
        }

        private void storePlayerName()
        {
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(playerNameGlobal);
                stream.Write(data, 0, data.Length);
            }
        }

        private void startServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, ServerPort);
                server.Start();
                //MessageBox.Show("Server started. Waiting for connections...");

                // Start accepting client connections asynchronously
                server.BeginAcceptTcpClient(handleClientConnection, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void handleClientConnection(IAsyncResult result)
        {
            try
            {
                // Accept the client connection
                client = server.EndAcceptTcpClient(result);
                //MessageBox.Show("Client connected.");
                initBoardButtons();
                receivePlayerName();
                storePlayerName();
                receiveGameData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void clientButton_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect(clientIPTextBox.Text, ServerPort);
                //MessageBox.Show("Connected to the server.");
                initBoardButtons();
                storePlayerName();
                receivePlayerName();
                serverStartButton.Text = "You play as";
                clientButton.Text = "Black color";
                clientIPTextBox.Text = "Client";
                isServer = false;
                clientIPTextBox.ReadOnly = true;
                clientButton.Enabled = false;
                serverStartButton.Enabled = false;
                initLocalNames(playerNameGlobal);
                blockPictureBoxes();
                receiveGameData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void serverStartButton_Click(object sender, EventArgs e)
        {
            isServer = true;
            initLocalNames(playerNameGlobal);
            serverStartButton.Text = "You play as";
            clientButton.Text = "Red color";
            clientIPTextBox.Text = "Server";

            clientIPTextBox.ReadOnly = true;
            clientButton.Enabled = false;
            serverStartButton.Enabled = false;
            startServer();
        }

        private void receivePlayerName()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] data = new byte[1024];
                    int bytesRead = stream.Read(data, 0, data.Length);
                    string message = Encoding.ASCII.GetString(data, 0, bytesRead);

                    if (!isServer)
                        initServerNames(message);
                    else
                        initClientNames(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            });
        }

        private void receiveGameData()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    while (true)
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] data = new byte[1024];
                        int bytesRead = stream.Read(data, 0, data.Length);
                        string message = Encoding.ASCII.GetString(data, 0, bytesRead);

                        updatePictureBoxesInNetwork(message);
                        checkPlayerTurnInNetwork();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            });
        }

        private void updatePictureBoxesInNetwork(string message)
        {
            string[] parts = message.Split(',');
            if (parts.Length == 5 &&
                int.TryParse(parts[0], out int i_initial) &&
                int.TryParse(parts[1], out int j_initial) &&
                int.TryParse(parts[2], out int i_final) &&
                int.TryParse(parts[3], out int j_final) &&
                int.TryParse(parts[4], out int value)
                )
            {
                movePieceInNetwork(i_initial, j_initial, i_final, j_final);
            }
        }

        private void storeMovedPieceInfo(int i_initial, int j_initial, int i_final, int j_final)
        {
            int newValue = pictureBoxButtons[i_initial][j_initial].getValue();
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes($"{i_initial},{j_initial},{i_final},{j_final},{newValue}");
                stream.Write(data, 0, data.Length);
            }
        }

        private void movePieceInNetwork(int i_initial, int j_initial, int i_final, int j_final)
        {
            redPiecesWhoCanCapture.Clear();
            blackPiecesWhoCanCapture.Clear();

            pictureBoxButtons[i_initial][j_initial].getPictureBox().BackColor = Color.Transparent;
            pictureBoxPressedClass.setPressed(false);

            swapImage(i_initial, j_initial, i_final, j_final);
            swapValue(i_initial, j_initial, i_final, j_final);
            if (removeCapturedPieces(i_initial, j_initial, i_final, j_final))
            {
                removeCapturedPieces(i_initial, j_initial, i_final, j_final);
                if (checkMultipleMoves(i_initial, j_initial, i_final, j_final) == false)
                {
                    checkIfPieceIsKing(i_final, j_final);
                    swapCurrentPlayerTurn(playerTurnClass.getPlayerTurn());
                    swapCurrentPlayerName();
                    multipleMovesClass.setPieceCanDoAMultipleMove(false);
                    checkPlayerTurnInNetwork();
                }
                else
                {
                    multipleMovesClass.setPieceCanDoAMultipleMove(true);
                    multipleMovesClass.setLastMultipleMovePositionI(i_initial);
                    multipleMovesClass.setLastMultipleMovePositionJ(j_initial);
                    multipleMovesClass.setCurrentMultipleMovePositionI(i_final);
                    multipleMovesClass.setCurrentMultipleMovePositionJ(j_final);
                }
            }
            else
            {
                checkIfPieceIsKing(i_final, j_final);
                swapCurrentPlayerTurn(playerTurnClass.getPlayerTurn());
                swapCurrentPlayerName();
                checkPlayerTurnInNetwork();
            }
            if (checkGameOver(player1, player2))
            {
                blockPictureBoxes();
                removeBoardTraces();
            }
        }

        private void checkPlayerTurnInNetwork()
        {
            if (isServer && playerTurnClass.getPlayerTurn() == true || !isServer && playerTurnClass.getPlayerTurn() == false)
            {
                if (isServer && playerTurnClass.getPlayerTurn() == true)
                {
                    clientIPTextBox.ForeColor = Color.Black;
                    blockPictureBoxes();
                }
                if (!isServer && playerTurnClass.getPlayerTurn() == false)
                {
                    clientIPTextBox.ForeColor = Color.Black;
                    blockPictureBoxes();
                }
            }
            else
            {
                clientIPTextBox.ForeColor = Color.Green;
                unblockPictureBoxes();
            }
        }

        private void initBoardButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => initBoardButtons()));
                return;
            }
            else
            {
                int value = 0;//valoare default pentru picturebox gol
                pictureBoxButtons = new PieceClass[8][];
                for (int i = 0; i < 8; i++)
                {
                    pictureBoxButtons[i] = new PieceClass[8];
                }
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        pictureBoxButtons[i][j] = new PieceClass(i, j, value, null, this, null);
                        Controls.Add(pictureBoxButtons[i][j].getPictureBox());
                    }
                }
            }
        }

        private void initStartState()
        {
            multipleMovesClass = new MultipleMovesClass(false, 0, 0, 0, 0);
            pictureBoxPressedClass = new PictureBoxPressedClass(false);
            playerTurnClass = new PlayerTurnClass(false);
        }

        private void removeBoardTraces()
        {
            //sterge casutele verzi,marcheaza tuplul de miscari de capturare
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (!playerTurnClass.getPlayerTurn())
                    {
                        if (!redPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                            pictureBoxButtons[i][j].getPictureBox().BackColor = Color.Transparent;
                        else
                            pictureBoxButtons[i][j].getPictureBox().BackColor = Color.GreenYellow;
                    }
                    else
                    {
                        if (!blackPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                            pictureBoxButtons[i][j].getPictureBox().BackColor = Color.Transparent;
                        else
                            pictureBoxButtons[i][j].getPictureBox().BackColor = Color.GreenYellow;
                    }
                }
            if (multipleMovesClass.getPieceCanDoAMultipleMove())
                pictureBoxButtons[multipleMovesClass.getCurrentMultipleMovePositionI()][multipleMovesClass.getCurrentMultipleMovePositionJ()].getPictureBox().BackColor = Color.GreenYellow;
        }
        private bool checkIfFirstRedPieceCanCapture()
        {
            redPiecesWhoCanCapture.Clear();
            bool moveFound = false;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (checkMultipleMoves(i, j, i, j) && pictureBoxButtons[i][j].getValue() % 2 == 0 && pictureBoxButtons[i][j].getValue() != 0)
                    {
                        moveFound = true;
                        redPiecesWhoCanCapture.Add(Tuple.Create(i, j));
                    }
                }
            //redPiecesWhoCanCapture.Add(Tuple.Create(-1, -1));
            if (moveFound)
                return true;
            return false;
        }
        private bool checkIfFirstBlackPieceCanCapture()
        {
            blackPiecesWhoCanCapture.Clear();
            bool moveFound = false;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (checkMultipleMoves(i, j, i, j) && pictureBoxButtons[i][j].getValue() % 2 != 0 && pictureBoxButtons[i][j].getValue() != 0)
                    {
                        moveFound = true;
                        blackPiecesWhoCanCapture.Add(Tuple.Create(i, j));
                    }
                }
            //redPiecesWhoCanCapture.Add(Tuple.Create(-1, -1));
            if (moveFound)
                return true;
            return false;
        }

        private void blockPictureBoxes()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => blockPictureBoxes()));
                return;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        pictureBoxButtons[i][j].getPictureBox().Enabled = false;
                    }
                }
                removeBoardTraces();
            }
        }

        private void unblockPictureBoxes()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => unblockPictureBoxes()));
                return;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        pictureBoxButtons[i][j].getPictureBox().Enabled = true;
                    }
                }
                removeBoardTraces();
            }
        }

        private bool checkGameOver(PlayerClass player1, PlayerClass player2)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => checkGameOver(player1, player2)));
                return false;
            }
            int counterRed = 0, counterBlack = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pictureBoxButtons[i][j].getValue() % 2 == 0 && pictureBoxButtons[i][j].getValue() != 0)
                        counterRed++;
                    if (pictureBoxButtons[i][j].getValue() % 2 != 0 && pictureBoxButtons[i][j].getValue() != 0)
                        counterBlack++;
                }
            }
            if (counterBlack == 0 || counterRed == 0)
            {
                blockPictureBoxes();
                if (counterBlack == 0)
                {
                    player1TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    currentPlayerTextBox.Text = "Game over";
                    currentPlayerTextBox.ForeColor = Color.Blue;
                    GameOverForm gameOverForm = new GameOverForm("Game over.\n" + player1.getName() + " wins!");
                    gameOverForm.Show();
                    //MessageBox.Show(player1.getName() + " wins!");
                }
                if (counterRed == 0)
                {
                    player1TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    currentPlayerTextBox.Text = "Game over";
                    currentPlayerTextBox.ForeColor = Color.Blue;
                    GameOverForm gameOverForm = new GameOverForm("Game over.\n" + player2.getName() + " wins!");
                    gameOverForm.Show();
                    //MessageBox.Show(player2.getName() + " wins!");
                }
                return true;
            }
            if (counterRed == 1 && counterBlack == 1 && multipleMovesClass.getPieceCanDoAMultipleMove() == false)
            {
                //MessageBox.Show("Draw");
                player1TextBox.BackColor = Color.FromArgb(49, 46, 43);
                player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
                currentPlayerTextBox.Text = "Game over";
                currentPlayerTextBox.ForeColor = Color.Blue;
                GameOverForm gameOverForm = new GameOverForm("Game over.\n" + "It's a draw!");
                gameOverForm.Show();
                removeBoardTraces();
                return true;
            }

            return false;
        }

        public void swapCurrentPlayerName()
        {
            if (currentPlayerTextBox.InvokeRequired)
            {
                currentPlayerTextBox.Invoke(new Action(() => swapCurrentPlayerName()));
            }
            else
            {
                if (playerTurnClass.getPlayerTurn() == false)
                {
                    checkIfFirstRedPieceCanCapture();
                    currentPlayerTextBox.Text = "Red moves";
                    currentPlayerTextBox.ForeColor = Color.Red;
                    player1TextBox.BackColor = Color.FromArgb(181, 136, 99);

                    player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    player2TextBox.ForeColor = Color.FromArgb(49, 46, 43);
                }
                else
                {
                    checkIfFirstBlackPieceCanCapture();
                    currentPlayerTextBox.Text = "Black moves";
                    currentPlayerTextBox.ForeColor = Color.Black;
                    player2TextBox.BackColor = Color.FromArgb(181, 136, 99);
                    player1TextBox.BackColor = Color.FromArgb(49, 46, 43);
                    player1TextBox.ForeColor = Color.FromArgb(49, 46, 43);
                }
            }
        }

        public void swapCurrentPlayerTurn(bool turn)
        {
            if (turn == false)
                playerTurnClass.setPlayerTurn(true);
            else
                playerTurnClass.setPlayerTurn(false);
        }

        public void swapImage(int i_initial, int j_initial, int i_final, int j_final)
        {
            pictureBoxButtons[i_final][j_final].getPictureBox().BackgroundImage = pictureBoxButtons[i_initial][j_initial].getPictureBox().BackgroundImage;
            pictureBoxButtons[i_initial][j_initial].getPictureBox().BackgroundImage = null;
        }

        public void swapValue(int i_initial, int j_initial, int i_final, int j_final)
        {
            pictureBoxButtons[i_final][j_final].setValue(pictureBoxButtons[i_initial][j_initial].getValue());
            pictureBoxButtons[i_initial][j_initial].setValue(0);
        }

        public void resetPictureboxPressed(int i_initial, int j_initial, int i_final, int j_final)
        {
            pictureBoxButtons[i_initial][j_initial].getPictureBox().BackColor = Color.Transparent;
            pictureBoxPressedClass.setPressed(false);
        }

        public void checkInitialMove(int i, int j)
        {
            i_firstMove = i;
            j_firstMove = j;
            pictureBoxButtons[i][j].getPictureBox().BackColor = Color.GreenYellow;
            pictureBoxPressedClass.setPressed(true);
            checkLegalMoves(i, j);
        }

        public void checkFinalMove(int i_initial, int j_initial, int i_final, int j_final)
        {
            if (pictureBoxPressedClass.getPressed() == true)
            {
                if (pictureBoxButtons[i_final][j_final].getValue() != 0 ||
                    pictureBoxButtons[i_initial][j_initial].getValue() == 0 ||
                    pictureBoxButtons[i_initial][j_initial].getValue() % 2 != 0 && playerTurnClass.getPlayerTurn() == false ||
                    pictureBoxButtons[i_initial][j_initial].getValue() % 2 == 0 && playerTurnClass.getPlayerTurn() == true ||
                    pictureBoxButtons[i_final][j_final].getPictureBox().BackColor != Color.GreenYellow ||
                    (multipleMovesClass.getPieceCanDoAMultipleMove() == true &&
                    (multipleMovesClass.getCurrentMultipleMovePositionI() != i_initial || multipleMovesClass.getCurrentMultipleMovePositionJ() != j_initial))
                    )
                {
                    resetPictureboxPressed(i_initial, j_initial, i_final, j_final);
                    removeBoardTraces();
                }
                else
                {
                    redPiecesWhoCanCapture.Clear();
                    blackPiecesWhoCanCapture.Clear();
                    movePiece(i_initial, j_initial, i_final, j_final);


                    //aici transmiti in retea matricea de pictureboxuri
                }
                removeBoardTraces();
            }
            if (checkGameOver(player1, player2))
            {
                blockPictureBoxes();
                removeBoardTraces();
            }
        }

        public void checkLegalMoves(int i, int j)
        {
            if (pictureBoxButtons[i][j].getValue() != 0)
            {
                drawLegalMovesTraces(i, j);
            }
        }

        public bool checkMultipleMovesBlackPiece(int i_intial, int j_initial, int i, int j)
        {
            if (i < 6 && pictureBoxButtons[i + 2][j].getValue() == 0 && pictureBoxButtons[i + 1][j].getValue() % 2 == 0 && pictureBoxButtons[i + 1][j].getValue() != 0)
                return true;
            if (j < 6 && pictureBoxButtons[i][j + 2].getValue() == 0 && pictureBoxButtons[i][j + 1].getValue() % 2 == 0 && pictureBoxButtons[i][j + 1].getValue() != 0)
                return true;
            if (j > 1 && pictureBoxButtons[i][j - 2].getValue() == 0 && pictureBoxButtons[i][j - 1].getValue() % 2 == 0 && pictureBoxButtons[i][j - 1].getValue() != 0)
                return true;
            return false;
        }

        public bool checkMultipleMovesRedPiece(int i_intial, int j_initial, int i, int j)
        {
            if (i > 1 && pictureBoxButtons[i - 2][j].getValue() == 0 && pictureBoxButtons[i - 1][j].getValue() % 2 != 0)
                return true;
            if (j > 1 && pictureBoxButtons[i][j - 2].getValue() == 0 && pictureBoxButtons[i][j - 1].getValue() % 2 != 0)
                return true;
            if (j < 6 && pictureBoxButtons[i][j + 2].getValue() == 0 && pictureBoxButtons[i][j + 1].getValue() % 2 != 0)
                return true;
            return false;
        }

        public bool checkMultipleMovesRedKingLeft(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool redPieceInBetween = false;
            bool doubleBlackPiece = false;
            while (j_search > 1 && !redPieceInBetween && !doubleBlackPiece)
            {
                j_search--;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 &&
                 pictureBoxButtons[i][j_search].getValue() != 0 &&
                 pictureBoxButtons[i][j_search - 1].getValue() % 2 != 0 &&
                 pictureBoxButtons[i][j_search - 1].getValue() != 0)
                    doubleBlackPiece = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    redPieceInBetween = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0 && pictureBoxButtons[i][j_search - 1].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMovesRedKingRight(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool redPieceInBetween = false;
            bool doubleBlackPiece = false;
            while (j_search < 6 && !redPieceInBetween && !doubleBlackPiece)
            {
                j_search++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 &&
                 pictureBoxButtons[i][j_search].getValue() != 0 &&
                 pictureBoxButtons[i][j_search + 1].getValue() % 2 != 0 &&
                 pictureBoxButtons[i][j_search + 1].getValue() != 0)
                    doubleBlackPiece = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    redPieceInBetween = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0 && pictureBoxButtons[i][j_search + 1].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMovesRedKingUp(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool redPieceInBetween = false;
            bool doubleBlackPiece = false;
            while (i_search > 1 && !redPieceInBetween && !doubleBlackPiece)
            {
                i_search--;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 &&
                  pictureBoxButtons[i_search][j].getValue() != 0 &&
                  pictureBoxButtons[i_search - 1][j].getValue() % 2 != 0 &&
                  pictureBoxButtons[i_search - 1][j].getValue() != 0)
                    doubleBlackPiece = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    redPieceInBetween = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0 && pictureBoxButtons[i_search - 1][j].getValue() == 0)
                    return true;
            }

            return false;
        }

        public bool checkMultipleMovesRedKingDown(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool redPieceInBetween = false;
            bool doubleBlackPiece = false;
            while (i_search < 6 && !redPieceInBetween && !doubleBlackPiece)
            {
                i_search++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 &&
                   pictureBoxButtons[i_search][j].getValue() != 0 &&
                   pictureBoxButtons[i_search + 1][j].getValue() % 2 != 0 &&
                   pictureBoxButtons[i_search + 1][j].getValue() != 0)
                    doubleBlackPiece = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    redPieceInBetween = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0 && pictureBoxButtons[i_search + 1][j].getValue() == 0)
                    return true;
            }

            return false;
        }

        public bool checkMultipleMovesBlackKingLeft(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool blackPieceInBetween = false;
            bool doubleRedPiece = false;
            while (j_search > 1 && !blackPieceInBetween && !doubleRedPiece)
            {
                j_search--;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 &&
                  pictureBoxButtons[i][j_search].getValue() != 0 &&
                  pictureBoxButtons[i][j_search - 1].getValue() % 2 == 0 &&
                  pictureBoxButtons[i][j_search - 1].getValue() != 0)
                    doubleRedPiece = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    blackPieceInBetween = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0 && pictureBoxButtons[i][j_search - 1].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMovesBlackKingRight(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool blackPieceInBetween = false;
            bool doubleRedPiece = false;
            while (j_search < 6 && !blackPieceInBetween && !doubleRedPiece)
            {
                j_search++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 &&
                  pictureBoxButtons[i][j_search].getValue() != 0 &&
                  pictureBoxButtons[i][j_search + 1].getValue() % 2 == 0 &&
                  pictureBoxButtons[i][j_search + 1].getValue() != 0)
                    doubleRedPiece = true;

                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    blackPieceInBetween = true;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0 && pictureBoxButtons[i][j_search + 1].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMovesBlackKingUp(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool blackPieceInBetween = false;
            bool doubleRedPiece = false;
            while (i_search > 1 && !blackPieceInBetween && !doubleRedPiece)
            {
                i_search--;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 &&
                   pictureBoxButtons[i_search][j].getValue() != 0 &&
                   pictureBoxButtons[i_search - 1][j].getValue() % 2 == 0 &&
                   pictureBoxButtons[i_search - 1][j].getValue() != 0)
                    doubleRedPiece = true;

                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    blackPieceInBetween = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0 && pictureBoxButtons[i_search - 1][j].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMovesBlackKingDown(int i_intial, int j_initial, int i, int j)
        {
            int i_search = i, j_search = j;
            bool blackPieceInBetween = false;
            bool doubleRedPiece = false;
            while (i_search < 6 && !blackPieceInBetween && !doubleRedPiece)
            {
                i_search++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 &&
                    pictureBoxButtons[i_search][j].getValue() != 0 &&
                    pictureBoxButtons[i_search + 1][j].getValue() % 2 == 0 &&
                    pictureBoxButtons[i_search + 1][j].getValue() != 0)
                    doubleRedPiece = true;

                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    blackPieceInBetween = true;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0 && pictureBoxButtons[i_search + 1][j].getValue() == 0)
                    return true;
            }
            return false;
        }

        public bool checkMultipleMoves(int i_initial, int j_initial, int i, int j)
        {
            //piesa rosie
            if (pictureBoxButtons[i][j].getValue() == 2)
            {
                if (checkMultipleMovesRedPiece(i_initial, j_initial, i, j))
                    return true;
            }
            //piesa neagra
            if (pictureBoxButtons[i][j].getValue() == 1)
            {
                if (checkMultipleMovesBlackPiece(i_initial, j_initial, i, j))
                    return true;
            }
            //rege rosu
            if (pictureBoxButtons[i][j].getValue() == 4)
            {
                //nu permitem ca regele sa faca o miscare multipla la 180 de grade, zona din care a venit e falsa
                bool i_up = false;
                bool i_down = false;
                bool j_right = false;
                bool j_left = false;
                if (i != i_initial)
                    if (i - i_initial > 0)
                        i_up = true;
                    else
                        i_down = true;

                if (j != j_initial)
                    if (j - j_initial > 0)
                        j_left = true;
                    else
                        j_right = true;

                if (!j_left)
                    if (checkMultipleMovesRedKingLeft(i_initial, j_initial, i, j))
                        return true;
                if (!j_right)
                    if (checkMultipleMovesRedKingRight(i_initial, j_initial, i, j))
                        return true;
                if (!i_up)
                    if (checkMultipleMovesRedKingUp(i_initial, j_initial, i, j))
                        return true;
                if (!i_down)
                    if (checkMultipleMovesRedKingDown(i_initial, j_initial, i, j))
                        return true;
            }
            //rege negru
            if (pictureBoxButtons[i][j].getValue() == 3)
            {
                //nu permitem ca regele sa faca o miscare multipla la 180 de grade, zona din care a venit e falsa
                bool i_up = false;
                bool i_down = false;
                bool j_right = false;
                bool j_left = false;
                if (i != i_initial)
                    if (i - i_initial > 0)
                        i_up = true;
                    else
                        i_down = true;

                if (j != j_initial)
                    if (j - j_initial > 0)
                        j_left = true;
                    else
                        j_right = true;

                if (!j_left)
                    if (checkMultipleMovesBlackKingLeft(i_initial, j_initial, i, j))
                        return true;

                if (!j_right)
                    if (checkMultipleMovesBlackKingRight(i_initial, j_initial, i, j))
                        return true;
                if (!i_up)
                    if (checkMultipleMovesBlackKingUp(i_initial, j_initial, i, j))
                        return true;
                if (!i_down)
                    if (checkMultipleMovesBlackKingDown(i_initial, j_initial, i, j))
                        return true;
            }
            return false;
        }

        public void drawRedPieceTrace(int i, int j)
        {
            //spatiu gol
            if (multipleMovesClass.getPieceCanDoAMultipleMove() == false && !checkIfFirstRedPieceCanCapture())
            {
                if (i > 0 && pictureBoxButtons[i - 1][j].getValue() == 0)
                    pictureBoxButtons[i - 1][j].getPictureBox().BackColor = Color.GreenYellow;
                if (j > 0 && pictureBoxButtons[i][j - 1].getValue() == 0)
                    pictureBoxButtons[i][j - 1].getPictureBox().BackColor = Color.GreenYellow;
                if (j < 7 && pictureBoxButtons[i][j + 1].getValue() == 0)
                    pictureBoxButtons[i][j + 1].getPictureBox().BackColor = Color.GreenYellow;
            }
            //piesa langa
            if (i > 1 && pictureBoxButtons[i - 2][j].getValue() == 0 && pictureBoxButtons[i - 1][j].getValue() % 2 != 0)
                pictureBoxButtons[i - 2][j].getPictureBox().BackColor = Color.GreenYellow;
            if (j > 1 && pictureBoxButtons[i][j - 2].getValue() == 0 && pictureBoxButtons[i][j - 1].getValue() % 2 != 0)
                pictureBoxButtons[i][j - 2].getPictureBox().BackColor = Color.GreenYellow;
            if (j < 6 && pictureBoxButtons[i][j + 2].getValue() == 0 && pictureBoxButtons[i][j + 1].getValue() % 2 != 0)
                pictureBoxButtons[i][j + 2].getPictureBox().BackColor = Color.GreenYellow;

            if ((multipleMovesClass.getCurrentMultipleMovePositionI() != i ||
           multipleMovesClass.getCurrentMultipleMovePositionJ() != j) &&
           multipleMovesClass.getPieceCanDoAMultipleMove())
                removeBoardTraces();
        }

        public void drawBlackPieceTrace(int i, int j)
        {
            //spatiu gol
            if (multipleMovesClass.getPieceCanDoAMultipleMove() == false && !checkIfFirstBlackPieceCanCapture())
            {
                if (i < 7 && pictureBoxButtons[i + 1][j].getValue() == 0)
                    pictureBoxButtons[i + 1][j].getPictureBox().BackColor = Color.GreenYellow;
                if (j < 7 && pictureBoxButtons[i][j + 1].getValue() == 0)
                    pictureBoxButtons[i][j + 1].getPictureBox().BackColor = Color.GreenYellow;
                if (j > 0 && pictureBoxButtons[i][j - 1].getValue() == 0)
                    pictureBoxButtons[i][j - 1].getPictureBox().BackColor = Color.GreenYellow;
            }
            //piesa langa
            if (i < 6 && pictureBoxButtons[i + 2][j].getValue() == 0 && pictureBoxButtons[i + 1][j].getValue() % 2 == 0 && pictureBoxButtons[i + 1][j].getValue() != 0)
                pictureBoxButtons[i + 2][j].getPictureBox().BackColor = Color.GreenYellow;
            if (j < 6 && pictureBoxButtons[i][j + 2].getValue() == 0 && pictureBoxButtons[i][j + 1].getValue() % 2 == 0 && pictureBoxButtons[i][j + 1].getValue() != 0)
                pictureBoxButtons[i][j + 2].getPictureBox().BackColor = Color.GreenYellow;
            if (j > 1 && pictureBoxButtons[i][j - 2].getValue() == 0 && pictureBoxButtons[i][j - 1].getValue() % 2 == 0 && pictureBoxButtons[i][j - 1].getValue() != 0)
                pictureBoxButtons[i][j - 2].getPictureBox().BackColor = Color.GreenYellow;

            if ((multipleMovesClass.getCurrentMultipleMovePositionI() != i ||
            multipleMovesClass.getCurrentMultipleMovePositionJ() != j) &&
            multipleMovesClass.getPieceCanDoAMultipleMove())
                removeBoardTraces();
        }

        public void drawRedKingLeftTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (j_search > 0 && contor < 2)
            {
                j_search--;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i][j_search].getValue() == 0)
                {
                    pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstRedPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawRedKingRightTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (j_search < 7 && contor < 2)
            {
                j_search++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i][j_search].getValue() == 0)
                {
                    pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstRedPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawRedKingUpTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (i_search > 0 && contor < 2)
            {
                i_search--;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i_search][j].getValue() == 0)
                {
                    pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstRedPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawRedKingDownTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (i_search < 7 && contor < 2)
            {
                i_search++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i_search][j].getValue() == 0)
                {
                    pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstRedPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawBlackKingLeftTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (j_search > 0 && contor < 2)
            {
                j_search--;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i][j_search].getValue() == 0)
                {
                    pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstBlackPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawBlackKingRightTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (j_search < 7 && contor < 2)
            {
                j_search++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 == 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i][j_search].getValue() % 2 != 0 && pictureBoxButtons[i][j_search].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i][j_search].getValue() == 0)
                {
                    pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstBlackPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i][j_search].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawBlackKingUpTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (i_search > 0 && contor < 2)
            {
                i_search--;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i_search][j].getValue() == 0)
                {
                    pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstBlackPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawBlackKingDownTrace(int i, int j)
        {
            int i_search = i, j_search = j, contor = 0;
            while (i_search < 7 && contor < 2)
            {
                i_search++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 == 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor++;
                if (pictureBoxButtons[i_search][j].getValue() % 2 != 0 && pictureBoxButtons[i_search][j].getValue() != 0)
                    contor = 2;
                if (pictureBoxButtons[i_search][j].getValue() == 0)
                {
                    pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.GreenYellow;
                    if ((multipleMovesClass.getPieceCanDoAMultipleMove() && contor == 0) || (checkIfFirstBlackPieceCanCapture() && contor == 0))
                        pictureBoxButtons[i_search][j].getPictureBox().BackColor = Color.Transparent;
                }
            }
        }

        public void drawLegalMovesTraces(int i, int j)
        {
            //piese rosii
            if (pictureBoxButtons[i][j].getValue() % 2 == 0)
            {
                drawRedPieceTrace(i, j);

                if (checkIfFirstRedPieceCanCapture())
                {
                    if (!redPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                        removeBoardTraces();
                }

                if (pictureBoxButtons[i][j].getValue() == 4)
                {
                    if (multipleMovesClass.getPieceCanDoAMultipleMove() == false)
                    {
                        if (!checkIfFirstRedPieceCanCapture())
                        {
                            drawRedKingLeftTrace(i, j);
                            drawRedKingRightTrace(i, j);
                            drawRedKingUpTrace(i, j);
                            drawRedKingDownTrace(i, j);
                        }
                        else
                        {
                            if (!redPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                                removeBoardTraces();
                            if (checkMultipleMovesRedKingLeft(i, j, i, j))
                                drawRedKingLeftTrace(i, j);
                            if (checkMultipleMovesRedKingRight(i, j, i, j))
                                drawRedKingRightTrace(i, j);
                            if (checkMultipleMovesRedKingUp(i, j, i, j))
                                drawRedKingUpTrace(i, j);
                            if (checkMultipleMovesRedKingDown(i, j, i, j))
                                drawRedKingDownTrace(i, j);
                        }
                    }
                    else
                    {
                        int i_initial = multipleMovesClass.getLastMultipleMovePositionI();
                        int j_initial = multipleMovesClass.getLastMultipleMovePositionJ();
                        bool i_up = false;
                        bool i_down = false;
                        bool j_right = false;
                        bool j_left = false;
                        if (i != i_initial)
                            if (i - i_initial > 0)
                                i_up = true;
                            else
                                i_down = true;

                        if (j != j_initial)
                            if (j - j_initial > 0)
                                j_left = true;
                            else
                                j_right = true;

                        if (!j_left)
                            if (checkMultipleMovesRedKingLeft(i_initial, j_initial, i, j))
                                drawRedKingLeftTrace(i, j);
                        if (!j_right)
                            if (checkMultipleMovesRedKingRight(i_initial, j_initial, i, j))
                                drawRedKingRightTrace(i, j);
                        if (!i_up)
                            if (checkMultipleMovesRedKingUp(i_initial, j_initial, i, j))
                                drawRedKingUpTrace(i, j);
                        if (!i_down)
                            if (checkMultipleMovesRedKingDown(i_initial, j_initial, i, j))
                                drawRedKingDownTrace(i, j);

                        if (multipleMovesClass.getPieceCanDoAMultipleMove())
                        {
                            if ((multipleMovesClass.getCurrentMultipleMovePositionI() != i || multipleMovesClass.getCurrentMultipleMovePositionJ() != j))
                                removeBoardTraces();
                        }





                    }
                }
            }
            //piese negre
            if (pictureBoxButtons[i][j].getValue() % 2 != 0)
            {
                drawBlackPieceTrace(i, j);

                if (checkIfFirstBlackPieceCanCapture())
                {
                    if (!blackPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                        removeBoardTraces();
                }

                if (pictureBoxButtons[i][j].getValue() == 3)
                    if (multipleMovesClass.getPieceCanDoAMultipleMove() == false)
                    {
                        if (!checkIfFirstBlackPieceCanCapture())
                        {
                            drawBlackKingLeftTrace(i, j);
                            drawBlackKingRightTrace(i, j);
                            drawBlackKingUpTrace(i, j);
                            drawBlackKingDownTrace(i, j);
                        }
                        else
                        {
                            if (!blackPiecesWhoCanCapture.Any(tuple => tuple.Item1 == i && tuple.Item2 == j))
                                removeBoardTraces();

                            if (checkMultipleMovesBlackKingLeft(i, j, i, j))
                                drawBlackKingLeftTrace(i, j);

                            if (checkMultipleMovesBlackKingRight(i, j, i, j))
                                drawBlackKingRightTrace(i, j);

                            if (checkMultipleMovesBlackKingUp(i, j, i, j))
                                drawBlackKingUpTrace(i, j);

                            if (checkMultipleMovesBlackKingDown(i, j, i, j))
                                drawBlackKingDownTrace(i, j);
                        }
                    }
                    else
                    {
                        int i_initial = multipleMovesClass.getLastMultipleMovePositionI();
                        int j_initial = multipleMovesClass.getLastMultipleMovePositionJ();
                        bool i_up = false;
                        bool i_down = false;
                        bool j_right = false;
                        bool j_left = false;
                        if (i != i_initial)
                            if (i - i_initial > 0)
                                i_up = true;
                            else
                                i_down = true;

                        if (j != j_initial)
                            if (j - j_initial > 0)
                                j_left = true;
                            else
                                j_right = true;

                        if (!j_left)
                            if (checkMultipleMovesBlackKingLeft(i_initial, j_initial, i, j))
                                drawBlackKingLeftTrace(i, j);
                        if (!j_right)
                            if (checkMultipleMovesBlackKingRight(i_initial, j_initial, i, j))
                                drawBlackKingRightTrace(i, j);
                        if (!i_up)
                            if (checkMultipleMovesBlackKingUp(i_initial, j_initial, i, j))
                                drawBlackKingUpTrace(i, j);
                        if (!i_down)
                            if (checkMultipleMovesBlackKingDown(i_initial, j_initial, i, j))
                                drawBlackKingDownTrace(i, j);

                        if (multipleMovesClass.getPieceCanDoAMultipleMove())
                        {
                            if ((multipleMovesClass.getCurrentMultipleMovePositionI() != i || multipleMovesClass.getCurrentMultipleMovePositionJ() != j))
                                removeBoardTraces();
                        }
                    }
            }
        }

        public bool removeCapturedPieces(int i_initial, int j_initial, int i_final, int j_final)
        {
            if (j_initial > j_final)
            {
                int j_temp = j_final; j_final = j_initial; j_initial = j_temp;
            }
            if (i_initial > i_final)
            {
                int i_temp = i_final; i_final = i_initial; i_initial = i_temp;
            }

            if (j_initial == j_final && i_initial != i_final + 1)
                for (int i = i_initial + 1; i < i_final; i++)
                    if (pictureBoxButtons[i][j_final].getValue() != 0)
                    {
                        pictureBoxButtons[i][j_final].setValue(0);
                        pictureBoxButtons[i][j_final].getPictureBox().BackgroundImage = null;
                        return true;
                    }
            if (i_initial == i_final && j_initial != j_final + 1)
                for (int j = j_initial + 1; j < j_final; j++)
                    if (pictureBoxButtons[i_initial][j].getValue() != 0)
                    {
                        pictureBoxButtons[i_initial][j].setValue(0);
                        pictureBoxButtons[i_initial][j].getPictureBox().BackgroundImage = null;
                        return true;
                    }
            return false;
        }

        public void movePiece(int i_initial, int j_initial, int i_final, int j_final)
        {
            pictureBoxButtons[i_initial][j_initial].getPictureBox().BackColor = Color.Transparent;
            pictureBoxPressedClass.setPressed(false);

            //
            storeMovedPieceInfo(i_initial, j_initial, i_final, j_final);

            //
            swapImage(i_initial, j_initial, i_final, j_final);
            swapValue(i_initial, j_initial, i_final, j_final);
            if (removeCapturedPieces(i_initial, j_initial, i_final, j_final))
            {
                removeCapturedPieces(i_initial, j_initial, i_final, j_final);
                if (checkMultipleMoves(i_initial, j_initial, i_final, j_final) == false)
                {
                    checkIfPieceIsKing(i_final, j_final);

                    swapCurrentPlayerTurn(playerTurnClass.getPlayerTurn());
                    swapCurrentPlayerName();
                    checkPlayerTurnInNetwork();

                    multipleMovesClass.setPieceCanDoAMultipleMove(false);
                    //checkIfBoardIsServer();
                }
                else
                {
                    multipleMovesClass.setPieceCanDoAMultipleMove(true);
                    multipleMovesClass.setLastMultipleMovePositionI(i_initial);
                    multipleMovesClass.setLastMultipleMovePositionJ(j_initial);
                    multipleMovesClass.setCurrentMultipleMovePositionI(i_final);
                    multipleMovesClass.setCurrentMultipleMovePositionJ(j_final);
                }
            }
            else
            {
                checkIfPieceIsKing(i_final, j_final);
                //

                //
                swapCurrentPlayerTurn(playerTurnClass.getPlayerTurn());
                swapCurrentPlayerName();

                checkPlayerTurnInNetwork();
            }
        }

        public bool checkIfPieceIsKing(int i, int j)
        {
            if (pictureBoxButtons[i][j].getValue() == 1 && i == 7)
            {
                pictureBoxButtons[i][j].setValue(3);
                pictureBoxButtons[i][j].getPictureBox().BackgroundImage = Resources.BlackKing;
                return true;
            }
            if (pictureBoxButtons[i][j].getValue() == 2 && i == 0)
            {
                pictureBoxButtons[i][j].setValue(4);
                pictureBoxButtons[i][j].getPictureBox().BackgroundImage = Resources.RedKing;
                return true;
            }
            return false;
        }

        public void pictureBoxClick(object sender)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (sender == pictureBoxButtons[i][j].getPictureBox())
                    {
                        if (pictureBoxPressedClass.getPressed() == false)
                            checkInitialMove(i, j);
                        else
                        {
                            checkFinalMove(i_firstMove, j_firstMove, i, j);
                        }
                    }
                }
            }
        }

        private void player2TextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void player1TextBox_TextChanged(object sender, EventArgs e)
        {
        }
    }
}