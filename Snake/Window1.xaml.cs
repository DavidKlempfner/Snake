//TODO:
//don't pass in full object to snakefood.cs, pass in only what u need
//select speed option
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Web.Mail;

namespace Snake
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        #region Pranks
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(String command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;
        

        private void DoPranks()
        {
            //open cd drive
            mciSendString("set CDAudio door open", null, 0, IntPtr.Zero);
            //mciSendString("set CDAudio door closed", null, 0, IntPtr.Zero);

            //minimize all windows
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
            System.Threading.Thread.Sleep(2000);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);

            //open text file
            TextWriter tw = new StreamWriter("YouHaveBeenHacked.txt");
            tw.WriteLine("YOU HAVE BEEN HACKED HAHAHAH!");
            tw.Close();
            System.Diagnostics.Process.Start("YouHaveBeenHacked.txt");

            //send fake email
            SmtpMail.SmtpServer = "mail.optusnet.com.au";
            SmtpMail.Send("Kristina.Keneally@nsw.gov.au", "katelyn.rowsell@hotmail.com", "Message From The Premier", "hey k8lz its the ex-premier here whatsup ?!?");
        }
        #endregion

        enum MoveDirection
        {
            Right,
            Left,
            Up,
            Down
        };

        bool _nextMoveAlreadyset;//if snake is moving right, and you quickly press up left
        //_nextMove will be set to up and then left within one tick interval of the timer,
        //this means the snake on the next tick will start moving left which is not allowed if it's currently going right

        DispatcherTimer _timer;
        Point _tempPoint = new Point(0, 0);//used to store tail Point
        Point _snakeHead = new Point();
        Point _snakeTail = new Point();
        int _tagNumber;
        int _snakeLength = 5;
        int _snakeBeginRow = 1;
        public int NoOfColsAndRows { get; set; }
        MoveDirection _nextMove = MoveDirection.Right;
        int _speed = 60;
        public Rectangle[,] Rects { get; set; }
        public Point CurrentFoodPt;
        bool _isPaused;
        int _score;
        int _highScore;
        int _slowestSpeed = 500;
        const string _pauseText = " Press space to pause. Press arrow keys or space to unpause.";
        const string _scoreText = "Score: ";
        const string _highScoreText = ", High Score: ";
        const string _restartText = " Press enter to restart";
        bool _hasCrashed;

        public Window1()
        {
            InitializeComponent();
            if (File.Exists("HighScore.txt"))
            {
                GetHighScore();
            }
            ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
            SpeedSlider.Value = _slowestSpeed - _speed;
            InstructionBox.Text = _pauseText;
            InputEventSubscription();
            NoOfColsAndRows = 40;
            CreateSnakeArray();
            SnakeFood sf = new SnakeFood(this);
            sf.GenerateRandomNumber();
            UpdateSnakeDisplay();
            StartTimer();
        }

        private void InputEventSubscription()
        {
            KeyDown += SnakeWin_KeyDown;
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _speed = _slowestSpeed - (int)e.NewValue;
            _timer.Interval = TimeSpan.FromMilliseconds(_speed);
        }

        private void GenerateRandomNumber()
        {
            //used for placing the first food rectangle
            //need the number to be constant until snake eats it, only in this class can the number remain constant,
            //each instance of snakefood creates a new random number
            Random random = new Random();
            CurrentFoodPt.X = random.Next(0, NoOfColsAndRows - 1);
            CurrentFoodPt.Y = random.Next(0, NoOfColsAndRows - 1);
        }

        private void CreateSnakeArray()
        {
            Rects = new Rectangle[NoOfColsAndRows, NoOfColsAndRows];
            for (int i = 0; i < _snakeLength; i++)
            {
                Rects[i, _snakeBeginRow] = new Rectangle();
                Rects[i, _snakeBeginRow].Fill = Brushes.Black;
                Rects[i, _snakeBeginRow].Tag = i;
            }

            _snakeHead.X = _snakeLength - 1;
            _snakeHead.Y = _snakeBeginRow;

            _snakeTail.X = 0;
            _snakeTail.Y = _snakeBeginRow;
        }

        private void UpdateSnakeDisplay()
        {
            SnakeGrid.Children.Clear();
            for (int i = 0; i < NoOfColsAndRows; i++)
            {
                for (int j = 0; j < NoOfColsAndRows; j++)
                {
                    if (Rects[i, j] != null)
                    {
                        SnakeGrid.Children.Add(Rects[i, j]);
                        Rects[i, j].SetValue(Grid.ColumnProperty, i);
                        Rects[i, j].SetValue(Grid.RowProperty, j);
                    }
                }
            }
            SnakeFood sf = new SnakeFood(this);
            sf.PlantFood(true);
        }

        private void SnakeWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && _nextMove != MoveDirection.Left && !_nextMoveAlreadyset)
            {
                if (_isPaused && !_hasCrashed)//make sure it hasn't crashed, othwerwise user can cheat
                //by pausing and unpausing and thus continuing the game when the snake has crashed
                {
                    UnpauseGame();
                }
                _nextMove = MoveDirection.Right;
                _nextMoveAlreadyset = true;
            }
            if (e.Key == Key.Left && _nextMove != MoveDirection.Right && !_nextMoveAlreadyset)
            {
                if (_isPaused && !_hasCrashed)
                {
                    UnpauseGame();
                }
                _nextMove = MoveDirection.Left;
                _nextMoveAlreadyset = true;
            }
            if (e.Key == Key.Up && _nextMove != MoveDirection.Down && !_nextMoveAlreadyset)
            {
                if (_isPaused && !_hasCrashed)
                {
                    UnpauseGame();
                }
                _nextMove = MoveDirection.Up;
                _nextMoveAlreadyset = true;
            }
            if (e.Key == Key.Down && _nextMove != MoveDirection.Up && !_nextMoveAlreadyset)
            {
                if (_isPaused && !_hasCrashed)
                {
                    UnpauseGame();
                }
                _nextMove = MoveDirection.Down;
                _nextMoveAlreadyset = true;
            }
            if (e.Key == Key.Space)
            {
                if (!_isPaused && !_hasCrashed)
                {
                    PauseGame();
                }
                else if (_isPaused && !_hasCrashed)
                {
                    UnpauseGame();
                }
            }
            if (e.Key == Key.Enter && _hasCrashed)
            {
                RestartGame();
            }
        }

        private void RestartGame()
        {
            _hasCrashed = false;
            _tagNumber = 0;
            _snakeLength = 5;
            _score = 0;
            ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
            InstructionBox.Text = _pauseText;
            _nextMove = MoveDirection.Right;
            CreateSnakeArray();
            GenerateRandomNumber();
            UpdateSnakeDisplay();
            StartTimer();
        }

        private void PauseGame()
        {
            if (!_isPaused)
            {
                _timer.Stop();
                _isPaused = true;
            }
        }

        private void UnpauseGame()
        {
            if (_isPaused)
            {
                _timer.Start();
                _isPaused = false;
            }
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_speed);
            _timer.Tick += TimerTask;
            _timer.Start();
        }

        private void TimerTask(object sender, EventArgs e)
        {
            _nextMoveAlreadyset = false;
            switch (_nextMove)
            {
                case MoveDirection.Right:
                    MoveRight();
                    break;
                case MoveDirection.Left:
                    MoveLeft();
                    break;
                case MoveDirection.Up:
                    MoveUp();
                    break;
                case MoveDirection.Down:
                    MoveDown();
                    break;
            }
        }

        private void UpdateTagNumber()
        {
            if (_tagNumber < _snakeLength - 1)
            {
                _tagNumber++;
            }
            else
            {
                _tagNumber = 0;
            }
        }

        public void UpdateSnakeTailPoint()
        {
            for (int i = 0; i < NoOfColsAndRows; i++)
            {
                for (int j = 0; j < NoOfColsAndRows; j++)
                {
                    if (_tagNumber != _snakeLength - 1 && Rects[i, j] != null && (int)Rects[i, j].Tag == _tagNumber + 1)
                    {
                        _tempPoint = new Point(i, j);//can't set new _snakeTail here because it get's nulled few lines down
                    }
                    else if (_tagNumber == _snakeLength - 1 && Rects[i, j] != null && (int)Rects[i, j].Tag == 0)
                    {
                        _tempPoint = new Point(i, j);
                    }
                }
            }
            Rects[(int)_snakeTail.X, (int)_snakeTail.Y] = null;
            _snakeTail = _tempPoint;//old _snakeTail is null, now set new _snakeTail
        }

        private void IncreaseTagNumberInRects()
        {
            for (int i = 0; i < NoOfColsAndRows; i++)
            {
                for (int j = 0; j < NoOfColsAndRows; j++)
                {
                    Point pt = new Point(i, j);
                    if (Rects[i, j] != null && (int)Rects[i, j].Tag >= _tagNumber && _snakeHead != pt)
                    {
                        Rects[i, j].Tag = (int)Rects[i, j].Tag + 1;
                    }
                }
            }
        }

        private void GetHighScore()
        {
            TextReader tr = new StreamReader("HighScore.txt");
            _highScore = Convert.ToInt32(tr.ReadLine());
            tr.Close();
        }

        private void UpdateScoreBox()
        {
            _score = _score + 10;
            if (_score > _highScore)
            {
                _highScore = _score;
                TextWriter tw = new StreamWriter("HighScore.txt");
                tw.WriteLine(_highScore);
                tw.Close();
            }
            ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
        }

        private void MoveRight()
        {
            //Check if next move will be out of bounds (snake will crash into wall) or if snake crashes into itself
            if (_snakeHead.X == NoOfColsAndRows - 1 || Rects[(int)_snakeHead.X + 1, (int)_snakeHead.Y] != null)
            {
                _timer.Stop();
                _hasCrashed = true;
                ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
                InstructionBox.Text = _restartText;
                return;
            }

            if ((int)_snakeHead.X + 1 == CurrentFoodPt.X && (int)_snakeHead.Y == CurrentFoodPt.Y)
            {
                Rects[(int)++_snakeHead.X, (int)_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                UpdateScoreBox();
                _snakeLength++;

                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                IncreaseTagNumberInRects();
                SnakeFood sf = new SnakeFood(this);
                sf.PlantFood(false);
            }
            else
            {
                Rects[(int)++_snakeHead.X, (int)_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                UpdateSnakeTailPoint();
            }
            UpdateTagNumber();
            UpdateSnakeDisplay();
        }

        private void MoveLeft()
        {
            if (_snakeHead.X == 0 || Rects[(int)_snakeHead.X - 1, (int)_snakeHead.Y] != null)
            {
                _timer.Stop();
                _hasCrashed = true;
                ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
                InstructionBox.Text = _restartText;
                return;
            }

            if ((int)_snakeHead.X - 1 == CurrentFoodPt.X && (int)_snakeHead.Y == CurrentFoodPt.Y)
            {
                Rects[(int)--_snakeHead.X, (int)_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                UpdateScoreBox();
                _snakeLength++;

                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                IncreaseTagNumberInRects();
                SnakeFood sf = new SnakeFood(this);
                sf.PlantFood(false);
            }

            else
            {
                Rects[(int)--_snakeHead.X, (int)_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                UpdateSnakeTailPoint();
            }

            UpdateSnakeDisplay();
            UpdateTagNumber();
        }

        private void MoveUp()
        {
            if (_snakeHead.Y == 0 || Rects[(int)_snakeHead.X, (int)_snakeHead.Y - 1] != null)
            {
                _timer.Stop();
                _hasCrashed = true;
                ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
                InstructionBox.Text = _restartText;
                return;
            }
            if ((int)_snakeHead.Y - 1 == CurrentFoodPt.Y && (int)_snakeHead.X == CurrentFoodPt.X)
            {
                Rects[(int)_snakeHead.X, (int)--_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                UpdateScoreBox();
                _snakeLength++;

                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                IncreaseTagNumberInRects();
                SnakeFood sf = new SnakeFood(this);
                sf.PlantFood(false);
            }
            else
            {
                Rects[(int)_snakeHead.X, (int)--_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                UpdateSnakeTailPoint();
            }

            UpdateSnakeDisplay();
            UpdateTagNumber();
        }

        private void MoveDown()
        {
            if (_snakeHead.Y == NoOfColsAndRows - 1 || Rects[(int)_snakeHead.X, (int)_snakeHead.Y + 1] != null)
            {
                _timer.Stop();
                _hasCrashed = true;
                ScoreBox.Text = _scoreText + Convert.ToString(_score) + _highScoreText + _highScore;
                InstructionBox.Text = _restartText;
                return;
            }
            if ((int)_snakeHead.Y + 1 == CurrentFoodPt.Y && (int)_snakeHead.X == CurrentFoodPt.X)
            {
                Rects[(int)_snakeHead.X, (int)++_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                UpdateScoreBox();
                _snakeLength++;

                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                IncreaseTagNumberInRects();
                SnakeFood sf = new SnakeFood(this);
                sf.PlantFood(false);
            }
            else
            {
                Rects[(int)_snakeHead.X, (int)++_snakeHead.Y] = new Rectangle() { Fill = Brushes.Black };
                Rects[(int)_snakeHead.X, (int)_snakeHead.Y].Tag = _tagNumber;
                UpdateSnakeTailPoint();
            }

            UpdateSnakeDisplay();
            UpdateTagNumber();
        }

        private void ShowTagNumbers()
        {
            for (int i = 0; i < NoOfColsAndRows; i++)
            {
                for (int j = 0; j < NoOfColsAndRows; j++)
                {
                    if (Rects[i, j] != null)
                    {
                        Console.WriteLine(Rects[i, j].Tag);
                    }
                }
            }
            Console.WriteLine("----");
        }
    }
}

