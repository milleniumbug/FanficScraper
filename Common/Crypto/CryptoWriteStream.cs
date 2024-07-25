using Geralt;

namespace Common.Crypto;

public class CryptoWriteStream : CryptoWriteStreamBase<SymmetricEncryptionFileInfo>
{
    private KeyInfo? keyInfo;

    public CryptoWriteStream(Stream stream, string password, bool leaveOpen = false) : base(stream, password, leaveOpen)
    {
    }

    protected override ReadOnlyMemory<byte> DeriveKey(string passphrase)
    {
        (var key, this.keyInfo) = Crypto.DeriveKey(passphrase);
        return key;
    }

    protected override SymmetricEncryptionFileInfo StoreNonce(ReadOnlyMemory<byte> nonce)
    {
        return new SymmetricEncryptionFileInfo(new DataInfo(nonce, keyInfo!));
    }
}