namespace Common.Crypto;

public record DataInfo(ReadOnlyMemory<byte> Nonce, KeyInfo KeyInfo);