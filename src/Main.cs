using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using System.IO;

class MainClass
{
    const string OUTPUT_FILE_NAME = "grouped_products.json";

    static async Task Main(string[] args)
    {
        // Create the auxiliary program and retrieve content from the given URL.
        Program program = new Program();
        List<Item> items = await program.GetContent("https://fakestoreapi.com/products");

        // Check whether or not the request was successful.
        if (items.Count == 0)
        {
            Console.WriteLine("Unsuccessful request.");
            return;
        }

        // Group the items first by their respective categories.
        var itemsGrouped = from item in items
                           group item by item.category;

        // Form a dictionary that contains the wanted information of each item.
        var itemsDictionary = itemsGrouped.ToDictionary(
            group => group.Key,
            group => group.Select(item => new { item.id, item.title, item.price }).OrderBy(item => item.price).ToArray()
        );

        // Serialize in json form.
        string json = JsonConvert.SerializeObject(itemsDictionary, Formatting.Indented);
        // Console.WriteLine(json);

        // Finally, write the output into a new file.
        try
        {
            File.WriteAllText(OUTPUT_FILE_NAME, json);
            Console.WriteLine($"Data successfully written to the file: {OUTPUT_FILE_NAME}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception when saving the result: {e.Message}");
        }
    }
}