using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FanficScraper.Data;

public class PhraseGenerator
{
    private static readonly List<string> Words = LoadWordList();

    private static List<string> LoadWordList()
    {
        var assembly = typeof(PhraseGenerator).Assembly;
        Stream resource = assembly.GetManifestResourceStream("FanficScraper.words")
            ?? throw new InvalidOperationException();

        var resultList = new List<string>();
        using var reader = new StreamReader(resource);
        string? line = null;
        while ((line = reader.ReadLine()) != null)
        {
            resultList.Add(line);
        }

        return resultList;
    }

    private readonly RandomNumberGenerator randomNumberGenerator;

    public PhraseGenerator(
        RandomNumberGenerator randomNumberGenerator)
    {
        this.randomNumberGenerator = randomNumberGenerator;
    }

    public string GeneratePhrase(int wordCount)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < wordCount; ++i)
        {
            if (i != 0)
            {
                sb.Append(" ");
            }
            sb.Append(GenerateWord());
        }

        return sb.ToString();
    }

    private string GenerateWord()
    {
        return Words[GetInt32(0, Words.Count)];
    }

    private int GetInt32(int fromInclusive, int toExclusive)
    {
        // The total possible range is [0, 4,294,967,295).
        // Subtract one to account for zero being an actual possibility.
        uint range = (uint)toExclusive - (uint)fromInclusive - 1;
 
        // If there is only one possible choice, nothing random will actually happen, so return
        // the only possibility.
        if (range == 0)
        {
            return fromInclusive;
        }
 
        // Create a mask for the bits that we care about for the range. The other bits will be
        // masked away.
        uint mask = range;
        mask |= mask >> 1;
        mask |= mask >> 2;
        mask |= mask >> 4;
        mask |= mask >> 8;
        mask |= mask >> 16;
 
        Span<uint> resultSpan = stackalloc uint[1];
        uint result;
 
        do
        {
            this.randomNumberGenerator.GetBytes(MemoryMarshal.AsBytes(resultSpan));
            result = mask & resultSpan[0];
        }
        while (result > range);
 
        return (int)result + fromInclusive;
    }
}