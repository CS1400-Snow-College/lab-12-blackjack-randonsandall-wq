using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static string playerName = "";
    static decimal bank = 100.00m;
    static List<string> deck = new List<string>();
    static List<string> playerHand = new List<string>();
    static List<string> dealerHand = new List<string>();
    static decimal bet = 0;

    static void Main(string[] args)
    {
        Console.WriteLine("=== BLACKJACK ===");
        Console.WriteLine("What is your name?");
        playerName = Console.ReadLine();

        LoadPlayerData();
        Console.WriteLine($"Welcome {playerName}, your bank is ${bank:F0}");

        bool playing = true;
        while (playing)
        {
            if (bank <= 0)
            {
                Console.WriteLine("The casino has mercy on you and gives you $50.00");
                bank = 50;
            }

            Console.WriteLine("Enter your bet:");
            string betInput = Console.ReadLine();
            if (string.IsNullOrEmpty(betInput))
            {
                break;
            }

            bet = decimal.Parse(betInput);
            if (bet > bank)
            {
                Console.WriteLine("You don't have enough money!");
                continue;
            }

            PlayRound();
            SavePlayerData();

            Console.WriteLine("Play again?");
            Console.WriteLine();
        }
    }

    static void PlayRound()
    {
        CreateDeck();
        ShuffleDeck();

        playerHand.Clear();
        dealerHand.Clear();

        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());
        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());

        Console.WriteLine($"{playerName}'s hand: {GetHandString(playerHand)}  Total: {GetHandValue(playerHand)}");
        Console.WriteLine($"Dealer Shows:   {dealerHand[0]}[?]");

        if (GetHandValue(playerHand) == 21 && playerHand.Count == 2)
        {
            HandleBlackjack();
            return;
        }

        PlayerTurn();

        if (GetHandValue(playerHand) > 21)
        {
            Console.WriteLine("Dealer's hand:");
            Console.WriteLine($"Dealer:         {GetHandString(dealerHand)}    Total: {GetHandValue(dealerHand)}");
            Console.WriteLine();
            Console.WriteLine($"{playerName}'s total: {GetHandValue(playerHand)}..........Dealer total: {GetHandValue(dealerHand)}");
            Console.WriteLine($"Outcome = LOSE, you lost ${bet:F2}. New bank total: ${bank - bet:F0}");
            bank -= bet;
        }
        else
        {
            DealerTurn();
            DetermineWinner();
        }
    }

    static void PlayerTurn()
    {
        bool done = false;
        while (!done)
        {
            string prompt = "(H)it, (S)tand";
            if (playerHand.Count == 2 && bet * 2 <= bank)
            {
                prompt += ", (D)ouble";
            }
            prompt += "? ";
            Console.Write(prompt);

            string choice = Console.ReadLine().ToLower();

            if (choice == "h")
            {
                string card = DrawCard();
                playerHand.Add(card);
                Console.WriteLine($"You drew:  {card}");
                Console.WriteLine($"{playerName}'s hand:     {GetHandString(playerHand)}    Total: {GetHandValue(playerHand)}");

                if (GetHandValue(playerHand) > 21)
                {
                    done = true;
                }
            }
            else if (choice == "s")
            {
                done = true;
            }
            else if (choice == "d" && playerHand.Count == 2 && bet * 2 <= bank)
            {
                bet *= 2;
                string card = DrawCard();
                playerHand.Add(card);
                Console.WriteLine($"You doubled down and drew: {card}");
                Console.WriteLine($"{playerName}'s hand:     {GetHandString(playerHand)}    Total: {GetHandValue(playerHand)}");
                done = true;
            }
        }
    }

    static void DealerTurn()
    {
        Console.WriteLine("Dealer's hand:");
        Console.WriteLine($"Dealer:         {GetHandString(dealerHand)}    Total: {GetHandValue(dealerHand)}");

        while (GetHandValue(dealerHand) < 17)
        {
            string card = DrawCard();
            dealerHand.Add(card);
            Console.WriteLine($"Dealer draws: {card}");
            Console.WriteLine($"Dealer:         {GetHandString(dealerHand)}    Total: {GetHandValue(dealerHand)}");
        }
        Console.WriteLine();
    }

    static void DetermineWinner()
    {
        int playerValue = GetHandValue(playerHand);
        int dealerValue = GetHandValue(dealerHand);

        Console.WriteLine($"{playerName}'s total: {playerValue}..........Dealer total: {dealerValue}");

        if (dealerValue > 21)
        {
            Console.WriteLine($"Outcome = WIN, you won ${bet:F2}. New bank total: ${bank + bet:F0}");
            bank += bet;
        }
        else if (playerValue > dealerValue)
        {
            Console.WriteLine($"Outcome = WIN, you won ${bet:F2}. New bank total: ${bank + bet:F0}");
            bank += bet;
        }
        else if (playerValue < dealerValue)
        {
            Console.WriteLine($"Outcome = LOSE, you lost ${bet:F2}. New bank total: ${bank - bet:F0}");
            bank -= bet;
        }
        else
        {
            Console.WriteLine($"Outcome = PUSH, it's a tie. New bank total: ${bank:F0}");
        }
    }

    static void HandleBlackjack()
    {
        Console.Write("(H)it, (S)tand, (D)ouble? s");
        Console.WriteLine();
        Console.WriteLine("Dealer's Hand:");
        Console.WriteLine($"Dealer:              {GetHandString(dealerHand)}       Total: {GetHandValue(dealerHand)}");

        if (GetHandValue(dealerHand) == 21 && dealerHand.Count == 2)
        {
            Console.WriteLine($"\n{playerName}'s total: 21.........Dealer total: 21");
            Console.WriteLine($"Outcome = PUSH, it's a tie. New bank total: ${bank:F0}");
        }
        else
        {
            while (GetHandValue(dealerHand) < 17)
            {
                string card = DrawCard();
                dealerHand.Add(card);
                Console.WriteLine($"Dealer draws:  {card}");
                Console.WriteLine($"Dealer:              {GetHandString(dealerHand)}       Total: {GetHandValue(dealerHand)}");
            }

            decimal winAmount = bet * 1.5m;
            Console.WriteLine($"\n{playerName}'s total: 21.........Dealer total: {GetHandValue(dealerHand)}");
            Console.WriteLine($"Outcome = BLACKJACK, you win ${winAmount:F2}.  New bank total: ${bank + winAmount:F0}");
            bank += winAmount;
        }
    }

    static void CreateDeck()
    {
        deck.Clear();
        string[] suits = { "♠", "♥", "♦", "♣" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        // Create a deck with all 52 cards
        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                deck.Add($"[{suit}{rank}]");
            }
        }
    }

    static void ShuffleDeck()
    {
        Random rand = new Random();
        for (int i = deck.Count - 1; i >= 0; i--)
        {
            int j = rand.Next(i + 1);
            string temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    static string DrawCard()
    {
        if (deck.Count == 0)
        {
            CreateDeck();
            ShuffleDeck();
        }
        string card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    static int GetHandValue(List<string> hand)
    {
        int total = 0;
        int aces = 0;

        foreach (string card in hand)
        {
            string rank = card.Replace("[", "").Replace("]", "").Substring(1);

            if (rank == "A")
            {
                total += 11;
                aces++;
            }
            else if (rank == "J" || rank == "Q" || rank == "K")
            {
                total += 10;
            }
            else
            {
                total += int.Parse(rank);
            }
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    static string GetHandString(List<string> hand)
    {
        string result = "";
        foreach (string card in hand)
        {
            result += card;
        }
        return result;
    }

    static void LoadPlayerData()
    {
        string filename = $"PlayerData/{playerName}.txt";
        if (File.Exists(filename))
        {
            string data = File.ReadAllText(filename);
            bank = decimal.Parse(data);
        }
        else
        {
            bank = 100.00m;
        }
    }

    static void SavePlayerData()
    {
        if (!Directory.Exists("PlayerData"))
        {
            Directory.CreateDirectory("PlayerData");
        }
        string filename = $"PlayerData/{playerName}.txt";
        File.WriteAllText(filename, bank.ToString());
    }
}
