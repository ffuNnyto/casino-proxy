
using Games.Hacksaw.DataBase;
using Newtonsoft.Json;

namespace Game.Providers.Hacksaw;


public class Hacksaw
{

    public static List<Tuple<string, string>> CurrentSessions = new List<Tuple<string, string>>();

    public static List<Tuple<string, IHacksawAuth>> Auths = new List<Tuple<string, IHacksawAuth>>();

    public static IHacksawAuth FindAuth(string Session)
    {
        return Auths.Find((auth) => auth.Item2.SessionUuid == Session)!.Item2;
    }

    public static void Auth(string Response)
    {

        var authContent = JsonConvert.DeserializeObject<IHacksawAuth>(Response);
        if (Auths.Find((item) => item.Item2.SessionUuid == (string)authContent!.SessionUuid!) == null)
            Auths.Add(new Tuple<string, IHacksawAuth>(authContent!.SessionUuid!, authContent));

    }

    public static void AddSession(string Data)
    {
        try
        {
            var gameLaunch = JsonConvert.DeserializeObject<IHackSawRequestGameLaunch>(Data);
            string game = gameLaunch!.PackageName!.Split(new char[] { '@' })[0];
            CurrentSessions.Add(new Tuple<string, string>(gameLaunch.SessionUuid!, game));

            var auth = FindAuth(gameLaunch.SessionUuid!);

            if (!HacksawDataBase.FindSlotInDB(game))
                HacksawDataBase.AddSlot(game, auth.BonusGames!);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public static async Task GetResponse(string RequestString, string ResponseString)
    {

        try
        {
            var requestData = JsonConvert.DeserializeObject<IHackSawRequest>(RequestString);
            var responseData = JsonConvert.DeserializeObject<IHackSawResponse>(ResponseString);

            if (requestData == null || responseData == null) return;

            if (requestData.ContinueInstructions != null) return; /* EVENT EXIT - FEATURE WIN*/

            var currentGame = CurrentSessions.Find((v) => v.Item1 == requestData.SessionUuid)?.Item2!;

            dynamic recordData = new System.Dynamic.ExpandoObject();
            recordData.betAmount = (float)requestData.Bets![0].BetAmount! / 100;
            recordData.buyBonus = requestData.Bets[0].BuyBonus == null ? false : true;
            recordData.isBonus = false;
            recordData.win = (float)responseData.Round!.events[0]?.wa / 100;
            recordData.feature = "spin";

            var EventsCount = (dynamic)responseData.Round.events.Count;

            if (EventsCount == 1 && recordData.buyBonus == true)  /*HACKSAW SPECIAL FEATURE*/
            {
                recordData.isBonus = true;
                recordData.feature = requestData.Bets[0].BuyBonus;
                recordData.x = recordData.win / recordData.betAmount;
            }
            else if (EventsCount == 1 && recordData.buyBonus == false)
                recordData.x = recordData.win / recordData.betAmount;

            else if (EventsCount > 1)
            {
                var feature = responseData.Round.events[1].c.bonusFeatureWon;

                recordData.isBonus = true;
                recordData.feature = feature != null ? feature : Convert.ToString(responseData.Round.events[1].etn).Split('_')[0];
                recordData.win = (float)responseData.Round.events[EventsCount - 1].awa / 100;
                //recordData.x = (float)recordData.win / recordData.betAmount;
            }
            await HacksawDataBase.EditHackSawDataBase(currentGame, recordData);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await Task.Delay(10);
    }

}