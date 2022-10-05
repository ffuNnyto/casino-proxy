
namespace Game.Providers.Hacksaw;
public class IHacksawAuth
{
    public dynamic[]? BonusGames { get; set; }

    public dynamic? AccountBalance { get; set; }
    public string? SessionUuid { get; set; }
}
public class IHackSawRequest
{
    public string? SessionUuid { get; set; }
    public IHackSawRequestBets[]? Bets { get; set; }
    public dynamic? ContinueInstructions { get; set; }
}

public class IHackSawRequestBets
{
    public int? BetAmount { get; set; }
    public string? BuyBonus { get; set; }
}


public class IHackSawResponse
{
    public dynamic? Round { get; set; }
}

public class IHackSawRequestGameLaunch
{
    public string? SessionUuid { get; set; }
    public string? PackageName { get; set; }
}