namespace Common.Crypto;

public record KeyInfo(ReadOnlyMemory<byte> Salt, int Iterations, int MemorySize);