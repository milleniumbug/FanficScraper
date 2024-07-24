namespace Common.Crypto;

using System.Text;
using Geralt;

public static class Crypto
{
    private const int DefaultIterations = 4;
    private const int DefaultMemorySize = 268435456;
    
    public static Secrets Encrypt(byte[] plaintext, string password)
    {
        var (key, keyInfo) = DeriveKey(password);

        var ciphertext = new byte[plaintext.Length + XChaCha20Poly1305.TagSize];
        var nonce = new byte[XChaCha20Poly1305.NonceSize];
        SecureRandom.Fill(nonce);
        XChaCha20Poly1305.Encrypt(ciphertext, plaintext, nonce, key.Span);

        return new Secrets(ciphertext, new DataInfo(nonce, keyInfo));
    }

    public static byte[] Decrypt(Secrets secrets, string password)
    {
        var key = DeriveKey(password, secrets.DataInfo.KeyInfo);
        
        var plaintext = new byte[secrets.Ciphertext.Length - XChaCha20Poly1305.TagSize];
        XChaCha20Poly1305.Decrypt(plaintext, secrets.Ciphertext.Span, secrets.DataInfo.Nonce.Span, key.Span);
        return plaintext;
    }

    public static (ReadOnlyMemory<byte> key, KeyInfo keyInfo) DeriveKey(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var key = new byte[Argon2id.KeySize];
        var salt = new byte[Argon2id.SaltSize];
        SecureRandom.Fill(salt);
        Argon2id.DeriveKey(key, passwordBytes, salt, iterations: DefaultIterations, memorySize: DefaultMemorySize);
        
        var keyInfo = new KeyInfo(salt, DefaultIterations, DefaultMemorySize);
        return (key, keyInfo);
    }
    
    public static ReadOnlyMemory<byte> DeriveKey(string password, KeyInfo keyInfo)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var key = new byte[Argon2id.KeySize];
        Argon2id.DeriveKey(key, passwordBytes, keyInfo.Salt.Span, iterations: keyInfo.Iterations, memorySize: keyInfo.MemorySize);
        return key;
    }
}

public record Secrets(ReadOnlyMemory<byte> Ciphertext, DataInfo DataInfo);