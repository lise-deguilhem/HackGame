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
        Console.WriteLine("Hacking Minigame, by Al Sweigart al@inventwithpython.com");
        Console.WriteLine("Find the password in the computer's memory. You are given clues after each guess.");
        Console.WriteLine("For example, if the secret password is MONITOR but the player guessed CONTAIN,");
        Console.WriteLine("they are given the hint that 2 out of 7 letters were correct,");
        Console.WriteLine("because both MONITOR and CONTAIN have the letter O and N as their 2nd and 3rd letter.");
        Console.WriteLine("You get four guesses.\n");
        Console.WriteLine("Press Enter to begin...");
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
                Console.WriteLine("A C C E S S   G R A N T E D");
                return;
            }
            else
            {
                int numMatches = NumMatchingLetters(secretPassword, playerMove);
                Console.WriteLine($"Access Denied ({numMatches}/7 correct)");
            }
        }

        Console.WriteLine($"Out of tries. Secret password was {secretPassword}.");
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
            Console.WriteLine($"Enter password: ({tries} tries remaining)");
            string guess = Console.ReadLine().ToUpper();
            if (words.Contains(guess))
            {
                return guess;
            }
            Console.WriteLine("That is not one of the possible passwords listed above.");
            Console.WriteLine($"Try entering \"{words[0]}\" or \"{words[1]}\".");
        }
    }
}
