using System.IO.Pipelines;
using Common.Crypto;
using Geralt;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace TestProject1;

public class AgeEncryption
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(64 * 1024)]
    public async Task HappyPath(int dataSize)
    {
        byte[] actualData;
        var expectedData = new byte[dataSize];
        if (dataSize != 0)
        {
            SecureRandom.Fill(expectedData);
        }

        byte[] encryptedData;
        using (var input = new MemoryStream(expectedData))
        using (var output = new MemoryStream())
        {
            await Age.Encrypt(input, output, recipients: ["age1qk82n3ytflnlt8hc04qnxav90cullnfler8t3pzdae0tlvlx64pqmjuu7k"]);

            encryptedData = output.ToArray();
        }
        
        using (var input = new MemoryStream(encryptedData))
        using (var output = new MemoryStream())
        {
            await Age.Decrypt(input, output, identityFilePaths: ["/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/lol.txt"]);

            actualData = output.ToArray();
        }
        
        Assert.Equal(expectedData, actualData);
    }
    
    [Fact]
    public async Task Tar()
    {
        var path = "/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/aaa/aaa.tar.gz";
        byte[] actualData;
        byte[] encryptedData;
        Task encrypt;
        var pipe = new Pipe();
        using (var outStream = new MemoryStream())
        {
            using (var gzoStream = new GZipOutputStream(pipe.Writer.AsStream()))
            using (var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
            {
                encrypt = Age.Encrypt(pipe.Reader.AsStream(), outStream,
                    recipients: ["age1qk82n3ytflnlt8hc04qnxav90cullnfler8t3pzdae0tlvlx64pqmjuu7k"]);
                var tarEntry =
                    TarEntry.CreateEntryFromFile("/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/lol.txt");
                tarArchive.WriteEntry(tarEntry, recurse: true);
            }
            
            await encrypt;
        }
        
        using (var input = File.OpenRead(path))
        using (var output = File.Create("/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/aaa/out.tar.gz"))
        {
            await Age.Decrypt(input, output, identityFilePaths: ["/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/lol.txt"]);
        }
    }
#if false
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(64 * 1024)]
    public async Task Streams(int dataSize)
    {
        var actualData = new byte[dataSize];
        var expectedData = new byte[dataSize];
        if (dataSize != 0)
        {
            SecureRandom.Fill(expectedData);
        }

        byte[] encryptedData;
        using (var output = new MemoryStream())
        {
            using (var cryptoStream = new AgeCryptoOutputStream(
                       output,
                       recipients: ["age1qk82n3ytflnlt8hc04qnxav90cullnfler8t3pzdae0tlvlx64pqmjuu7k"],
                       leaveOpen: true))
            {
                await cryptoStream.WriteAsync(expectedData);
                await cryptoStream.FlushAsync();
            }
            
            encryptedData = output.ToArray();
        }

        using (var input = new MemoryStream(encryptedData))
        using (var output = new MemoryStream())
        {
            await Age.Decrypt(input, output,
                identityFilePaths: ["/home/milleniumbug/dokumenty/PROJEKTY/InDevelopment/FanficScraper/lol.txt"]);
        }
        
        Assert.Equal(expectedData, actualData);
    }
#endif
}