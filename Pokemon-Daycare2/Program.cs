using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PokemonDaycare
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Welcome to the Pokémon Daycare! ===");
            Console.Write("Which Pokémon would you like to leave with us? ");

            string pokemonName = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(pokemonName))
            {
                Console.WriteLine("You must enter a Pokémon name.");
                return;
            }

            try
            {
                var pokemon = await GetPokemonData(pokemonName);

                if (pokemon == null || pokemon.Species == null)
                {
                    Console.WriteLine("We couldn't find that Pokémon in our Daycare records.");
                    return;
                }

                var species = await GetPokemonSpeciesData(pokemon.Species.Url);

                if (species == null)
                {
                    Console.WriteLine("We found the Pokémon, but could not retrieve species data.");
                    return;
                }

                Console.WriteLine($"\nGreat! {pokemonName.ToUpper()} has been checked in.");
                Console.WriteLine("\n--- Daycare Worker Report ---");

                // Egg groups
                string eggGroups = species.EggGroups != null && species.EggGroups.Length > 0
                    ? string.Join(", ", species.EggGroups.Select(g => g.Name))
                    : "Unknown";

                Console.WriteLine($"Egg Groups: {eggGroups}");

                // Hatch counter
                Console.WriteLine($"Hatch Counter: {species.HatchCounter} cycles");

                // Steps until hatch
                int stepsPerCycle = 257;
                int totalSteps = species.HatchCounter * stepsPerCycle;
                Console.WriteLine($"Estimated Steps Until Egg Hatches: {totalSteps}");

                // Time until egg can be produced (fun mechanic)
                int minutesUntilEgg = species.HatchCounter * 3;
                Console.WriteLine($"Estimated Time Until Daycare Can Produce an Egg: {minutesUntilEgg} minutes");

                Console.WriteLine("\nYour Pokémon is in good hands!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        // ---------------------------
        // API CALLS WITH ERROR HANDLING
        // ---------------------------

        static async Task<Pokemon?> GetPokemonData(string name)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = $"https://pokeapi.co/api/v2/pokemon/{name}";

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Pokemon>(json);
            }
            catch
            {
                return null;
            }
        }

        static async Task<PokemonSpecies?> GetPokemonSpeciesData(string url)
        {
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PokemonSpecies>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    // ---------------------------
    // CORRECT MODELS (MATCH POKEAPI EXACTLY)
    // ---------------------------

    public class Pokemon
    {
        [JsonPropertyName("species")]
        public SpeciesInfo Species { get; set; }
    }

    public class SpeciesInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class PokemonSpecies
    {
        [JsonPropertyName("egg_groups")]
        public EggGroup[] EggGroups { get; set; }

        [JsonPropertyName("hatch_counter")]
        public int HatchCounter { get; set; }
    }

    public class EggGroup
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
