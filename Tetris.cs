using System;
using System.Threading;

namespace Tetris
{
    class Program
    {
        static void Main(string[] args)
        {
            // Oculta el cursor para una mejor experiencia de juego
            Console.CursorVisible = false;

            // Dimensiones del tablero de juego
            int width = 10;
            int height = 20;

            // Puntuación y puntuación máxima
            int score = 0;
            int highScore = 0;

            // Inicialización del tablero de juego y la puntuación máxima
            bool[,] playfield = new bool[width, height];

            // Variable para controlar si el jugador quiere jugar otra vez
            bool playAgain = true;

            do
            {
                // Limpia la consola, restablece la puntuación y crea un nuevo tablero de juego
                Console.Clear();
                score = 0;
                playfield = new bool[width, height];

                // Genera una nueva pieza aleatoria y la posición inicial
                bool[,] currentPiece = GetRandomPiece();
                int pieceX = width / 2 - currentPiece.GetLength(0) / 2;
                int pieceY = 0;

                // Permite al jugador elegir el nivel de dificultad
                int difficultyLevel = ChooseDifficultyLevel();

                // Obtiene la velocidad de caída según el nivel de dificultad seleccionado
                int fallSpeed = GetFallSpeed(difficultyLevel);

                do
                {
                    // Limpia la consola y dibuja el tablero de juego actualizado
                    Console.Clear();
                    DrawPlayfield(playfield, currentPiece, pieceX, pieceY, score, highScore);
                    Thread.Sleep(fallSpeed); // Ajusta la velocidad de caída de las piezas según el nivel de dificultad

                    // Maneja la entrada del jugador para mover o rotar la pieza actual
                    if (Console.KeyAvailable)
                    {
                        var keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.LeftArrow)
                        {
                            if (IsPositionValid(playfield, currentPiece, pieceX - 1, pieceY))
                            {
                                pieceX--;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.RightArrow)
                        {
                            if (IsPositionValid(playfield, currentPiece, pieceX + 1, pieceY))
                            {
                                pieceX++;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.DownArrow)
                        {
                            if (IsPositionValid(playfield, currentPiece, pieceX, pieceY + 1))
                            {
                                pieceY++;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.R)
                        {
                            var rotatedPiece = RotatePiece(currentPiece);
                            if (IsPositionValid(playfield, rotatedPiece, pieceX, pieceY))
                            {
                                currentPiece = rotatedPiece;
                            }
                        }
                    }

                    // Si la posición siguiente es válida, la pieza sigue bajando; de lo contrario, se fusiona con el tablero
                    if (IsPositionValid(playfield, currentPiece, pieceX, pieceY + 1))
                    {
                        pieceY++;
                    }
                    else
                    {
                        MergePiece(playfield, currentPiece, pieceX, pieceY);
                        score += ClearLines(playfield);

                        // Verifica si el juego ha terminado (cuando la nueva pieza alcanza la parte superior)
                        if (pieceY == 0)
                        {
                            Console.WriteLine("Game Over");
                            if (score > highScore)
                            {
                                highScore = score;
                                Console.WriteLine($"¡Nuevo récord! Puntuación máxima: {highScore}");
                            }
                            else
                            {
                                Console.WriteLine($"Puntuación máxima: {highScore}");
                            }
                            Console.Write("¿Quieres jugar otra vez? (s/n): ");
                            char choice = Console.ReadKey().KeyChar;
                            if (choice != 's')
                            {
                                playAgain = false;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Genera una nueva pieza aleatoria y la posición inicial
                        currentPiece = GetRandomPiece();
                        pieceX = width / 2 - currentPiece.GetLength(0) / 2;
                        pieceY = 0;
                    }

                } while (true);
            } while (playAgain);
        }

        // Dibuja el tablero de juego y las piezas actuales en la consola
        static void DrawPlayfield(bool[,] playfield, bool[,] currentPiece, int pieceX, int pieceY, int score, int highScore)
        {
            Console.WriteLine($"Score: {score}    High Score: {highScore}");
            Console.WriteLine(new string('-', playfield.GetLength(0) * 2 + 2));
            for (int y = 0; y < playfield.GetLength(1); y++)
            {
                Console.Write("|");
                for (int x = 0; x < playfield.GetLength(0); x++)
                {
                    if (x >= pieceX && x < pieceX + currentPiece.GetLength(0) &&
                        y >= pieceY && y < pieceY + currentPiece.GetLength(1) &&
                        currentPiece[x - pieceX, y - pieceY])
                    {
                        Console.Write("[]");
                    }
                    else
                    {
                        Console.Write(playfield[x, y] ? "[]" : "  ");
                    }
                }
                Console.WriteLine("|");
            }
            Console.WriteLine(new string('-', playfield.GetLength(0) * 2 + 2));
        }

        // Comprueba si la posición de la pieza actual es válida en el tablero de juego
        static bool IsPositionValid(bool[,] playfield, bool[,] piece, int x, int y)
        {
            for (int i = 0; i < piece.GetLength(0); i++)
            {
                for (int j = 0; j < piece.GetLength(1); j++)
                {
                    if (piece[i, j])
                    {
                        int newX = x + i;
                        int newY = y + j;

                        if (newX < 0 || newX >= playfield.GetLength(0) ||
                            newY < 0 || newY >= playfield.GetLength(1))
                        {
                            return false;
                        }

                        if (playfield[newX, newY])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // Fusiona la pieza actual con el tablero de juego cuando alcanza el fondo
        static void MergePiece(bool[,] playfield, bool[,] piece, int x, int y)
        {
            for (int i = 0; i < piece.GetLength(0); i++)
            {
                for (int j = 0; j < piece.GetLength(1); j++)
                {
                    if (piece[i, j])
                    {
                        playfield[x + i, y + j] = true;
                    }
                }
            }
        }

        // Elimina las líneas completas del tablero de juego y devuelve el número de líneas eliminadas
        static int ClearLines(bool[,] playfield)
        {
            int linesCleared = 0;
            for (int y = playfield.GetLength(1) - 1; y >= 0; y--)
            {
                bool isLineFull = true;
                for (int x = 0; x < playfield.GetLength(0); x++)
                {
                    if (!playfield[x, y])
                    {
                        isLineFull = false;
                        break;
                    }
                }
                if (isLineFull)
                {
                    linesCleared++;
                    for (int newY = y; newY > 0; newY--)
                    {
                        for (int x = 0; x < playfield.GetLength(0); x++)
                        {
                            playfield[x, newY] = playfield[x, newY - 1];
                        }
                    }
                    y++;
                }
            }
            return linesCleared;
        }

        // Rota la pieza actual en sentido horario
        static bool[,] RotatePiece(bool[,] piece)
        {
            int width = piece.GetLength(0);
            int height = piece.GetLength(1);
            bool[,] rotatedPiece = new bool[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rotatedPiece[y, x] = piece[width - x - 1, y];
                }
            }

            return rotatedPiece;
        }

        // Devuelve una pieza aleatoria
        static bool[,] GetRandomPiece()
        {
            Random random = new Random();
            int pieceIndex = random.Next(7);

            switch (pieceIndex)
            {
                case 0:
                    return new bool[,] { { true, true }, { true, true } };
                case 1:
                    return new bool[,] { { true, false }, { true, false }, { true, true } };
                case 2:
                    return new bool[,] { { false, true }, { false, true }, { true, true } };
                case 3:
                    return new bool[,] { { true, true, true }, { false, true, false } };
                case 4:
                    return new bool[,] { { false, true, true }, { true, true, false } };
                case 5:
                    return new bool[,] { { true, true, false }, { false, true, true } };
                case 6:
                    return new bool[,] { { true }, { true }, { true }, { true } };
                default:
                    return new bool[,] { { true, true }, { true, true } };
            }
        }

        // Permite al jugador elegir el nivel de dificultad
        static int ChooseDifficultyLevel()
        {
            Console.WriteLine("Choose Difficulty Level:");
            Console.WriteLine("1. Easy");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. Hard");

            int choice;
            do
            {
                Console.Write("Enter your choice (1-3): ");
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3);

            return choice;
        }

        // Devuelve la velocidad de caída de las piezas según el nivel de dificultad seleccionado
        static int GetFallSpeed(int difficultyLevel)
        {
            switch (difficultyLevel)
            {
                case 1:
                    return 200; // Fácil
                case 2:
                    return 100; // Medio
                case 3:
                    return 50;  // Difícil
                default:
                    return 100; // Valor predeterminado
            }
        }
    }
}
