using System.Text.RegularExpressions;
using BattleShip;

// Muestra el mensaje de bienvenida y las reglas del juego
ShowWelcomeMessage();
ShowGameRules();

// Crea una instancia del motor de juego
GameEngine gameEngine = new();

// Bucle principal del juego
while (true)
{
    gameEngine.Run(); 
  
}

void ShowWelcomeMessage()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"
         __|__
     --o--o--(_)--o--o--
     ____|_____|____
    \            /
~~~~~\~~~~~~~~~~/~~~~~
  ~~~~~~~~~~~~~~~~~~~~  
");
    Console.WriteLine("¡Bienvenido a la Batalla Naval, Almirante!");
    Console.WriteLine("Prepárate para una batalla épica contra la armada enemiga.");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
}

void ShowGameRules()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("**Reglas del juego:**\n");
    Console.WriteLine("- Tienes barcos para colocar en tu isla: NAVÍOS (N), FRAGATAS (F) y PORTAVIONES (P).");
    Console.WriteLine("- Intenta hundir los barcos de la armada enemiga antes de que ellos destruyan los tuyos.");
    Console.WriteLine("- Se turnan para atacar, seleccionando una coordenada de la isla enemiga (ej: A5).");
    Console.WriteLine("- Un impacto exitoso se marca con 'X' y resta un barco del total del enemigo.");
    Console.WriteLine("- Un disparo fallido se marca con 'O' en la isla del jugador.");
    Console.WriteLine("- Puedes usar una pista (que revela la ubicación de un barco enemigo) una vez por juego (consume la pista).");
    Console.WriteLine("- ¡El primer jugador en hundir todos los barcos del enemigo gana la partida!");
    Console.WriteLine("- Puntaje: Incrementa según el valor del barco enemigo hundido (NAVÍO: 3, FRAGATA: 1, PORTAVIONES: 5).");
    Console.WriteLine("- Tienes un límite de 25 intentos para hundir todos los barcos enemigos.");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
    Console.WriteLine("Presiona cualquier tecla para comenzar...");
    Console.ReadKey();
    Console.Clear();
}

namespace BattleShip
{
    partial class GameEngine
    {
        public Ships playerShips;
        List<char> playerIsland;
        List<char> enemyIsland;
        List<char> enemyIslandCover;
        bool firstTimeGenerate = true;
        public int HintCount { get; private set; }
        public int Attempts { get; private set; }

        public GameEngine()
        {
            playerShips = new Ships(this, 6);
            HintCount = 3;
            Attempts = 25;
        }
        
public void ComputerAttack()
            {
                bool attackResult = playerShips.AttackEnemy(playerIsland, this);
                if (attackResult)
                {
                    GetError("¡La computadora ha atacado uno de tus barcos!");
                }
                else
                {
                    GetSuccess("La computadora ha fallado su ataque.");
                }
                Thread.Sleep(2000);
            }

        

        public void Run()
        {
            if (firstTimeGenerate)
            {
                InitializeIslands();
                firstTimeGenerate = false;
            }

            DrawIslands();
            ShowInformation();
            string userInput = GetString("░ Elige una opción > ");

            switch (userInput)
            {
                case "1":
                    // Ataque del jugador
                    var (row, column) = RequestAttackLocation();
                    bool attackResult = playerShips.Attack(enemyIsland, enemyIslandCover, row * 10 + column);

                    // Mostrar resultado del ataque
                    if (attackResult)
                    {
                        GetSuccess("¡Hundiste uno de los barcos enemigos! ¡PODER!");
                    }
                    else
                    {
                        GetError("¡Maldición, Almirante! No hay barco enemigo en esta posición.");
                    }

                    Attempts--;
                    // Verificar si alguien ha ganado
                    CheckWinner();
                    break;

                case "2":
                    // Usar pista
                    if (HintCount > 0)
                    {
                        bool result = Ships.Hint(enemyIsland, enemyIslandCover, playerShips.GetGameEngine());
                        HintCount--;

                        if (!result)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int index = playerIsland.LastIndexOf('N');
                                if (index == -1) index = playerIsland.LastIndexOf('F');
                                if (index == -1) index = playerIsland.LastIndexOf('P');
                                playerIsland.RemoveAt(index);
                                playerIsland.Insert(index, '░');
                            }
                            DrawIslands();
                            CheckWinner();
                        }
                    }
                    else
                    {
                        GetError("¡Ya no tienes pistas disponibles!");
                    }
                    break;

                case "3":
                    Console.Clear();
                    GetSuccess("Gracias por jugar. ¡Hasta luego!");
                    Environment.Exit(0);
                    break;

                default:
                    GetError("¡Opción incorrecta! Por favor, elige una opción válida.");
                    break;
            }
        }
       
        private void InitializeIslands()
        {
            playerIsland = new List<char>();
            playerIsland = new List<char>(); 
            enemyIsland = new List<char>();
            enemyIslandCover = new List<char>();

            for (int i = 0; i < 100; i++)
            {
                playerIsland.Add('■');
                enemyIsland.Add('■');
                enemyIslandCover.Add('·');
            }

            playerShips.GenerateShips(playerIsland);
            playerShips.GenerateShips(enemyIsland);
        }

        private void DrawIslands()
        {
            Console.Clear();
            Console.WriteLine("███ ISLA DEL JUGADOR ███\t\t   ███ ISLA ENEMIGA ███");
            Console.WriteLine("   1 2 3 4 5 6 7 8 9 10 \t\t   1 2 3 4 5 6 7 8 9 10\t\t");
            Console.WriteLine(" ╔═════════════════════╗\t\t ╔═════════════════════╗");

            for (int i = 0; i < 10; i++)
            {
                Console.Write(((char)('A' + i)) + "║ ");
                for (int j = 0; j < 10; j++)
                {
                    Console.Write($"{playerIsland[i * 10 + j]} ");
                }

                Console.Write("║\t\t");

                Console.Write(((char)('A' + i)) + "║ ");
                for (int j = 0; j < 10; j++)
                {
                    Console.Write($"{enemyIslandCover[i * 10 + j]} ");
                }

                Console.WriteLine("║");
            }

            Console.WriteLine(" ╚═════════════════════╝\t\t ╚═════════════════════╝");
        }

        private void ShowInformation()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"░ TUS BARCOS: {playerShips.TusBarcos} \t BARCOS ENEMIGOS: {playerShips.BarcosEnemigos} \t PISTAS RESTANTES: {HintCount} \t PUNTAJE: {playerShips.Puntaje} \t INTENTOS RESTANTES: {Attempts}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("░ 1. ATACAR \t 2. PISTA \t 3. SALIR");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void CheckWinner()
        {
            if (playerShips.BarcosEnemigos == 0)
            {
                DrawIslands();
                GetSuccess("¡Ganaste, Almirante! ¡Regresemos al país y contemos a la gente sobre tu valentía! ¡HURRA!");
                Environment.Exit(0);
            }
            else if (playerShips.TusBarcos == 0 || Attempts == 0)
            {
                DrawIslands();
                GetError("¡Perdiste, Almirante! ¡No te desanimes, juega de nuevo!");
                Environment.Exit(0);
            }
        }

        public static string GetString(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadLine();
        }

        public static void GetError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(3000);
        }

        public static void GetSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(2000);
        }

        private static (int, int) RequestAttackLocation()
        {
            Console.WriteLine("Ingresa la ubicación para atacar (ejemplo: A5):");

            string input = Console.ReadLine().ToUpper();

            while (!MyRegex().IsMatch(input))
            {
                Console.WriteLine("Entrada inválida. Asegúrate de ingresar una letra de A a J seguida de un número de 1 a 10 (ejemplo: A5):");

                input = Console.ReadLine().ToUpper();
            }

            int row = input[0] - 'A';
            int column = (input.Length == 3) ? 9 : int.Parse(input[1].ToString()) - 1;

            return (row, column);
        }

        [GeneratedRegex("^[A-J](10|[1-9])$")]
        private static partial Regex MyRegex();
    }

    class Ships
    {
        public GameEngine GameEngine { get; }
        public int TusBarcos { get; set; }
        public int BarcosEnemigos { get; set; }
        public int Puntaje { get; set; }
        public int HintCount { get; private set; }

        public Ships(GameEngine gameEngine, int cuantosBarcos)
        {
            GameEngine = gameEngine;
            TusBarcos = cuantosBarcos;
            BarcosEnemigos = cuantosBarcos;
            Puntaje = 0;
            HintCount = 1;
        }

        public void GenerateShips(List<char> isla)
        {
            int counter = 0;
            Random random = new();
            
            // Generar navíos (valor de 3)
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(0, isla.Count);
                if (FindBestLocation(index, isla))
                {
                    isla[index] = 'N';
                    isla[index - 1] = 'N';
                    isla[index + 1] = 'N';
                    counter++;
                }
            }

            // Generar fragatas (valor de 1)
            for (int i = 0; i < 4; i++)
            {
                int index = random.Next(0, isla.Count);
                if (FindBestLocation(index, isla))
                {
                    isla[index] = 'F';
                    counter++;
                }
            }

            // Generar portaaviones (valor de 5)
            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(0, isla.Count);
                if (FindBestLocation(index, isla))
                {
                    isla[index] = 'P';
                    isla[index - 1] = 'P';
                    isla[index + 1] = 'P';
                    counter++;
                }
            }
        }

        private bool FindBestLocation(int index, List<char> isla)
        {
            try
            {
                int rowStartIndex = index - (index % 10);
                for (int i = rowStartIndex; i < rowStartIndex + 10; i++)
                {
                    if (isla[i] != '■')
                    {
                        return false;
                    }
                }
                return index % 10 > 2 && index % 10 < 9;
            }
            catch
            {
                return false;
            }
        }

        public GameEngine GetGameEngine()
        {
            return GameEngine;
        }

        public static bool Hint(List<char> enemyIsla, List<char> enemyIslaCover, GameEngine gameEngine)
        {
            for (int i = 0; i < enemyIsla.Count; i++)
            {
                if (enemyIsla[i] == 'N' || enemyIsla[i] == 'F' || enemyIsla[i] == 'P')
                {
                    enemyIslaCover[i] = enemyIsla[i];
                    return true;
                }
            }
            return false;
        }

        public bool Attack(List<char> enemyIsla, List<char> enemyIslaCover, int index)
        {
            try
            {
                char target = enemyIsla[index];
                if (target == 'N' || target == 'F' || target == 'P')
                {
                    enemyIsla[index] = 'X';
                    enemyIslaCover[index] = 'X';
                    
                    if (target == 'N') Puntaje += 3;
                    if (target == 'F') Puntaje += 1;
                    if (target == 'P') Puntaje += 5;

                    BarcosEnemigos--;
                    return true;
                }
                else
                {
                    enemyIslaCover[index] = 'O';
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool AttackEnemy(List<char> playerIsla, Ships playerShips)
        {
            try
            {
                Random random = new();
                int index = random.Next(0, playerIsla.Count);

                char target = playerIsla[index];
                if (target == 'N' || target == 'F' || target == 'P')
                {
                    playerIsla[index] = 'X';
                    playerShips.TusBarcos--;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        internal bool AttackEnemy(List<char> playerIsland, GameEngine gameEngine)
        {
            throw new NotImplementedException();
        }
    }
}