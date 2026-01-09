using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using UnityEngine;
using Firebase.Database.Query;
using static FirebaseConnect;
using System.Threading.Tasks;
using System.Linq;

public class ThamKhaoCodeFirebase : MonoBehaviour
{
    //LINK FIREBASE
    public static string firebaseUrl = "https://lab12final-26a7f-default-rtdb.asia-southeast1.firebasedatabase.app/";

    //LINK JSON
    private static readonly HttpClient client = new HttpClient();
    public string jsonUrl = "https://raw.githubusercontent.com/NTH-VTC/OnlineDemoC-/refs/heads/main/lab12_players.json";

    static async Task Main(string[] args)
    {
        //VIETNAMESE
        Console.OutputEncoding = Encoding.UTF8;


        //CONNECT FIREBASE
        FirebaseApp.Create(new AppOptions()
        {
            //KEY GENERATE (PROJECT SETTING -> SERVICE ACCOUNTS -> GENERATE NEW KEY)
            Credential = GoogleCredential.FromFile("lab12final-26a7f-firebase-adminsdk-fbsvc-efa1fa5050.json")
        });

        var firebase = new FirebaseClient(firebaseUrl);


        //CONNECT AND LOAD JSON
        string json = await client.GetStringAsync("https://raw.githubusercontent.com/NTH-VTC/OnlineDemoC-/refs/heads/main/lab12_players.json");

        var allPlayers = JsonConvert.DeserializeObject<List<Player>>(json);


        //WORKING

        //1
        DateTime now = new DateTime(2025, 07, 01, 0, 0, 0, DateTimeKind.Utc);

        var list1 = allPlayers.Where(p => (((now - p.LastLogin).TotalDays > 10) || (p.IsActive == false)) && p.VipLevel <= 8)
            .Select(p => new { p.Name, p.IsActive, p.Level, p.LastLogin });

        Console.WriteLine($"{"[Name]",-10} {"[IsActive]",-10} {"[Level]",-10} {"[LastLogin]",5}");
        foreach (var p in list1)
        {
            Console.WriteLine($"{p.Name,-10} {p.IsActive,-10} {p.Level,-10} {p.LastLogin,5}");
        }

        int i = 1;
        foreach (var player in list1)
        {
            await firebase
                .Child("inactive_lowlevel_players")
                .Child($"{i}")
                .PutAsync(player);
            i++;
        }

        var list2 = allPlayers.Where(p => p.Level >= 12 && p.Gold > 2000)
            .Select(p => new { p.Name, p.Level, CurrentGold = p.Gold });


        Console.WriteLine($"\n{"[Name]",-10} {"[Level]",-10} {"[CurrentGold]",5}");
        foreach (var p in list2)
        {
            Console.WriteLine($"{p.Name,-10} {p.Level,-10} {p.CurrentGold,5}");
        }


        i = 1;
        foreach (var player in list2)
        {
            await firebase
                .Child("highlevel_rich_players")
                .Child($"{i}")
                .PutAsync(player);
            i++;
        }

        //2
        DateTime now1 = new DateTime(2025, 07, 01, 0, 0, 0, DateTimeKind.Utc);
        var list3 = allPlayers.Where(p => (now1 - p.LastLogin).TotalDays <= 3 && (p.IsActive == true))
            .OrderByDescending(p => p.Coins)
            .Take(3).Select(p => new { p.Name, p.Level, p.Coins, AwardedCoinAmount = 0, Rank = 0 });

        int bonus = 3000;
        int r = 1;
        Console.WriteLine($"\n{"[Name]",-10} {"[Level]",-10} {"[Coins]",-10} {"[AwardedCoinAmount]",-10} {"[Rank]",5}");
        foreach (var p in list3)
        {
            Console.WriteLine($"{p.Name,-10} {p.Level,-10} {p.Coins,-10} {p.AwardedCoinAmount + bonus,-20} {p.Rank + r,-5}");
            bonus -= 1000;
            r++;
        }

        i = 1;
        foreach (var player in list3)
        {
            await firebase
                .Child("top3_active_coin_awards")
                .Child($"{i}")
                .PutAsync(player);
            i++;
        }


        //update
        i = 1;
        int AwardedCoinAmount = 3000;
        int Rank = 1;
        foreach (var player in list3)
        {
            var Data = new
            {
                AwardedCoinAmount,
                Rank
            };

            await firebase
                .Child("top3_active_coin_awards")
                .Child($"{i}")
                .PatchAsync(Data);

            i++;
            AwardedCoinAmount -= 1000;
            Rank += 1;
        }
    }
}
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int VipLevel { get; set; }
        public int Gold { get; set; }
        public int Coins { get; set; }
        public bool IsActive { get; set; }
        public string Region { get; set; }
        public DateTime LastLogin { get; set; }
    }
