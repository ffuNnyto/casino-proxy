using Games.Hacksaw.DataBase;
using Games.Pragmatic.DataBase;
using WS.Connection;
using System.Net;
using System;

new ServerConnection(IPAddress.Any, 6990).Start();
HacksawDataBase.OpenDataBase();
PragmaticDataBase.OpenDataBase();

AppProxy.Proxy.Start();

Console.WriteLine("CHECK YOUR STATS HERE: https://casino-proxy-stats.pages.dev/");

Console.ReadLine();