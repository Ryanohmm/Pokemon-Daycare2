using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokemonDaycare
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Welcome to the Pokémon Daycare! ===");
            Console.Write("Which Pokémon would you like to leave with us? ");

            string pokemonName = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(pokemonName))
            {
                Console.WriteLine("You must enter a Pokémon name.");
                return;
            }

            try
            {
                var pokemon = await GetPokemonData(pokemonName);

                if (pokemon == null)
                {
                    Console.WriteLine("That Pokémon could not be found in the Daycare records.");
                    return;
                }

                Console.WriteLine($"\nGreat choice! {pokemonName.ToUpper()} has been checked in.");

                // Get species data (contains egg info)
                var species = await GetPokemonSpeciesData(pokemon.Species.Url);

                Console.WriteLine("\n--- Daycare Worker Report ---");

                Console.WriteLine($"Egg Groups: {string.Join(", ", species.EggGroups.Select(g => g.Name))}");
                Console.WriteLine($"Hatch Counter: {species.HatchCounter} cycles");

                // Hatch time calculation
                int stepsPerCycle = 257; // PokeAPI standard
                int totalSteps = species.HatchCounter * stepsPerCycle;

                Console.WriteLine($"Estimated Steps Until Egg Hatches: {totalSteps}");

                // Egg production time (fun mechanic)
                int minutesUntilEgg = species.HatchCounter * 3;
                Console.WriteLine($"Estimated Time Until Daycare Can Produce an Egg: {minutesUntilEgg} minutes");

                Console.WriteLine("\nYour Pokémon is in good hands!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred retrieving Pokémon data: {ex.Message}");
            }
        }

        // ---------------------------
        // API CALLS
        // ---------------------------

        static async Task<Pokemon?> GetPokemonData(string name)
        {
            using HttpClient client = new HttpClient();
            string url = $"https://pokeapi.co/api/v2/pokemon/{name}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Pokemon>(json);
        }

        static async Task<PokemonSpecies?> GetPokemonSpeciesData(string url)
        {
            using HttpClient client = new HttpClient();

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PokemonSpecies>(json);
        }
    }

    // ---------------------------
    // DATA MODELS
    // ---------------------------

    public class Pokemon
    {
        public SpeciesInfo Species { get; set; }
    }

    public class SpeciesInfo
    {
        public string Url { get; set; }
    }

    public class PokemonSpecies
    {
        public EggGroup[] EggGroups { get; set; }
        public int HatchCounter { get; set; }
    }

    public class EggGroup
    {
        public string Name { get; set; }
    }
}

