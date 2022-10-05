

using Newtonsoft.Json;

namespace Games.Pragmatic.DataBase;

class PragmaticDataBase
{

    public static Dictionary<string, dynamic> DB;
    public static void OpenDataBase()
    {
        using (StreamReader r = new StreamReader("./pragmatic.json"))
        {
            DB = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(r.ReadToEnd());
        }
    }

    public static bool FindSlotInDB(string Slot)
    {
        return DB.ContainsKey(Slot);

    }


    public static void AddSlot(string Name, string LabelName)
    {
        if (PragmaticDataBase.FindSlotInDB(Name))
            return;

        var slot = new
        {
            label = LabelName,
            spins = new
            {
                total = (int)0,
                win = (int)0,
                lose = (int)0

            },
            bonus = new
            {
                name = "FREESPINS",
                buy = new
                {
                    total = (int)0,
                    win = (int)0,
                    lose = (int)0
                },
                spin = new
                {
                    total = (int)0,
                    win = (int)0
                }
            },
            total = new
            {
                waste = (int)0,
                win = (int)0
            }
        };

        DB.Add(Name, slot);
        File.WriteAllText("./pragmatic.json", JsonConvert.SerializeObject(DB));
        OpenDataBase();
    }
    public static async Task EditPragmaticDataBase(string Game, dynamic Record)
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
                    if (Record.isBuy)
                    {
                        DB[Game].bonus.buy.total += 1;
                        DB[Game].bonus.buy.win += Record.win;
                        DB[Game].bonus.buy.lose += (Record.betAmount * 100);
                        DB[Game].total.waste += (Record.betAmount * 100);
                    }
                    else
                    {
                        DB[Game].bonus.spin.total += 1;
                        DB[Game].bonus.spin.win += Record.win;
                    }
                }
                DB[Game].total.win += Record.win;
                await File.WriteAllTextAsync("./pragmatic.json", JsonConvert.SerializeObject(DB));

            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Task.Delay(10);
        }
    }
}