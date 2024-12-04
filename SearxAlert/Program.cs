// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text.Json;

var query = "some-topic";

var oldResults = ReadCurrent(query);
var knownUrl = oldResults!.Select(x => x.url).ToHashSet();

var currentResults = await FetchCurrentResults(query);

var newResults = new List<Results>(oldResults);
newResults.AddRange(currentResults.Where(x => !knownUrl.Contains(x.url)));


var filePath = StoreResults(query, newResults);

Console.WriteLine($"Object written to {filePath} with indentation.");

Console.WriteLine("Hello, World!");

string StoreResults(string fileName, List<Results> resultsList)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true // Enable indentation
    };

// Serialize the object to JSON and write to a file
    string filePath1 = Path.Combine(GetParentFolder(), $"{fileName}.json");
    using (FileStream fs = File.Create(filePath1))
    {
        JsonSerializer.Serialize(fs, resultsList, options);
    }

    return filePath1;
}

List<Results>? ReadCurrent(string fileName)
{

// Serialize the object to JSON and write to a file
    string filePath1 = Path.Combine(GetParentFolder(), $"{fileName}.json");
    using (FileStream fs = File.OpenRead(filePath1))
    {
        return JsonSerializer.Deserialize<List<Results>>(fs);
    }

}

string GetParentFolder([CallerFilePath] string path = "") => new DirectoryInfo(Path.GetDirectoryName(path)).Parent.FullName;

async Task<List<Results>> FetchCurrentResults(string s1)
{
    {
        List<Results> list = new List<Results>();
        Results[]? lastResults = null;
        for (var i = 1; i <= 100 && (lastResults == null || lastResults.Length > 1); i++)
        {
            lastResults = await GetAndDisplay(s1, i);
            if (lastResults?.Length > 0)
            {
                list.AddRange(lastResults);
            }
        }

        return list;
    }

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