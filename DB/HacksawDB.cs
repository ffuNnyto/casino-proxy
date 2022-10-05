

using System.Dynamic;
using Newtonsoft.Json;

namespace Games.Hacksaw.DataBase;

class HacksawDataBase
{

    public static Dictionary<string, dynamic> DB;
    public static void OpenDataBase()
    {
        using (StreamReader r = new StreamReader("./hacksaw.json"))
        {
            DB = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(r.ReadToEnd());
        }

    }

    public static bool FindSlotInDB(string Slot)
    {
        return DB.ContainsKey(Slot);

    }

    public static void AddSlot(string Name, dynamic bonusGames)
    {

        var setBonus = new ExpandoObject();

        foreach (var bonus in bonusGames)
        {
            setBonus!.TryAdd((string)bonus.bonusGameId, new
            {
                multi = bonus.betCostMultiplier,
                expectRtp = bonus.expectedRtp,
                spin = new
                {
                    total = 0,
                    win = 0
                },
                buy = new
                {
                    total = 0,
                    win = 0,
                    lose = 0
                }
            });
        }

        var slot = new
        {
            spins = new
            {
                total = 0,
                win = 0,
                lose = 0

            },
            ingame_bonus_names = setBonus,
            total = new
            {
                waste = 0,
                win = 0

            }
        };

        DB.Add(Name, slot);
        File.WriteAllText("./hacksaw.json", JsonConvert.SerializeObject(DB, Formatting.Indented));
        OpenDataBase();

    }
    public static async Task EditHackSawDataBase(string Game, dynamic Record)
    {
        try
        {
            await Task.Run(async () =>
            {

                if (!Record.isBonus)
                {
                    DB[Game].spins.total += 1;
                    DB[Game].spins.lose += Record.betAmount;
                    DB[Game].spins.win += Record.win;
                    DB[Game].total.waste += Record.betAmount;

                }
                else
                {
                    string bonusName = Convert.ToString(Record!.feature!);
                    if (Record.buyBonus)
                    {
                        DB[Game].ingame_bonus_names[bonusName].buy.total += 1;
                        DB[Game].ingame_bonus_names[bonusName].buy.win += Record.win;
                        DB[Game].ingame_bonus_names[bonusName].buy.lose += Record.betAmount * Convert.ToInt32(DB[Game].ingame_bonus_names[bonusName].multi);
                        DB[Game].total.waste += Record.betAmount * Convert.ToInt32(DB[Game].ingame_bonus_names[bonusName].multi);
                    }
                    else
                    {
                        DB[Game].ingame_bonus_names[bonusName].spin.total += 1;
                        DB[Game].ingame_bonus_names[bonusName].spin.win += Record.win;
                    }
                }
                DB[Game].total.win += Record.win;
                await File.WriteAllTextAsync("./hacksaw.json", JsonConvert.SerializeObject(DB));
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        await Task.Delay(10);
    }
}