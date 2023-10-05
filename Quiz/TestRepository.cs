using System.Text.Json;

namespace Quiz;

public static class TestRepository
{
    public static List<Test> GetTests()
    {
        var stringTest = File.ReadAllText("C:\\Users\\User\\Quiz_TelegramBot\\Quiz\\questions.json");
        var tests = JsonSerializer.Deserialize<List<Test>>(stringTest);

        return tests;
    }
}