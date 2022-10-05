
namespace Game.Providers.Pragmatic;

public class PragmaticSession
{
    public string Action { get; set; }
    public string Symbol { get; set; }
    public string Mgckey { get; set; }
    public bool OnBonus { get; set; }
    public bool BuyBonus { get; set; }
    public double BonusTw { get; set; }
    public double BonusBet { get; set; }

    public PragmaticSession(string action, string symbol, string mgckey)
    {
        this.Action = action;
        this.Symbol = symbol;
        this.Mgckey = mgckey;
        this.OnBonus = false;
        this.BuyBonus = false;
        this.BonusTw = 0;
    }

    public void SetOnBonusBuy(double BonusBet)
    {
        this.OnBonus = true;
        this.BuyBonus = true;
        this.BonusBet = BonusBet;
    }
    public void CloseBonus()
    {
        this.OnBonus = false;
        this.BuyBonus = false;
        this.BonusTw = 0;
        this.BonusBet = 0;
    }

    public void OnBonusSpin()
    {
        this.OnBonus = true;
        this.BuyBonus = false;
    }
}