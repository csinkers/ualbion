using UAlbion.Formats;

namespace UAlbion
{
    class Program
    {
        static void Main(string[] args)
        {
            var palette = Assets.LoadPalette(2);
            var menuBackground = Assets.LoadTexture(AssetType.Picture, 19);

            /*
            Load palettes
            Load GUI sprites
            Show game frame
                Set mode to main menu
            */

        }
    }
/*
	public class Snake : Game
	{
		private List<SnakeSegment> _snake; // Store all segments of snake

		// Coordinates of the food
		private int _foodX;
		private int _foodY;

		private int _score; // Player's score

		private int _dir; // Direction of snake

		private bool _dead; // Is the snake dead?
		private bool _started; // Has the game been started?

		static void Main()
		{
			// Create an instance
			Snake s = new Snake();
			// Construct the game
			s.Construct(50, 50, 10, 10, 30);
			// Start the game
			s.Start();
		}

		// A part of the snake
		private struct SnakeSegment
		{
			public SnakeSegment(int x, int y) : this()
			{
				this.X = x;
				this.Y = y;
			}

			public int X { get; private set; } // X location
			public int Y { get; private set; } // Y location
		}

		// Set the title of the window
		public Snake() => AppName = "SNAKE!";

		// Start the game
		public override void OnCreate()
		{
			// Uncomment to make the game fullscreen
			//Enable(Subsystem.Fullscreen);

			Enable(Subsystem.HrText);

			Reset();
		}

		// Reset all fields
		private void Reset()
		{
			// Init and make the snake
			_snake = new List<SnakeSegment>();
			for (int i = 0; i < 9; i++)
				_snake.Add(new SnakeSegment(i + 20, 15));

			// Set the variables to default values
			_foodX = 30;
			_foodY = 15;
			_score = 0;
			_dir = 3;
			_dead = false;

			Seed();
		}

		public override void OnUpdate(float elapsed)
		{
			CheckStart();
			UpdateSnake();
			DrawGame();
		}

		// Draw the game
		private void DrawGame()
		{
			// Clear the screen
			Clear(Pixel.Presets.Black);

			if (_started) // Inform the player of their score
				DrawTextHr(new Point(15, 15), "Score: " + _score, Pixel.Presets.Green, 2);
			else // Inform the player to start by pressing enter
				DrawTextHr(new Point(15, 15), "Press Enter To Start", Pixel.Presets.Green, 2);

			// Draw the border
			DrawRect(new Point(0, 0), ScreenWidth - 1, ScreenHeight - 1, Pixel.Presets.Grey);

			// Render snake
			for (int i = 1; i < _snake.Count; i++)
				Draw(_snake[i].X, _snake[i].Y, _dead ? Pixel.Presets.Blue : Pixel.Presets.Yellow);

			// Draw snake head
			Draw(_snake[0].X, _snake[0].Y, _dead ? Pixel.Presets.Green : Pixel.Presets.Magenta);

			// Draw food
			Draw(_foodX, _foodY, Pixel.Presets.Red);
		}

		// Update the snake's position
		private void UpdateSnake()
		{
			// End game if snake is dead
			if (_dead)
				_started = false;

			// Turn right
			if (GetKey(Key.Right).Pressed)
			{
				_dir++;
				if (_dir == 4)
					_dir = 0;
			}

			// Turn left
			if (GetKey(Key.Left).Pressed)
			{
				_dir--;
				if (_dir == -1)
					_dir = 3;
			}

			if (_started)
			{
				// Move in the direction
				switch (_dir)
				{
					case 0: // UP
						_snake.Insert(0, new SnakeSegment(_snake[0].X, _snake[0].Y - 1));
						break;
					case 1: // RIGHT
						_snake.Insert(0, new SnakeSegment(_snake[0].X + 1, _snake[0].Y));
						break;
					case 2: // DOWN
						_snake.Insert(0, new SnakeSegment(_snake[0].X, _snake[0].Y + 1));
						break;
					case 3: // LEFT
						_snake.Insert(0, new SnakeSegment(_snake[0].X - 1, _snake[0].Y));
						break;
				}

				// Pop the tail
				_snake.RemoveAt(_snake.Count - 1);

				CheckCollision();
			}
		}

		// Check for snake's collision
		private void CheckCollision()
		{
			// Check collision with food
			if (_snake[0].X == _foodX && _snake[0].Y == _foodY)
			{
				_score++;
				RandomizeFood();

				_snake.Add(new SnakeSegment(_snake[_snake.Count - 1].X, _snake[_snake.Count - 1].Y));
			}

			// Check wall collision
			if (_snake[0].X <= 0 || _snake[0].X >= ScreenWidth || _snake[0].Y <= 0 || _snake[0].Y >= ScreenHeight - 1)
				_dead = true;

			// Check self collision
			for (int i = 1; i < _snake.Count; i++)
				if (_snake[i].X == _snake[0].X && _snake[i].Y == _snake[0].Y)
					_dead = true;
		}

		// Check if the game is started
		private void CheckStart()
		{
			if (!_started)
			{
				// Check if game has to be started
				if (GetKey(Key.Enter).Pressed)
				{
					Reset();
					_started = true;
				}
			}
		}

		// Set random location for food
		private void RandomizeFood()
		{
			// Loop while the food is not on empty cell
			while (GetScreenPixel(_foodX, _foodY) != Pixel.Presets.Black)
			{
				// Set food to random point
				_foodX = Random(ScreenWidth);
				_foodY = Random(ScreenHeight);
			}
		}
	}
    */
}
