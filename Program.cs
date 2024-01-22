using System;
using System.Collections.Generic;


//Hocam bu kısmı görüyosanız kod çalışana kadar ortalık çok karşıtı
//Bu sebeple kodda gereksiz fazlalık kodlar ve satırlar var
//sonradan yanlış bi yerleri silip bozarım diye ellemedim 
class Room
{
    protected List<Item> items;
    private bool clueCollected;
    private bool explored;

    public Room()
    {
        this.items = new List<Item>();
        this.clueCollected = false;
        this.explored = false;
    }

    public virtual void ExploreRoom(List<Item> playerInventory)
    {
        if (!explored)
        {
            explored = true;
            CollectItems(playerInventory);
        }
        else
        {
            Console.WriteLine("You've already explored this room.");
        }
    }

    private void CollectItems(List<Item> playerInventory)
    {
        if (!clueCollected && playerInventory.Count == 0)
        {
            clueCollected = true;
            items.Add(new Item("a piece of paper with a clue written on it"));
        }
    }

    public bool HasClue()
    {
        return clueCollected;
    }

    public List<Item> GetItems()
    {
        return items;
    }

    public virtual Npc GetNpc()
    {
        return null;
    }
}

class NpcRoom : Room
{
    private Npc npc;

    public NpcRoom() : base()
    {
        this.npc = new Npc();
    }

    public override Npc GetNpc()
    {
        return npc;
    }

    public void PlaceNpc()
    {
        Console.WriteLine("You sense a presence in the room...");
        Console.WriteLine("You encounter an NPC. He asks you a riddle: " + npc.GetRiddle());
    }
}

class Npc
{
    private string riddle;
    private string[] answerOptions;
    private string correctAnswer;
    private bool answerVisible;

    public Npc()
    {
        this.riddle = "I speak without a mouth and hear without ears. I have no body, but I come alive with the wind. What am I?";
        this.answerOptions = new string[] { "a river", "an echo", "a tree", "a mountain", "a shadow" };
        this.correctAnswer = "an echo";
        this.answerVisible = false;
    }

    public string GetRiddle()
    {
        if (answerVisible)
        {
            return $"{riddle}\nCorrect answer: {correctAnswer}";
        }
        else
        {
            return riddle;
        }
    }

    public string[] GetAnswerOptions(bool hasClue)
    {
        if (hasClue)
        {
            return answerOptions;
        }
        else
        {
            List<string> optionsWithoutCorrectAnswer = new List<string>(answerOptions);
            optionsWithoutCorrectAnswer.Remove(correctAnswer);
            return optionsWithoutCorrectAnswer.ToArray();
        }
    }

    public bool CheckAnswer(string playerAnswer)
    {
        return playerAnswer.ToLower() == correctAnswer.ToLower();
    }

    public void MakeAnswerVisible()
    {
        answerVisible = true;
    }

    public bool IsAnswerVisible()
    {
        return answerVisible;
    }
}

class Item
{
    private string name;

    public Item(string name)
    {
        this.name = name;
    }

    public string GetName()
    {
        return name;
    }

    public void PickUp()
    {
        Console.WriteLine($"{name} is now in your inventory.");
    }
}

class TextAdventureGame
{
    static void Main()
    {
        PlayGame();
    }

    static void PlayGame()
    {
        Room[,] map = CreateMap();
        int playerX = 3;
        int playerY = 3;
        List<Item> playerInventory = new List<Item>();
        int chancesLeft = 2;
        bool clueMessageDisplayed = false;

        while (true)
        {
            map[playerX, playerY].ExploreRoom(playerInventory);
            PrintPlayerPosition(playerX, playerY);

            if (map[playerX, playerY] is NpcRoom && map[playerX, playerY].HasClue())
            {
                NpcRoom currentNpcRoom = (NpcRoom)map[playerX, playerY];
                currentNpcRoom.PlaceNpc();

                Npc npc = currentNpcRoom.GetNpc();
                Console.WriteLine($"You encounter an NPC. He asks you a riddle: {npc.GetRiddle()}");

                string[] answerOptions;
                if (clueMessageDisplayed)
                {
                    answerOptions = npc.GetAnswerOptions(true);
                }
                else
                {
                    answerOptions = npc.GetAnswerOptions(false);
                }

                for (int i = 0; i < answerOptions.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {answerOptions[i]}");
                }

                Console.Write("Your answer (enter the number): ");
                int playerChoice;
                if (int.TryParse(Console.ReadLine(), out playerChoice) && playerChoice >= 1 && playerChoice <= answerOptions.Length)
                {
                    string playerAnswer = answerOptions[playerChoice - 1];

                    if (npc.CheckAnswer(playerAnswer))
                    {
                        Console.WriteLine("Congratulations! You solved the riddle and gave the correct answer. You win!");
                        break;
                    }
                    else
                    {
                        chancesLeft--;
                        Console.WriteLine($"Wrong answer. You have {chancesLeft} chances left.");
                        if (chancesLeft == 0)
                        {
                            Console.WriteLine("You couldn't solve the riddle. The NPC kills you. Game over!");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Try again!");
                }
            }

            if (playerX == 6 && playerY == 6 && map[playerX, playerY] is RoomWithClue && !clueMessageDisplayed)
            {
                RoomWithClue roomWithClue = (RoomWithClue)map[playerX, playerY];
                roomWithClue.DisplayClueMessage();
                clueMessageDisplayed = true;
            }

            Console.Write("Enter a command (N, S, E, W, Q to quit): ");
            string command = Console.ReadLine().ToUpper();

            if (command == "Q")
            {
                Console.WriteLine("Thanks for playing. Goodbye!");
                break;
            }

            switch (command)
            {
                case "N":
                    if (playerX > 0)
                        playerX--;
                    break;
                case "S":
                    if (playerX < map.GetLength(0) - 1)
                        playerX++;
                    break;
                case "E":
                    if (playerY < map.GetLength(1) - 1)
                        playerY++;
                    break;
                case "W":
                    if (playerY > 0)
                        playerY--;
                    break;
                default:
                    Console.WriteLine("Invalid command. Try again.");
                    continue;
            }
        }
    }

    static Room[,] CreateMap()
    {
        Room[,] map = new Room[7, 7];

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                map[i, j] = new Room();
            }
        }

        map[6, 6] = new RoomWithClue();
        map[0, 0] = new NpcRoom();

        return map;
    }

    static void PrintPlayerPosition(int x, int y)
    {
        Console.WriteLine($"Current Position: ({x}, {y})");
    }
}

class RoomWithClue : Room
{
    public override void ExploreRoom(List<Item> playerInventory)
    {
        if (playerInventory.Count == 0)
        {
            base.ExploreRoom(playerInventory);
        }
        else
        {
            Console.WriteLine("You've already explored this room.");
        }
    }

    public void DisplayClueMessage()
    {
        Console.WriteLine("U Find Clue its a note with \"an echo\" written on it");
    }
}
