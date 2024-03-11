using System.Net;
using System.Net.Http.Headers;

namespace Common.Challenges;

public class ChallengeSolverHandler : DelegatingHandler
{
    private const int MaxCountAttempts = 5;
    private readonly IChallengeSolver solver;
    private readonly HttpClientHandler handler;

    public ChallengeSolverHandler(IChallengeSolver solver, HttpClientHandler? handler = null)
    {
        this.handler = handler ?? new HttpClientHandler();
        this.InnerHandler = this.handler;
        this.solver = solver;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        for(int i = 0; i < MaxCountAttempts; i++)
        {
            var solution = await solver.Solve(request.RequestUri ?? throw new ArgumentException(nameof(request)));

            if (solution.Cookies != null)
            {
                foreach (var cookie in solution.Cookies)
                {
                    this.handler.CookieContainer.Add(cookie);
                }
            }

            if (solution.UserAgent != null)
            {
                request.Headers.UserAgent.Clear();
                request.Headers.UserAgent.TryParseAdd(solution.UserAgent);
            }

            response = await base.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                solver.Invalidate(solution);
            }
            else
            {
                return response;                
            }
        }

        return response ?? throw new HttpRequestException();
    }
}