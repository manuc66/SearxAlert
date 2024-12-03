// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text.Json;

List<Results> results = new List<Results>();
Results[]? lastResults = null;
var query = "some-topic";
for (var i = 1; i <= 100 && (lastResults == null || lastResults.Length > 1); i++)
{
    lastResults = await GetAndDisplay(query, i);
    if (lastResults?.Length > 0)
    {
        results.AddRange(lastResults);
    }
}

var options = new JsonSerializerOptions
{
    WriteIndented = true // Enable indentation
};

// Serialize the object to JSON and write to a file
string filePath = Path.Combine(GetParentFolder(), $"{query}.json");
using (FileStream fs = File.Create(filePath))
{
    JsonSerializer.Serialize(fs, results, options);
}

Console.WriteLine($"Object written to {filePath} with indentation.");

Console.WriteLine("Hello, World!");

async Task<string> GetSearchResultPage(string query, int page)
{
    string s;
    using (HttpClient client = new HttpClient())
    {
        var response = await client.GetAsync(
            $"https://somewhere.tld/searxng/search?q={query}&language=fr&pageno={page}&format=json");
        response.EnsureSuccessStatusCode();
        s = await response.Content.ReadAsStringAsync();
    }

    return s;
}

string GetParentFolder([CallerFilePath] string path = "") => new DirectoryInfo(Path.GetDirectoryName(path)).Parent.FullName;

async Task<Results[]?> GetAndDisplay(string query, int page = 1)
{
    var data1 = await GetSearchResultPage(query, page);
    var rootObject = JsonSerializer.Deserialize<RootObject>(data1);
    var rootObjectResults = rootObject?.results ?? [];
    foreach (var result in rootObjectResults)
    {
        Console.WriteLine($"{result.url} --- {result.title}");
    }

    return rootObjectResults;
}


public class RootObject
{
    public string query { get; set; }
    public int number_of_results { get; set; }
    public Results[]? results { get; set; }
    public string[] answers { get; set; }
    public object[] corrections { get; set; }
    public Infoboxes[] infoboxes { get; set; }
    public string[] suggestions { get; set; }
    public object[] unresponsive_engines { get; set; }
}

public class Results
{
    public string title { get; set; }
    public string url { get; set; }
    public string content { get; set; }
    public string engine { get; set; }
    public string[] parsed_url { get; set; }
    public string template { get; set; }
    public string[] engines { get; set; }
    public int[] positions { get; set; }
    public object publishedDate { get; set; }
    public string thumbnail { get; set; }
    public double score { get; set; }
    public string category { get; set; }
}

public class Infoboxes
{
    public string infobox { get; set; }
    public string id { get; set; }
    public string content { get; set; }
    public string img_src { get; set; }
    public Urls[] urls { get; set; }
    public string engine { get; set; }
    public string[] engines { get; set; }
    public object[] attributes { get; set; }
}

public class Urls
{
    public string title { get; set; }
    public string url { get; set; }
}