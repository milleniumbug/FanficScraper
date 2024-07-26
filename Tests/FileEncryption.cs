using System.Security.Cryptography;
using Common.Crypto;
using Geralt;

namespace TestProject1;

public class FileEncryption
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(64 * 1024 - 1)]
    [InlineData(64 * 1024)]
    [InlineData(64 * 1024 + 1)]
    [InlineData(64 * 1024 + IncrementalXChaCha20Poly1305.TagSize - 1)]
    [InlineData(64 * 1024 + IncrementalXChaCha20Poly1305.TagSize)]
    [InlineData(64 * 1024 + IncrementalXChaCha20Poly1305.TagSize + 1)]
    [InlineData(2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize - 1)]
    [InlineData(2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize)]
    [InlineData(2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize + 1)]
    [InlineData(1024 + 2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize - 1)]
    [InlineData(1024 + 2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize)]
    [InlineData(1024 + 2 * 64 * 1024 + IncrementalXChaCha20Poly1305.TagSize + 1)]
    public void HappyPath(int dataSize)
    {
        var actualData = new byte[dataSize];
        var expectedData = new byte[dataSize];
        if (dataSize != 0)
        {
            SecureRandom.Fill(expectedData);
        }

        var password = "this is a common password";

        byte[] encryptedData;
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoWriteStream(memoryStream, password, leaveOpen: true))
            {
                cryptoStream.Write(expectedData);
            }

            encryptedData = memoryStream.ToArray();
        }

        using (var memoryStream = new MemoryStream(encryptedData))
        {
            using (var cryptoStream = new CryptoReadStream(memoryStream, password, leaveOpen: true))
            {
                cryptoStream.ReadExactly(actualData);
            }
        }
        
        Assert.Equal(expectedData, actualData);
    }
    
    [Fact]
    public void Corruption()
    {
        var actualData = new byte[16];
        var expectedData = new byte[16];
        SecureRandom.Fill(expectedData);
        var password = "this is a common password";

        byte[] encryptedData;
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoWriteStream(memoryStream, password, leaveOpen: true))
            {
                cryptoStream.Write(expectedData);
            }

            encryptedData = memoryStream.ToArray();
        }

        encryptedData[^4] = (byte)(encryptedData[^4] + 1);
        
        Assert.Throws<CryptographicException>(() =>
        {
            using var memoryStream = new MemoryStream(encryptedData.AsSpan().ToArray());
            using var cryptoStream = new CryptoReadStream(memoryStream, password, leaveOpen: true);
            cryptoStream.ReadExactly(actualData);
        });
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Truncation(int bytesToCutOff)
    {
        var actualData = new byte[64 * 1024];
        var expectedData = new byte[64 * 1024];
        SecureRandom.Fill(expectedData);
        var password = "this is a common password";

        byte[] encryptedData;
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoWriteStream(memoryStream, password, leaveOpen: true))
            {
                cryptoStream.Write(expectedData);
            }

            encryptedData = memoryStream.ToArray();
        }
        
        Assert.Throws<CryptographicException>(() =>
        {
            using var memoryStream = new MemoryStream(encryptedData.AsSpan(..^bytesToCutOff).ToArray());
            using var cryptoStream = new CryptoReadStream(memoryStream, password, leaveOpen: true);
            cryptoStream.ReadExactly(actualData);
        });
    }
}