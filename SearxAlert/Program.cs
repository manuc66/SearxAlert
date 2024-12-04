using System.Runtime.CompilerServices;
using System.Text.Json;

var tasks = new[]
{
    new
    {
        lang = "en",
        topics = new[]
        {
            "topic1", "topic2"
        }
    },
    new
    {
        lang = "fr",
        topics = new[] { "sujet1", "sujet2" }
    }
};

foreach (var task in tasks)
{
    foreach (string topic in task.topics)
    {
        Console.WriteLine($"{new string('=', 10)} {task.lang}/{topic} {new string('=', 10)}");
        await ProcessTopic(topic, task.lang);
    }
}


return;

async Task ProcessTopic(string topic, string lang)
{
    var oldResults = ReadCurrent(topic, lang);
    var knownUrl = oldResults!.Select(x => x.url).ToHashSet();

    var currentResults = await FetchCurrentResults(topic, lang);

    var latestResults = new List<Results>(oldResults);
    var newResults = currentResults.Where(x => !knownUrl.Contains(x.url));

    Console.WriteLine("Here are the new results:");
    foreach (var result in newResults)
    {
        Console.WriteLine($"\t{result.url} - {result.title}");
    }

    latestResults.AddRange(newResults);


    var filePath = StoreResults(topic, lang, latestResults);

    Console.WriteLine($"Stored in: {filePath}");
}

List<Results>? ReadCurrent(string topic, string lang)
{
// Serialize the object to JSON and write to a file
    string filePath1 = GetFileResultPath(topic, lang);
    if (!File.Exists(filePath1))
    {
        return new List<Results>();
    }

    using FileStream fs = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read);
    return JsonSerializer.Deserialize<List<Results>>(fs);
}

async Task<List<Results>> FetchCurrentResults(string s1, string lang)
{
    {
        List<Results> list = new List<Results>();
        Results[]? lastResults = null;
        for (var i = 1; i <= 100 && (lastResults == null || lastResults.Length > 1); i++)
        {
            lastResults = await GetAndDisplay(s1, lang, i);
            if (lastResults?.Length > 0)
            {
                list.AddRange(lastResults);
            }
        }

        return list;
    }
}

async Task<string> GetSearchResultPage(string query, string lang, int page)
{
    string s;
    using (HttpClient client = new HttpClient())
    {
        var response = await client.GetAsync(
            $"https://somewhere.tld/searxng/search?q={query}&language={lang}&pageno={page}&format=json");
        response.EnsureSuccessStatusCode();
        s = await response.Content.ReadAsStringAsync();
    }

    return s;
}

async Task<Results[]?> GetAndDisplay(string query, string lang, int page = 1)
{
    var json = await GetSearchResultPage(query, lang, page);
    var rootObject = JsonSerializer.Deserialize<RootObject>(json);
    var rootObjectResults = rootObject?.results ?? [];

    var engines = rootObjectResults.SelectMany(x => x.engines).Distinct();

    Console.WriteLine(
        $"Page {page} with {rootObjectResults.Length} results about {query} from the following engines: {string.Join(", ", engines)}.");

    return rootObjectResults;
}


string StoreResults(string fileName, string lang, List<Results> resultsList)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true // Enable indentation
    };

// Serialize the object to JSON and write to a file
    string filePath1 = GetFileResultPath(fileName, lang);
    using (FileStream fs = File.Create(filePath1))
    {
        JsonSerializer.Serialize(fs, resultsList, options);
    }

    return filePath1;
}

string GetParentFolder([CallerFilePath] string path = "") =>
    new DirectoryInfo(Path.GetDirectoryName(path)).Parent.FullName;

string GetFileResultPath(string topic, string lang)
{
    return Path.Combine(GetParentFolder(), "Data", lang, $"{topic}.json");
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
    public object content { get; set; }
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