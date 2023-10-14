using Newtonsoft.Json;

namespace Quiz;

public class QAGenerator
{
    public static Dictionary<string, KeyValuePair<int, string[]>> MyDict = new Dictionary<string, KeyValuePair<int, string[]>>();
    public static List<KeyValuePair<int, string[]>> Dict1 = new();
    public Dictionary<string, KeyValuePair<int, string[]>> Generate()
    {
        Random rnd = new Random();
        //Generating questions and answers for telegram chat bot
        //read list of questions from file
        string currentDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine($"currentDirectory: {currentDirectory}");
        string path1 = System.IO.Path.Combine(currentDirectory, "linesofqs.txt");
        string path2 = System.IO.Path.Combine(currentDirectory, "linesofas.txt");
        //string path2 = "C:\\Users\\User\\Documents\\linesofas.txt";

        var lines1 = File.ReadLines(path1).ToArray();
        var lines2 = File.ReadLines(path2).ToArray();

        //Console.WriteLine(lines1.Count());
        Console.WriteLine(lines2.Count());

        //foreach (var line in lines1)
        //{
        //    //Console.WriteLine(line);
        //    //myDict.Add(line, null);
        //}
        Console.WriteLine(Dict1.Count);

        //foreach (var kvp in MyDict)
        //{
        //    Console.WriteLine(kvp.Key); 
        //}

        //foreach (var line in lines2)
        //{
        //    Console.WriteLine(line);
        //    //MyDict.Add(line, null);
        //}
        var allIndexes = Enumerable.Range(0, 3).ToArray();
        //Console.WriteLine($"allIndexes {JsonConvert.SerializeObject(allIndexes)}");
        var assignedIndex = new int[1];

        var linesindexes = Enumerable.Range(0, 2).ToArray();

        //Console.WriteLine(allIndexes.Length);
        //generate answers list
        for (var i = 0; i < lines2.Length; i++)
        {
            string[] arr = new string[3];

            var index1 = rnd.Next(3);
            assignedIndex[0] = index1;
            //Dict1.ElementAt(i);
            var index2 = rnd.Next(lines2.Length);
            var index3 = rnd.Next(lines2.Length);
            while (index2 == index3)
            {
                index3 = rnd.Next(lines2.Length);
            }
            linesindexes[0] = index2;
            linesindexes[1] = index3;
            //Console.WriteLine($"linesindexes {JsonConvert.SerializeObject(linesindexes)}");

            var freeindexes = allIndexes.Except(assignedIndex).ToArray();
            //Console.WriteLine($"freeindexes {JsonConvert.SerializeObject(freeindexes)}");
            arr[index1] = lines2[i];

            for (var j = 0; j < freeindexes.Length; j++)
            {
                arr[freeindexes[j]] = lines2[linesindexes[j]];
            }
            //Console.WriteLine($"arr is {JsonConvert.SerializeObject(arr)}");

            Dict1.Add(new KeyValuePair<int, string[]>(index1, arr));
                                                                     
        }


        for (var i = 0; i < lines1.Length; i++)
        {
            MyDict.Add(lines1[i], Dict1[i]);
        }

        //foreach (var kvp in MyDict)
        //{
        //    Console.WriteLine(kvp.Key);
        //    Console.WriteLine($"kvp.Value {JsonConvert.SerializeObject(kvp.Value)}");
        //}

        return MyDict;
    }

}
