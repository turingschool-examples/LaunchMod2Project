﻿// See https://aka.ms/new-console-template for more information
using MessageLogger.Data;
using MessageLogger.Models;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using (var context = new MessageLoggerContext())
{
    Console.WriteLine("Welcome to Message Logger!");
    RunProgram(context);
    ClosingInfo(context);
}

static string Format(string s)
{
    return s.ToLower().Replace(" ", "");
}

static void HelpMessage()
{
    Console.WriteLine();
    Console.WriteLine("To log out of your user profile, enter `log out`.");

    Console.WriteLine();
    Console.Write("Add a message (or `quit` to exit): ");
}

static void RunProgram(MessageLoggerContext context)
{
    User currentUser = LogIn(context);

    HelpMessage();
    string userInput = Console.ReadLine();
    if (currentUser == null) userInput = "quit";

    while (Format(userInput) != "quit")
    {
        while (Format(userInput) != "logout")
        {
            currentUser.Messages.Add(new Message(userInput));
            context.SaveChanges();

            currentUser.PrintMessages();

            Console.Write("Add a message: ");
            userInput = Console.ReadLine();
            Console.WriteLine();
        }
        currentUser = LogIn(context);
        if (currentUser != null)
        {
            Console.Write("Add a message: ");
            userInput = Console.ReadLine();
        }
        else
        {
            Console.WriteLine("could not find user");
            userInput = "quit";

        }
    }
}

static User NewUser(MessageLoggerContext context)
{
    Console.WriteLine();
    Console.WriteLine("Let's create a user pofile for you.");
    Console.Write("What is your name? ");
    string name = Console.ReadLine();
    Console.Write("What is your username? (one word, no spaces!) ");
    string username = Console.ReadLine();
    User currentUser = new User(name, username);
    context.Users.Add(currentUser);
    context.SaveChanges();
    return currentUser;
}

static void ClosingInfo(MessageLoggerContext context)
{
    Console.WriteLine("Thanks for using Message Logger!");
    Console.WriteLine();
    Console.WriteLine("Please enter a number to see some neat statistics:");
    Console.WriteLine("1: The number of messages each user has written");
    Console.WriteLine("2: Users ordered by the number of messages they have written");
    Console.WriteLine("3: The hour with the most messages written");
    Console.WriteLine("4: The most common word users wrote");
    Console.WriteLine("5: Quit");
    string userInput = Console.ReadLine();
    switch (userInput) 
    {
        case "1":
            MessageCountByUser(context); 
            break;
        case "2":
            UsersOrderedByMessageCount(context);
            break;
        case "3":
            HourWithMostMessages(context);
            break;
        case "4":
            MostCommonWord(context);
            break;
        default:
            Console.WriteLine("Goodbye"); 
            break;
    }
}

static User GetUser(MessageLoggerContext context)
{
    Console.Write("What is your username? ");
    string username = Console.ReadLine();
    User currentUser = null;
    foreach (var existingUser in context.Users)
    {
        if (existingUser.Username == username)
        {
            currentUser = existingUser;
        }
    }
    return currentUser;
}

static User LogIn(MessageLoggerContext context)
{
    Console.Write("Would you like to log in a `new` or `existing` user? Or, `quit`? ");
    string userInput = Console.ReadLine();
    if (Format(userInput) == "new")
    {
        return NewUser(context);
    }
    else if (Format(userInput) == "existing")
    {
        return GetUser(context);
    }
    else return null;
}

static void UsersOrderedByMessageCount(MessageLoggerContext context)
{
    var orderedUsers = context.Users.OrderByDescending
        (user => user.Messages.Count());
    foreach (var user in orderedUsers)
    {
        Console.WriteLine($"{user.Name}: {user.Messages.Count()} messages");
    }
}

static void HourWithMostMessages(MessageLoggerContext context)
{
    var hours = context.Messages.GroupBy(message => message.CreatedAt.ToLocalTime().Hour);
    int hourWithMost = 0;
    int messageCount = 0;
    foreach(var hour in hours)
    {
        if (hour.Count() > messageCount)
        {
            hourWithMost = hour.Key;
            messageCount = hour.Count();
        }
    }
    Console.WriteLine($"Hour {hourWithMost} had the most messsages with a count of {messageCount}");
}

static string AllWords(MessageLoggerContext context)
{
    string allWords = "";
    var characterList = new List<string> { " ", "!", "' ", "?", ";", ":", ".", "/" };
    foreach(var message in context.Messages)
    {
        allWords += message.Content + " ";
    }
    foreach(string character in characterList)
    {
        if (allWords.Contains(character))
        {
            allWords = allWords.Replace(character, ",");
        }
    }
    Console.WriteLine(allWords);
    return allWords;
}

static void MostCommonWord(MessageLoggerContext context)
{
    string allWords = AllWords(context);
    var split = allWords.Split(",");
    var wordCount = new Dictionary<string, int>();

    foreach(string word in split)
    {
        Console.WriteLine(word);
        string lowerWord = word.ToLower();
        if (string.IsNullOrEmpty(lowerWord))
        {
            continue;
        }
        if (!wordCount.ContainsKey(lowerWord))
        {
            wordCount.Add(lowerWord, 0);
        }
        wordCount[lowerWord]++;
    }
    var maxPair = wordCount.First(word => word.Value == wordCount.Max(word => word.Value));

    Console.WriteLine($"The most common word is {maxPair.Key} with {maxPair.Value} uses");
}

static void MessageCountByUser(MessageLoggerContext context)
{
    foreach (var u in context.Users)
    {
        Console.WriteLine($"{u.Name} has written {u.Messages.Count} messages.");
    }
}