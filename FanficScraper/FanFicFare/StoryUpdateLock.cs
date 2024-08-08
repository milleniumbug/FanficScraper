using Nito.AsyncEx;

namespace FanficScraper.FanFicFare;

public class StoryUpdateLock
{
    private readonly AsyncReaderWriterLock asyncLock = new AsyncReaderWriterLock();

    public AwaitableDisposable<IDisposable> TakeStoryUpdateLock()
    {
        return asyncLock.ReaderLockAsync();
    }
    
    public AwaitableDisposable<IDisposable> TakeBackupCreateLock()
    {
        return asyncLock.WriterLockAsync();
    }
}