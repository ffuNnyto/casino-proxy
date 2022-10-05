
using System.Dynamic;
using System.Globalization;
using System.Text;
using Games.Pragmatic.DataBase;

namespace Game.Providers.Pragmatic;


public class Pragmatic
{
    public static Dictionary<string, PragmaticSession> PragmaticSessions = new Dictionary<string, PragmaticSession>();
    public static PragmaticSession FindSessionByMGCKey(string mgcKey)
    {

        try
        {
            return PragmaticSessions[mgcKey];
        }
        catch
        {
            return null;
        }

    }

    public static string? FindValue(string[] Data, string Value)
    {
        try
        {
            return Data.Where((param) => param.Split('=')[0] == Value!).FirstOrDefault()!.Split('=')[1];
        }
        catch
        {
            return null;
        }

    }
    public static async Task GetResponse(string Request, string Response, string Referer)
    {

        try
        {
            CultureInfo cultures = new CultureInfo("en-US");

            var requestData = Request.Split('&');
            var responseData = Response.Split('&');
            var refererParams = Referer.Split('?');
            var labelGameName = Convert.ToString(FindValue(Referer.Split('?')[1]!.Split('&'), "gname")!).Replace("%20", "");

            string action = FindValue(requestData, "action")!;
            string symbol = FindValue(requestData, "symbol")!;
            string mgckey = FindValue(requestData, "mgckey")!;

            double c = Convert.ToDouble(FindValue(requestData, "c"), cultures);
            double l = Convert.ToDouble(FindValue(requestData, "l"), cultures);

            var bl = FindValue(requestData, "bl");
            var pur = FindValue(requestData, "pur");
            var fsmul = FindValue(responseData, "fsmul");
            var fs_total = FindValue(responseData, "fs_total");


            PragmaticSession session = FindSessionByMGCKey(mgckey);

            dynamic record = new ExpandoObject();
            record.symbol = symbol;
            record.isBuy = (pur != null) ? true : false;
            record.betAmount = (bl == "1" ? (c * 20) + ((c * 20) * 0.25) : (c * 20));
            record.win = Convert.ToDouble(FindValue(responseData, "tw"), cultures);





            if (record.isBuy)
                session.SetOnBonusBuy(record.betAmount);


            switch (action)
            {
                case "doInit":
                    PragmaticSessions.Add(mgckey, new PragmaticSession((string)action, (string)symbol, (string)mgckey));
                    PragmaticDataBase.AddSlot(symbol, labelGameName);
                    return;
                case "doSpin":
                    if (record.isBuy == false && !session.OnBonus) //NORMAL SPIN
                    {
                        //if(record.win > 0) return;
                        session.BonusBet = record.betAmount;
                        session.BonusTw = record.win;
                        record.isBonus = false;
                        await PragmaticDataBase.EditPragmaticDataBase(symbol, record);
                    }
                    else if (session.OnBonus) //BONUS SPIN
                    {
                        record.isBonus = true;
                        session.BonusTw = record.win;
                        session.BonusBet = record.betAmount;
                    }
                    return;
                case "doCollect":
                    record.win = session.BonusTw;
                    record.isBonus = session.OnBonus;
                    record.isBuy = session.BuyBonus;
                    record.betAmount = session.BonusBet;
                    await PragmaticDataBase.EditPragmaticDataBase(symbol, record);
                    session.CloseBonus();
                    return;
                case "doFSOption":
                case "doBonus":
                    session.OnBonus = true;
                    session.BonusBet = record.betAmount;
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await Task.Delay(10);

    }
}