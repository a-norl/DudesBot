using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PokeApiNet;

namespace DudesBot.Commands
{
    public class PokemonCommands : BaseCommandModule
    {

        static readonly Dictionary<string, string> TypeToColour = new()
        {
            { "normal", "A8A77A" },
            { "fire", "EE8130" },
            { "water", "6390F0" },
            { "electric", "F7D02C" },
            { "grass", "7AC74C" },
            { "ice", "96D9D6" },
            { "fighting", "C22E28" },
            { "poison", "A33EA1" },
            { "ground", "E2BF65" },
            { "flying", "A98FF3" },
            { "psychic", "F95587" },
            { "bug", "A6B91A" },
            { "rock", "B6A136" },
            { "ghost", "735797" },
            { "dragon", "6F35FC" },
            { "dark", "705746" },
            { "steel", "B7B7CE" },
            { "fairy", "D685AD" },
        };
        public PokeApiClient pokeApiClient { private get; set; }

        [Command("pokemon"), Priority(1)]
        public async Task PokemonByName(CommandContext context, string pokemonName)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(pokemonName);
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, false);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        [Command("pokemon"), Priority(2)]
        public async Task PokemonByDex(CommandContext context, int dexNum)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(dexNum);
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, false);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        [Command("pokemon"), Priority(0)]
        public async Task PokemonRandom(CommandContext context)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(new Random().Next(932));
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, false);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        [Command("shinypokemon"), Priority(1)]
        public async Task ShinyPokemonByName(CommandContext context, string pokemonName)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(pokemonName);
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, true);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        [Command("shinypokemon"), Priority(2)]
        public async Task ShinyPokemonByDex(CommandContext context, int dexNum)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(dexNum);
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, true);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        [Command("shinypokemon"), Priority(0)]
        public async Task ShinyPokemonRandom(CommandContext context)
        {
            Pokemon requestedPokemon = await pokeApiClient.GetResourceAsync<Pokemon>(new Random().Next(932));
            DiscordEmbedBuilder pokemonEmbed = BuildPokemonEmbed(requestedPokemon, true);
            await context.Channel.SendMessageAsync(pokemonEmbed.Build());
        }

        private DiscordEmbedBuilder BuildPokemonEmbed(Pokemon inputPokemon, bool shiny)
        {
            DiscordEmbedBuilder embedBuilder = new();

            if (shiny)
            {
                embedBuilder.WithImageUrl($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/shiny/{inputPokemon.Id}.gif");
                if (inputPokemon.Id >= 650)
                {
                    embedBuilder.WithImageUrl($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/shiny/{inputPokemon.Id}.png");
                }
            }
            else
            {
                embedBuilder.WithImageUrl($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/animated/{inputPokemon.Id}.gif");
                if (inputPokemon.Id >= 650)
                {
                    embedBuilder.WithImageUrl($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-v/black-white/{inputPokemon.Id}.png");
                }
            }
            embedBuilder.WithTitle($"#{inputPokemon.Id}: {ToUpperFirstLetter(inputPokemon.Species.Name)}");
            List<PokemonType> inputPokemonType = inputPokemon.Types;
            embedBuilder.WithColor(new DiscordColor(TypeToColour[inputPokemonType[0].Type.Name]));
            string typeString = ToUpperFirstLetter(inputPokemonType[0].Type.Name);
            if (inputPokemonType.Count == 2) { typeString += $"/{ToUpperFirstLetter(inputPokemonType[1].Type.Name)}"; };
            embedBuilder.AddField("Type", typeString, true);
            List<PokemonAbility> abilities = inputPokemon.Abilities;
            string abilityString = "";
            foreach (PokemonAbility ability in abilities)
            {
                abilityString += $"{ToUpperFirstLetter(ability.Ability.Name)}\n";
            }
            embedBuilder.AddField("Abilities", abilityString, true);
            return embedBuilder;
        }
        public static string ToUpperFirstLetter(string source)
        {
            if (string.IsNullOrEmpty(source)) { return string.Empty; }
            char[] letters = source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

    }
}