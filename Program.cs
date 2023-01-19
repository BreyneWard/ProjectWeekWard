using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System;
using static System.Console;
using static System.IO.Directory;
using static System.IO.Path;
using static System.Environment;
using System.IO;

namespace ProjectWeekWard;

class Program
{
    // Global variables
    public static int currentOption = 0;
    public static string[] mainMenuOptions = new string[] { "1. Gebruiker Toevoegen", "2. Gebruiker bewerken", "3. Gebruiker verwijderen", "4. Inloggen" };
    public static string[] gameMenuOptions = new string[] { "1. Blackjack", "2. Slotmachine", "3. Memory", "4. Afloggen" };
    public static List<User> users = new List<User>();
    // test with file in current dir, users.txt
    // public static string filePath = "users.txt";
    public static int mainMenu = 0;
    public static int gameMenu = 1;
    public static string[] options;
    public static string dirName = "ProjectWeekWard";
    public static string fileName = "Users.txt";
    public static string directoryPath = Path.Combine(GetTempPath(), dirName);
    public static string filePath = Path.Combine(directoryPath, fileName);
    public enum CardSuiteEn { Clubs, Diamonds, Hearts, Spades }
    public static Random gen = new Random();
    public static int currentValueHandPlayer = 0;
    public static int currentValueHandDealer = 0;
    public static User signedInUser;

    static void Main(string[] args)
    {
        //bool exit = false;
        bool userLoadCompleted = false;

        do
        {
            userLoadCompleted = LoadUsers();
        } while (!userLoadCompleted);

        ShowMenu(mainMenu);
    }

    // Methods
    //
    //
    // Method for showing a menu, the menuIn parameter defines if the main or game menu will be launched.
    //public static void ShowMenu(int menuIn, User signedInUserIn)
    public static void ShowMenu(int menuIn)
    {

        if (menuIn == 0)
        {
            options = new string[mainMenuOptions.Length];
            Array.Copy(mainMenuOptions, options, mainMenuOptions.Length);
        }
        else if (menuIn == 1)
        {
            Console.WriteLine($"Welcome, {signedInUser.Name}.");
            Console.WriteLine($"Current Time: {DateTime.Now}.");
            signedInUser.CalculateOnlineTime(); 
            Console.WriteLine($"Current saldo: {signedInUser.Saldo}");
            Console.WriteLine($"It's a good day to play a game!");
            Console.WriteLine();

            System.Threading.Thread.Sleep(3000);
            
            options = new string[gameMenuOptions.Length];
            Array.Copy(gameMenuOptions, options, gameMenuOptions.Length);
        }
        else
        {
            Console.WriteLine("Invalid menu option");
        }

        while (true)
        {
            Console.Clear();
            
            Console.WriteLine("Please make a selection:");
            for (int i = 0; i < options.Length; i++)
            {
                if (i == currentOption)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("> ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  ");
                }
                Console.WriteLine(options[i]);
            }

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.UpArrow)
            {
                currentOption = Math.Max(currentOption - 1, 0);
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                currentOption = Math.Min(currentOption + 1, options.Length - 1);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                
                if (currentOption == 0)
                {
                    if (menuIn == 0)
                    { AddUser(); }
                    else if (menuIn == 1)
                    { Blackjack(); }
                    else { Console.WriteLine("Invalid option"); }
                }
                else if (currentOption == 1)
                {
                    if (menuIn == 0)
                    { ModifyUser(); }
                    else if (menuIn == 1)
                    { Slotmachine(); }
                    else { Console.WriteLine("Invalid option"); }
                }
                else if (currentOption == 2)
                {
                    if (menuIn == 0)
                    { RemoveUser(); }
                    else if (menuIn == 1)
                    { Memory(); }
                    else { Console.WriteLine("Invalid option"); }
                }
                else if (currentOption == 3)
                {
                    if (menuIn == 0)
                    { LoginUser(); }
                    else if (menuIn == 1)
                    { LogoffUser(); }
                    else { Console.WriteLine("Invalid option"); }
                }
                else
                {
                    Console.WriteLine("Not a valid option.");
                }
            }
        }
    }

    // Method to load users from user file into a list, if file doesn't exist, it creates the file.
    public static bool LoadUsers()
    {
        bool fileExists = false;
        if (!File.Exists(filePath))
        {
            Console.WriteLine("User file not found");
            CreateUserFile();
            fileExists = false;
        }
        else
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                User user = new User(parts[0], parts[1]);
                users.Add(user);
            }
            fileExists = true;
        }
        return fileExists;
    }

    // Method to save new users to the user file
    static void SaveUsers()
    {
        List<string> lines = new List<string>();
        foreach (User user in users)
        {
            lines.Add(user.Name + "," + user.Password);
        }
        File.WriteAllLines(filePath, lines);
    }

    // Method for testing purposes to display the users that are actif in the user file
    public static void DisplayUsers()
    {
        foreach (User user in users)
        {
            Console.WriteLine($"Naam: {user.Name}, Paswoord:{user.Password}");
        }
    }

    // Method to Add a user account
    public static void AddUser()
    {

        string username = "";
        string password = "";
        string encryptedPassword = "";
        
        bool usernameAvailable = false;
        do
        {
            username = Input.AskUsername("Enter a username (may only contain letters and numbers!): ");
        }
        while (username == "");
        Console.WriteLine("Checking it's availability...");
        (usernameAvailable, signedInUser) = UsernameAvailable(username);
        if (usernameAvailable)
        {
            do
            {
                password = Input.AskPassword($"Username: {username} is available, enter your password: ");

            } while (password == "");
            Console.WriteLine("Password is valid");
            encryptedPassword = EncryptPwd(password);
            //testcode
            //Console.WriteLine(encryptedPassword);
            User newUser = new User(username, encryptedPassword);
            users.Add(newUser);
            SaveUsers();
            //LoadUsers();

        }
        else
        {
            Console.WriteLine($"Username: {username} is not available.");
            AddUser();
        }

    }
    // Method to modify a user account (username or password)
    public static void ModifyUser()
    {
        string username = "";
        string newUsername = "";
        string enteredPassword = "";
        string password = "";
        string newPassword = "";
        string encryptedPassword = "";

        bool usernameAvailable = false;
        
        do
        {
            username = Input.AskString("Enter your username: ");
        }
        while (username == "");
        Console.WriteLine("Checking if username exists...");
        (usernameAvailable, signedInUser) = UsernameAvailable(username);
        if (!usernameAvailable)
        {
            do
            {
                enteredPassword = Input.AskString($"Enter your password: ");

            } while (enteredPassword == "");
            // testcode
            // Console.WriteLine($"Saved hash user: {signedInUser.Password}");

            bool validLogin = DecryptPwd(signedInUser.Password, enteredPassword);
            if (validLogin)
            {
                string modifyAnswer = Input.AskString("What do you want to change, enter (U) for Username, (P) for Password: ");
                
                if (modifyAnswer.ToLower() == "u")
                {
                    usernameAvailable = false;
                    do
                    {
                        newUsername = Input.AskUsername("Enter the username you want to modify: ");
                    }
                    while (newUsername == "");
                    Console.WriteLine("Checking if username exists...");
                    (usernameAvailable, signedInUser) = UsernameAvailable(newUsername);
                    if (usernameAvailable)
                    {

                        Console.WriteLine("Modifying username");
                        User newUser = new User(newUsername, EncryptPwd(enteredPassword));
                        users.RemoveAll(u => u.Name == username);
                        users.Add(newUser);
                        SaveUsers();
                        
                        
                        
                    }else { Console.WriteLine("Username unavailable."); ShowMenu(mainMenu); }
                }
                else if (modifyAnswer.ToLower() == "p")
                {
                    do
                    {
                        newPassword = Input.AskPassword($"Enter your password: ");

                    } while (newPassword == "");
                    Console.WriteLine("Password is valid");
                    encryptedPassword = EncryptPwd(newPassword);
                    //test code
                    //Console.WriteLine(encryptedPassword);
                    
                    signedInUser.Password=encryptedPassword;
                    User newUser = new User(signedInUser.Name, encryptedPassword);
                    users.Remove(signedInUser);
                    users.Add(newUser);
                    SaveUsers();
                    
 
                }
                else { Console.WriteLine("Invalid option.");ShowMenu(mainMenu); }
            }
            else
            {
                Console.WriteLine("Unknown username.");
                System.Threading.Thread.Sleep(3000);
                ShowMenu(mainMenu);
            }
        }

    }
    // Method to remove a user account
    public static void RemoveUser()
    {
        string username = "";
        string enteredPassword = "";

        bool usernameAvailable = false;
        User tempUser;

        do
        {
            username = Input.AskString("Enter your username: ");
        }
        while (username == "");
        Console.WriteLine("Checking if username exists...");
        (usernameAvailable, signedInUser) = UsernameAvailable(username);
        if (!usernameAvailable)
        {
            do
            {
                enteredPassword = Input.AskString($"Enter your password: ");

            } while (enteredPassword == "");
            // testcode
            // Console.WriteLine($"Saved hash user: {signedInUser.Password}");

            bool validLogin = DecryptPwd(signedInUser.Password, enteredPassword);
            if (validLogin)
            {
                Console.WriteLine($"User account: {username} will be removed.");
                users.RemoveAll(u => u.Name == username);
                SaveUsers();
                //LoadUsers();
                ShowMenu(mainMenu);
            }
            else
            {
                Console.WriteLine("Dementia, hacker or rheumatic fingers?? Try again");
                System.Threading.Thread.Sleep(3000);
                ShowMenu(mainMenu);
            }
        }else
        {
            Console.WriteLine("Unknown username.");
            System.Threading.Thread.Sleep(3000);
            ShowMenu(mainMenu);
        }

    }
    // Method to login a user into the application (validation of user/password combo)
    public static void LoginUser()
    {
        string username = "";
        string enteredPassword = "";

        bool usernameAvailable = false;
        
        do
        {
            username = Input.AskString("Enter your username: ");
        }
        while (username == "");
        Console.WriteLine("Checking if username exists...");
        (usernameAvailable, signedInUser) = UsernameAvailable(username);
            if (!usernameAvailable)
            {
                do
                {
                    enteredPassword = Input.AskString($"Enter your password: ");

                } while (enteredPassword == "");
                // testcode
                // Console.WriteLine($"Saved hash user: {signedInUser.Password}");

                bool validLogin = DecryptPwd(signedInUser.Password, enteredPassword);
                if (validLogin)
                {
                    signedInUser.SetSaldo(200);
                    signedInUser.SetLoginTime(DateTime.Now);
                    ShowMenu(gameMenu);
                }
                else
                {
                    Console.WriteLine("Dementia, hacker or rheumatic fingers?? Try again");
                    System.Threading.Thread.Sleep(3000);
                    ShowMenu(mainMenu);
                }
            }else
            {
            Console.WriteLine("Unknown username.");
            System.Threading.Thread.Sleep(3000);
            ShowMenu(mainMenu);
            }
    }
    // Method to verify if a given username exists already (if not can be used for adduser method, if it exist can be used to login)
    public static (bool, User) UsernameAvailable(string usernameIn)
    {
        bool validUsername = true;
        User userFound = new User();
        foreach (User user in users)
        {
            if (user.Name.ToLower() == usernameIn.ToLower())
            {
                validUsername = false;
                userFound = user;
                break;
            }
        }

        return (validUsername, userFound);
    }

    // Password encryption method with use of hashing, salt and cycling
    private static string EncryptPwd(string passwordIn)
    {
        // Create a new salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

        // Create the Rfc2898DeriveBytes and get the hash value
        var pbkdf2 = new Rfc2898DeriveBytes(passwordIn, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine the salt and password bytes for later use
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        // Convert the combined salt+password to a string for storage
        string savedPasswordHash = Convert.ToBase64String(hashBytes);

        return savedPasswordHash;
    }

    // Password decryption method for above mentionned encryption method
    private static bool DecryptPwd(string savedPasswordHashIn, string enteredPasswordIn)
    {
        // Get the stored password hash and salt
        string savedPasswordHash = savedPasswordHashIn;
        byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Get the password entered by the user
        string enteredPassword = enteredPasswordIn;

        // Create a new Rfc2898DeriveBytes with the entered password and the stored salt
        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
        byte[] newHash = pbkdf2.GetBytes(20);

        // Compare the new hash with the stored hash
        bool isPasswordCorrect = true;
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != newHash[i])
            {
                isPasswordCorrect = false;
                break;
            }
        }

        return isPasswordCorrect;
    }

    // Method to create a userfile
    public static void CreateUserFile()
    {
        // string dirName = "ProjectWeekWard";
        // string fileName = "Users.txt";
        // string directoryPath = Combine(GetTemPath(), dirName);
        // string filePath = Combine(directoryPath,fileName);

        Console.WriteLine($"User database Users.txt: will be available in {filePath}");
        try
        {
            Directory.CreateDirectory(directoryPath);

        }
        catch (IOException ex)
        {

            Console.WriteLine(ex.Message);
        }

        if (!File.Exists(filePath))
        {
            try
            {
                using(File.Create(filePath));
            }
            catch (IOException ex)
            {
                //Uncomment for debugging (no sensitive info to user) 
                //Console.WriteLine(ex.Message);
            }

        }
        else
        {
            // Handling situation if the file already exists
            Console.WriteLine("File already exists.Nothing to do here");
        }

    }

    // Method to Launch a blackjack game
    public static void Blackjack()
    {
        string initializationStatus = "";
        string playerGameStatus = "";
        string dealerGameStatus = "";

        BlackjackGame newGame = new BlackjackGame();
        initializationStatus = newGame.InitializeBJGame();
        if(initializationStatus=="21")
        {
            SharedGameFunctions.Win(newGame.GameWin);
        }
        
        while(!(playerGameStatus == "dead" || playerGameStatus == "stands") )
        {
            playerGameStatus = newGame.askPlayerHitOrStand();
        }
        if(playerGameStatus == "dead")
        {
            SharedGameFunctions.Loose();
        }
        else if(playerGameStatus == "stands")
        {
            dealerGameStatus = newGame.DealerPlays();
            if(!(dealerGameStatus=="dead"))
            {
                if(currentValueHandPlayer>currentValueHandDealer)
                {
                    SharedGameFunctions.Win(newGame.GameWin);
                }
                else if(currentValueHandPlayer==currentValueHandDealer)
                {
                    SharedGameFunctions.Draw(newGame.GameDraw);
                }
                else
                {
                    SharedGameFunctions.Loose();
                }
            }
            else { Console.WriteLine($"{newGame.dealer.Name} is dead, you win!"); SharedGameFunctions.Win(newGame.GameWin); }

        }

    }
    // Method to launch a slot machine game
    public static void Slotmachine()
    {
        int gameStake = -5;
        string gameName = "slot machine";

        // Pay game, if can't be payed return to game menu
        SharedGameFunctions.PayGame(gameStake,gameName);

        // 2D array with 3 columns and 3 rows to represent the slot machine
        string[,] slotMachine = new string[3, 3];

        // Fill the array with random icons
        List<string> slotIcons = new List<string> { "\u0001", "\u2660", "\u2663", "\u2666", "\u2665", "A", "\u0037" };
       // Dictionary to map slotIcons to prize to win for a row of that icon
        Dictionary<string, int> prizeValues = new Dictionary<string, int>
        {
            {"\u0001", 3},
            {"\u2660", 5},
            {"\u2663", 7},
            {"\u2666", 10},
            {"\u2665", 20},
            {"A", 50},
            {"\u0037", 100},
        };
        
        //Print the slot machine
        Console.WriteLine("Slot Machine:");
        for (int i = 0; i < 3; i++)
        {
            Console.Write("[");
            
            for (int j = 0; j < 3; j++)
            {
                Console.Write(slotMachine[i, j] = slotIcons[gen.Next(slotIcons.Count)]);
                if(j==2){Console.Write("]");}else{}
            }
            
            Console.WriteLine();
        }
        

        // Check rows for matching icons
        for (int i = 0; i < 3; i++)
        {
            if (slotMachine[i, 0] == slotMachine[i, 1] && slotMachine[i, 1] == slotMachine[i, 2])
            {
                Console.WriteLine("You won! Row " + (i + 1) + " has matching icons.");
                int prize = prizeValues[slotMachine[i, 0]];
                Console.WriteLine("You won " + prize + " dollars");
                SharedGameFunctions.Win(prize);
                ShowMenu(gameMenu);
            }
        }

        // Check columns for matching icons
        for (int i = 0; i < 3; i++)
        {
            if (slotMachine[0, i] == slotMachine[1, i] && slotMachine[1, i] == slotMachine[2, i])
            {
                Console.WriteLine("You won! Column " + (i + 1) + " has matching icons.");
                int prize = prizeValues[slotMachine[0, i]];
                Console.WriteLine("You won " + prize + " dollars");
                SharedGameFunctions.Win(prize);
                ShowMenu(gameMenu);
                   
            }
            
        }
        SharedGameFunctions.Loose();
    }
    
    // Method to launch a memory game
    public static void Memory() 
    {
        int gameStake = -20;
        int gameWin = 30;
        int correctAnswers = 0;
        string gameName = "memory";

        // Pay game, if can't be payed return to game menu
        SharedGameFunctions.PayGame(gameStake,gameName);

        // Create list of unique icons
        List<string> memoryIcons = new List<string> { "\u2660", "\u2663", "\u2666", "\u2665", "A" };

        // Create a new list to store icons, every icon appears two times
        List<string> iconList = new List<string>();
        foreach(string icon in memoryIcons)
        {
            iconList.Add(icon);
            iconList.Add(icon);
        }

        // Randomly shuffle the list
        iconList = iconList.OrderBy(x => gen.Next()).ToList();

        // Store current cursor position
        int originalLeft = Console.CursorLeft;
        int originalTop = Console.CursorTop;

        // Display icons
        foreach (string icon in iconList)
        {
            Console.Write("[" + icon + "] ");
        }
        
        // Wait 5 seconds so user can memorize the icon line (increment if needed to give player more time to memorize)
        Thread.Sleep(5000);

        Console.SetCursorPosition(originalLeft, originalTop);
        
        // Clear the last printed line by overwriting the data after the saved cursorposition by an empty line
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(originalLeft, originalTop);

        // Print a number row to bind icon to a number so player can use numbers to refer to an icon
        // Reveal first 4 items from the icon list, other icons are to be guessed (printed with a '?'-sign)
        Console.WriteLine("Enter the correct position of the other icon pairs and win this game!");
        for (int i = 0; i < iconList.Count; i++)
        {
            Console.Write("[" + i + "] ");
        }
        Console.WriteLine();

        for (int i = 0; i < iconList.Count; i++)
        {
            if (i > 3) { Console.Write("[?] "); }
            else { Console.Write("[" + iconList[i] + "] "); }
        }
        Console.WriteLine();
        
        // Ask user to reproduce the pairs
        for(int i = 0; i < iconList.Count; i++)
        {
            int playerInput = Input.AskInteger("Kaart " + i + ": ");
            if (iconList[i] == iconList[playerInput])
            { correctAnswers++; }
            else { SharedGameFunctions.Loose(); }

        }
        if (correctAnswers == iconList.Count)
        { SharedGameFunctions.Win(gameWin); }else { SharedGameFunctions.Loose(); };
    }

    // Method to log off from the application
    public static void LogoffUser()
    {
        Environment.Exit(0);
    }

    

    // Classes

    // General User class
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; } 
        public int Saldo { get; private set; } = 0;
        public DateTime LoginTime { get; private set; }
        

        public void SetSaldo(int valueIn)
        {
            Saldo += valueIn;
        }
        public void SetLoginTime(DateTime o)
        {
            LoginTime = o;
        }

        public void CalculateOnlineTime()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan OnlineTime = currentTime - LoginTime;
            Console.WriteLine($"Online time: {OnlineTime.ToString()}");
        }
        public User() { }
        public User(string userNameIn, string paswoordIn)
        {
            Name = userNameIn;
            Password = paswoordIn;
        }
    }

    // Card class only for blackjack game
    public class Card
    {
        public string CardNumber { get; set; }
        public CardSuiteEn CardSuite { get; set; }

        public Card() { }
        public Card(CardSuiteEn cardSuiteIn, string cardNumberIn)
        {
            CardSuite = cardSuiteIn;
            CardNumber = cardNumberIn;
        }
        public void printCard()
        {
            Dictionary<CardSuiteEn, string> cardSuiteIcons = new Dictionary<CardSuiteEn, string>
                {
                    { CardSuiteEn.Spades, "\u2660" },
                    { CardSuiteEn.Hearts, "\u2665" },
                    { CardSuiteEn.Diamonds, "\u2666" },
                    { CardSuiteEn.Clubs, "\u2663" }
                };

            Console.Write("("+this.CardNumber + "-" + cardSuiteIcons[this.CardSuite] + "),");
            Console.WriteLine();
        }
    }
    // Deck class only for blackjack game
    public class Deck
    {
        public List<Card> aDeck { get; private set; } = new List<Card>();

        public void FillDeck()
        {
            //int totalCards = 52;
            List<string> cardNumbers = new List<string>{"A","2", "3", "4", "5", "6", "7", "8", "9", "10",
            "J", "Q", "K"};

            // Fill deck with cards
            foreach (CardSuiteEn suite in Enum.GetValues(typeof(CardSuiteEn)))
            {
                foreach (string cardNumber in cardNumbers)
                {
                    aDeck.Add(new Card(suite, cardNumber));
                }
            }
        }
        
        public Card DealCard()
        {
            Card dealedCard = new Card();
            
            int nummer = gen.Next(0, aDeck.Count);
            dealedCard = aDeck[nummer];
            aDeck.RemoveAt(nummer);
            
            return dealedCard;
        }
        public void PrintDeck()
        {
            foreach (Card card in aDeck)
            {
                card.printCard();
            }
            Console.WriteLine();
        }
    }

    // Player class only for blackjack game
    public class Player : User
    {
        public bool SaldoPositive { get; private set; } = false;
        public bool WinGame { get; private set; } = false;
        public List<Card> playerHand = new List<Card>();
        public void printPlayerHand()
        {
            foreach(Card card in playerHand)
            {
                card.printCard();
            }
            
        }
        public int CalculateValueHand() 
        {
            int handValue = 0;
            int aceCount = 0;

            foreach (Card card in playerHand)
            {
                
                if (card.CardNumber == "A")
                {
                    aceCount++;
                    handValue += 11;
                }
                else if (card.CardNumber == "10" || card.CardNumber == "J" || card.CardNumber == "Q" || card.CardNumber == "K")
                {
                    handValue += 10;
                }
                else
                {
                    handValue += int.Parse(card.CardNumber);
                }
            }

            while (handValue > 21 && aceCount > 0)
            {
                handValue -= 10;
                aceCount--;
            }

            return handValue;

        }
        
        public bool CalculateSaldo(int gainIn)
        {
            
            this.SetSaldo(gainIn);
            if(Saldo<0)
            {
                this.SetSaldo(0);
                SaldoPositive = false;
            }
            else
            {
                SaldoPositive = true;
            }
            return SaldoPositive;
        } 

    }
    // Game class for Blackjack
    public class BlackjackGame
    {
        // Variables
        public int GameStake { get; } = -10;
        public int GameWin { get; } = 20;
        public int GameDraw { get; } = 10;
        public string GameName { get; } = "blackjack";
        public int InitialDeal { get; } = 2;
        public bool SaldoPostiveInitBJGame { get; set; } = false;
        public bool BjInitialized { get; set; } = false;
        public Player dealer { get; set; }
        public Player player{ get; set; }
        public Deck myDeck{ get; set; }

        public string InitializeBJGame()
        {
            // Pay game
            SharedGameFunctions.PayGame(GameStake, GameName);

            // Create a deck object myDeck
            myDeck = new Deck();
            myDeck.FillDeck();

            // Make 2 players: player and croupier
            dealer = new Player();
            dealer.Name = "Croupier";
            player = new Player();

            // Set player name to name property of the User object that logged into the application
            player.Name = signedInUser.Name;

            // Variable to know of initial deal was 21 or not
            string statusAfterDeal = "";

            // Game can be launched
            Console.WriteLine($"Dealing cards to {player.Name} and {dealer.Name}");
            for (int i = 0; i < InitialDeal;i++)
            {
                player.playerHand.Add(myDeck.DealCard());
                dealer.playerHand.Add(myDeck.DealCard());
            }

            // Print player hand
            Console.WriteLine($"Player: {player.Name}'s hand contains following cards: ");
            player.printPlayerHand();
            // Calculate player hand
            currentValueHandPlayer = player.CalculateValueHand();
            if (currentValueHandPlayer == 21)
            {
                statusAfterDeal = "21";
            }

            Console.WriteLine($"Player's hand with a total value of: {currentValueHandPlayer}.");

            return statusAfterDeal; //21 Win game or empty, game continues
        
        }
        
        public string askPlayerHitOrStand()
        {
            // Ask to hit (take card), or stand (don't take card)
            string playerAnswer = Input.AskString("Hit(H) or Stand(S)?");
            string status = "";
            if(playerAnswer.ToLower() =="h")
            {
                player.playerHand.Add(myDeck.DealCard());
                player.printPlayerHand();
                currentValueHandPlayer = player.CalculateValueHand();
                if(currentValueHandPlayer==21)
                {
                    status = "21";

                }else if (currentValueHandPlayer>21)
                {
                    status = "dead";
                }
                else { Console.WriteLine("Game continues,no winner yet."); }


                Console.WriteLine($"Player's hand with a total value of: {currentValueHandPlayer}.");

            }else if (playerAnswer.ToLower() == "s")
            {
                Console.WriteLine($"Player {player.Name} stands.");
                status = "stands";
            }else
            { Console.WriteLine("Invalid input.");askPlayerHitOrStand(); }

            return status;
        }

        public string DealerPlays()
        {
            Console.WriteLine($"{dealer.Name}'s turn.");
            Console.WriteLine($"{dealer.Name}'s hand contains following cards:");
            dealer.printPlayerHand();
            currentValueHandDealer = dealer.CalculateValueHand();
            Console.WriteLine($"{dealer.Name}'s hand with a total value of: {currentValueHandDealer}.");
            string status = "";
            do
            {
                Console.WriteLine("Dealer takes a card:");
                dealer.playerHand.Add(myDeck.DealCard());
                Console.WriteLine($"{dealer.Name}'s hand contains following cards:");
                dealer.printPlayerHand();
                currentValueHandDealer = dealer.CalculateValueHand();
                Console.WriteLine($"{dealer.Name}'s hand with a total value of: {currentValueHandDealer}.");
            } while (currentValueHandDealer < 17);
            if(currentValueHandDealer>21)
            {
                status = "dead";
            }
            else{}

            return status;

        }
      
    }

    // Static classes
    
    // Input class for input validation
    public static class Input
    {
        public static string AskString(string question)
        {
            Console.Write($"{question}");
            return Console.ReadLine() ?? string.Empty;
        }

        public static int AskInteger(string question)
        {
            while(true)
            {
                Console.Write($"{question}");
                if(int.TryParse(Console.ReadLine(), out int r))
                    return r;
            }
        }

        public static string AskUsername(string question)
        {
            string pattern = @"^[a-zA-Z0-9]+$";
            Console.Write($"{question}");
            string username = Console.ReadLine() ?? string.Empty;
            if (username == "")
            {
                Console.WriteLine("You didn't enter anything.");
                return string.Empty;
            }
            else
            {
                if (Regex.IsMatch(username, pattern))
                {
                    Console.WriteLine("Username is valid");
                    System.Threading.Thread.Sleep(3000);
                    return username;
                }
                else
                {
                    Console.WriteLine("Username is not valid");

                    return string.Empty;
                }
            }
        }

        public static string AskPassword(string question)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,20}$";
            Console.Write($"{question}");
            string password = Console.ReadLine() ?? string.Empty;
            if (password == "")
            {
                Console.WriteLine("You didn't enter anything.");
                return string.Empty;
            }
            else
            {
                if (Regex.IsMatch(password, pattern))
                {
                    Console.WriteLine("Password is valid");
                    System.Threading.Thread.Sleep(3000);
                    return password;
                }
                else
                {
                    Console.WriteLine("Password is not valid");
                    return string.Empty;
                }
            }
        }
    }

    // Shared Game functions used by all games: pay for the game, return win, loose, draw status to the played games
    public static class SharedGameFunctions
    {
        public static void PayGame(int gameStakeIn, string gameNameIn)
        {
            Console.WriteLine($"Welcome {signedInUser.Name}, play with our {gameNameIn} game and hopefully win some cash! Good Luck!");
            Console.WriteLine($"Saldo before paying {gameNameIn}: {signedInUser.Saldo}.");
            if (signedInUser.Saldo < gameStakeIn)
            {
                 Console.WriteLine("Not enough money to play, refill your wallet by doing a log off/log in to get 200 euro's");
                ShowMenu(gameMenu);
            }
            else
            {
                signedInUser.SetSaldo(gameStakeIn);
                Console.WriteLine($"Current saldo after paying {gameStakeIn} euro for a {gameNameIn} game: {signedInUser.Saldo} euro");
            }

        }
        public static void Win(int gameWinIn)
        {
            signedInUser.SetSaldo(gameWinIn);
            Console.WriteLine($"Player: {signedInUser.Name} you won the game, your benefit: ({gameWinIn}), new saldo: {signedInUser.Saldo}");
            System.Threading.Thread.Sleep(5000);
            ShowMenu(gameMenu);
        }

        public static void Loose()
        {
            
            Console.WriteLine($"Player: {signedInUser.Name} you lost the game. Saldo: {signedInUser.Saldo}");
            System.Threading.Thread.Sleep(5000);
            ShowMenu(gameMenu);
        }

        public static void  Draw(int gameDrawIn)
        {
            signedInUser.SetSaldo(gameDrawIn);
            Console.WriteLine($"Player: {signedInUser.Name} draw, your benefit: ({gameDrawIn}), new saldo: {signedInUser.Saldo}");
            System.Threading.Thread.Sleep(5000);
            ShowMenu(gameMenu);
        }
    }
}

