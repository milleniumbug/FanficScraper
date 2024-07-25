namespace Common.Crypto;

public class CryptoReadStream : CryptoReadStreamBase<SymmetricEncryptionFileInfo>
{
    public CryptoReadStream(Stream stream, string passphrase, bool leaveOpen = false) : base(stream, passphrase, leaveOpen)
    {
        
    }

    protected override (ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> nonce) DeriveKey(string passphrase, SymmetricEncryptionFileInfo header)
    {
        var key = Crypto.DeriveKey(passphrase, header.DataInfo.KeyInfo);
        return (key, header.DataInfo.Nonce);
    }
}