using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Emzi0767;

namespace DudesBot.Commands
{
    public class Hangman : BaseCommandModule
    {
        enum GameState
        {
            inProgress,
            Win,
            Lose
        }
        class HangmanGame
        {
            public ulong GameHost { get; }
            public string SolutionWord { get; }
            private string logicalSolutionWord;
            public char[] GuessWord { get; private set; }
            public int RemainingGuesses = 10;
            public List<string> WrongGuesses { get; }
            public GameState gameState { get; private set; }

            public HangmanGame(string word, ulong hostID)
            {
                gameState = GameState.inProgress;
                if (word.Length == 0) { throw new ArgumentException("fuck you"); }
                GameHost = hostID;
                WrongGuesses = new();
                SolutionWord = word;
                logicalSolutionWord = SolutionWord.ToLower();
                GuessWord = new char[word.Length];
                for (int i = 0; i < SolutionWord.Length; i++)
                {
                    if (SolutionWord[i].IsBasicLetter())
                    {
                        GuessWord[i] = '_';
                    }
                    else
                    {
                        GuessWord[i] = SolutionWord[i];
                    }
                }
            }

            public bool Guess(char guessChar)
            {
                guessChar = char.ToLower(guessChar);
                if (logicalSolutionWord.Contains(guessChar))
                {
                    for (int i = 0; i < SolutionWord.Length; i++)
                    {
                        if (logicalSolutionWord[i] == guessChar)
                        {
                            GuessWord[i] = guessChar;
                        }
                    }
                    if (logicalSolutionWord == new string(GuessWord))
                    {
                        gameState = GameState.Win;
                    }
                    return true;
                }
                WrongGuesses.Add(guessChar.ToString());
                RemainingGuesses--;
                if (RemainingGuesses <= 0)
                {
                    gameState = GameState.Lose;
                }
                return false;
            }

            public bool Guess(string guessString)
            {
                if (guessString.Trim().ToLower() == logicalSolutionWord.ToLower())
                {
                    GuessWord = SolutionWord.ToCharArray();
                    gameState = GameState.Win;
                    return true;
                }
                WrongGuesses.Add(guessString);
                RemainingGuesses--;
                if (RemainingGuesses <= 0)
                {
                    gameState = GameState.Lose;
                }
                return false;
            }

            public override string ToString()
            {
                string wrongGuessString = "";
                foreach (var guess in WrongGuesses)
                {
                    wrongGuessString += $"{guess}, ";
                }
                if (wrongGuessString.Length == 0) { wrongGuessString = "NONE"; }
                return $"**Game Info:**\nWord is `{SolutionWord}`\nGuessed word is `{new string(GuessWord)}`\nWrong guesses are `{wrongGuessString}`\n`{RemainingGuesses}` guesses remain\nGame state is `{gameState}`";
            }
        }

        static Dictionary<ulong, HangmanGame> ActiveGames = new();
        public static Dictionary<ulong, ulong> UserWaiting = new(); //userID,ChannelID

        public static async Task StartGame(DiscordClient client, ulong channelID, ulong userID, string word)
        {
            ActiveGames.Add(channelID, new HangmanGame(word, userID));
            var channel = await client.GetChannelAsync(channelID);
            //send game message
        }

        [Command("hangtest")]
        public async Task HangmanTest(CommandContext context, [RemainingText] string word)
        {
            var game = new HangmanGame(word, context.User.Id);
            if(ActiveGames.ContainsKey(context.Channel.Id))
            {
                await context.RespondAsync("There is already an active game in this channel");
                return;
            }
            ActiveGames.Add(context.Channel.Id, game);
            await context.RespondAsync(game.ToString());

        }

        [Command("hangman")]
        public async Task HangmanCommand(CommandContext context)
        {
            if(ActiveGames.ContainsKey(context.Channel.Id))
            {
                await context.RespondAsync("There is already an active game in this channel");
                return;                
            }
            UserWaiting.Add(context.User.Id, context.Channel.Id);
            
        }

        [Command("guess"), Aliases("g"), Priority(0)]
        public async Task GuessStringCommand(CommandContext context, [RemainingText] string guess)
        {
            if (ActiveGames.ContainsKey(context.Channel.Id))
            {
                var game = ActiveGames[context.Channel.Id];
                if(guess.Length == 1)
                {
                    game.Guess(guess[0]);
                }
                else
                {
                    game.Guess(guess);
                }
                
                await context.RespondAsync(game.ToString());

                if (game.gameState == GameState.Win || game.gameState == GameState.Lose)
                {
                    //Do win or lose stuff
                    ActiveGames.Remove(context.Channel.Id);
                }
            }
            else
            {
                await context.RespondAsync("There is no active game in this channel");
            }
        }
    }
}