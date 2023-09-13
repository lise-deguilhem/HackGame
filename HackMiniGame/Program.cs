using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    private static readonly string[] GARBAGE_CHARS = new string[] { "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "-", "=", "{", "}", "[", "]", "|", ";", ":", ",", ".", "<", ">", "?", "/" };

    static void Main()
    {
        Console.WriteLine("Hacking Minigame");
        Console.WriteLine("Recherchez le mot de passe dans la mémoire de l'ordinateur. Des indices vous sont donnés après chaque supposition.");
        Console.WriteLine("Par exemple, si le mot de passe secret est MONITOR mais que le joueur a deviné CONTAIN,");
        Console.WriteLine("vous serez informé que 2 lettres sur 7 étaient correctes,");
        Console.WriteLine("car MONITOR et CONTAIN ont tous deux les lettres O et N comme 2e et 3e lettres.");
        Console.WriteLine("Vous avez 4 tentatives.\n");
        Console.WriteLine("Appuyez sur ENTER pour démarrer...");
        Console.ReadLine();

        List<string> gameWords = GetWords();
        string computerMemory = GetComputerMemoryString(gameWords);
        string secretPassword = gameWords[0];

        Console.WriteLine(computerMemory);

        for (int triesRemaining = 4; triesRemaining > 0; triesRemaining--)
        {
            string playerMove = AskForPlayerGuess(gameWords, triesRemaining);

            if (playerMove == secretPassword)
            {
                Console.WriteLine("A C C E S   A U T O R I S E");
                return;
            }
            else
            {
                int numMatches = NumMatchingLetters(secretPassword, playerMove);
                Console.WriteLine($"Accés Refusé ({numMatches}/7 correct)");
            }
        }

        Console.WriteLine($"Trop de tentatives. Le mot de passe était {secretPassword}.");
    }

    static List<string> GetWords()
    {
        List<string> words = new List<string>();
        string[] allWords = File.ReadAllLines("sevenletterwords.txt").Select(word => word.Trim().ToUpper()).ToArray();
        string secretPassword = allWords[new Random().Next(allWords.Length)];
        words.Add(secretPassword);

        while (words.Count < 3)
        {
            string randomWord = GetOneWordExcept(words, allWords);
            if (NumMatchingLetters(secretPassword, randomWord) == 0)
            {
                words.Add(randomWord);
            }
        }

        int maxAttempts = 500;
        for (int i = 0; i < maxAttempts && words.Count < 5; i++)
        {
            string randomWord = GetOneWordExcept(words, allWords);
            if (NumMatchingLetters(secretPassword, randomWord) == 3)
            {
                words.Add(randomWord);
            }
        }

        maxAttempts = 500;
        for (int i = 0; i < maxAttempts && words.Count < 12; i++)
        {
            string randomWord = GetOneWordExcept(words, allWords);
            if (NumMatchingLetters(secretPassword, randomWord) > 0)
            {
                words.Add(randomWord);
            }
        }

        while (words.Count < 12)
        {
            string randomWord = GetOneWordExcept(words, allWords);
            words.Add(randomWord);
        }

        return words;
    }

    static string GetOneWordExcept(List<string> blocklist, string[] allWords)
    {
        while (true)
        {
            string randomWord = allWords[new Random().Next(allWords.Length)];
            if (!blocklist.Contains(randomWord))
            {
                return randomWord;
            }
        }
    }

    static int NumMatchingLetters(string word1, string word2)
    {
        int matches = 0;
        for (int i = 0; i < word1.Length; i++)
        {
            if (word1[i] == word2[i])
            {
                matches++;
            }
        }
        return matches;
    }

    static string GetComputerMemoryString(List<string> words)
    {
        List<int> linesWithWords = Enumerable.Range(0, 16 * 2).OrderBy(x => Guid.NewGuid()).Take(words.Count).ToList();
        int memoryAddress = 16 * new Random().Next(4000);

        StringBuilder computerMemory = new StringBuilder();

        for (int lineNum = 0; lineNum < 16; lineNum++)
        {
            string leftHalf = new string(Enumerable.Repeat(GARBAGE_CHARS[new Random().Next(GARBAGE_CHARS.Length)], 16).ToArray());
            string rightHalf = new string(Enumerable.Repeat(GARBAGE_CHARS[new Random().Next(GARBAGE_CHARS.Length)], 16).ToArray());

            if (linesWithWords.Contains(lineNum))
            {
                int insertionIndex = new Random().Next(10);
                leftHalf = leftHalf.Remove(insertionIndex, 7).Insert(insertionIndex, words[0]);
            }
            if (linesWithWords.Contains(lineNum + 16))
            {
                int insertionIndex = new Random().Next(10);
                rightHalf = rightHalf.Remove(insertionIndex, 7).Insert(insertionIndex, words[0]);
            }

            computerMemory.AppendFormat("0x{0:X4}  {1}    0x{2:X4}  {3}", memoryAddress, leftHalf, memoryAddress + 16 * 16, rightHalf);
            computerMemory.AppendLine();
            memoryAddress += 16;
        }

        return computerMemory.ToString();
    }

    static string AskForPlayerGuess(List<string> words, int tries)
    {
        while (true)
        {
            Console.WriteLine($"Mot de passe: ({tries} tries remaining)");
            string guess = Console.ReadLine().ToUpper();
            if (words.Contains(guess))
            {
                return guess;
            }
            Console.WriteLine("Ce n'est pas l'un des mots de passe possibles répertoriés ci-dessus.");
            Console.WriteLine($"Essayer d'entrer \"{words[0]}\" or \"{words[1]}\".");
        }
    }
}
