using FanficScraper.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FanficScraper.Services;

public class UserManager
{
    private readonly IPasswordHasher<User> hasher;
    private readonly StoryContext storyContext;
    private readonly PhraseGenerator phraseGenerator;
    private static readonly int PhraseLength = 12;
    private static readonly int LoginLength = 4;

    public UserManager(
        IPasswordHasher<User> hasher,
        StoryContext storyContext,
        PhraseGenerator phraseGenerator)
    {
        this.hasher = hasher;
        this.storyContext = storyContext;
        this.phraseGenerator = phraseGenerator;
    }
    
    public async Task<RegistrationData> CreateUser()
    {
        var registrationData = new RegistrationData(
            this.phraseGenerator.GeneratePhrase(LoginLength),
            this.phraseGenerator.GeneratePhrase(PhraseLength));

        var (login, password) = SplitPhrase(registrationData.PassPhrase);

        var user = new User()
        {
            Id = registrationData.ActivationPhrase,
            Login = login,
            CreationDate = DateTime.UtcNow,
            IsActivated = false,
        };
        user.PasswordHash = this.hasher.HashPassword(user, password);

        this.storyContext.Users.Add(user);
        await this.storyContext.SaveChangesAsync();

        return registrationData;
    }

    public async Task<AuthorizationResult> IsAuthorized(string phrase)
    {
        if (!IsValidPhrase(phrase))
        {
            return AuthorizationResult.Failure;
        }

        var (login, password) = SplitPhrase(phrase);
        var user = await this.storyContext.Users
            .FirstOrDefaultAsync(user => user.Login == login);

        if (user == null)
        {
            return AuthorizationResult.Failure;
        }

        var verificationResult = this.hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return AuthorizationResult.Failure;
        }
        
        if (!user.IsActivated)
        {
            return AuthorizationResult.NeedsActivation;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = this.hasher.HashPassword(user, password);
            await this.storyContext.SaveChangesAsync();
        }

        return AuthorizationResult.Success;
    }

    private bool IsValidPhrase(string phrase)
    {
        return phrase.Split(' ').Length == PhraseLength;
    }

    private (string login, string password) SplitPhrase(string phrase)
    {
        return (
            string.Join(" ", phrase.Split(' ').Take(LoginLength)),
            string.Join(" ", phrase.Split(' ').Skip(LoginLength)));
    }
}