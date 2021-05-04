using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using NLua;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace Auvitau
{
    public class MoneyLib // not at all stolen code from miners haven
    {
        private Lua State { get; set; }

        public MoneyLib()
        {
            State = new Lua();
            State.DoFile("./MoneyLib.lua");
        }

        public string ToString(object x)
        {
            return (string)(State.GetFunction("_G.MoneyLib.HandleMoney").Call(x))[0];
        }

        public double ToNumber(string x)
        {
            return (double)(State.GetFunction("_G.MoneyLib.UnHandleMoney").Call(x))[0];
        }
    }

    public enum Rank
    {
        Disabled = -1,
        User = 0,
        VIP = 1,
        Moderator = 2,
        Administrator = 3,
        Owner = 4,
        Developer = 5,
        Creator = 6
    }

    public enum Perk
    {
        DoubleChances = 0,
        DoubleIncome = 1,
        BagsOCash = 2,
        Rigged = 3,
        TrueRigged = 4
    }

    public enum TransactionType
    {
        Add = 0,
        Remove = 1,
        Pay = 2,
        Reward = 3,
        Fine = 4,
        RawAdd = 5,
        RawSet = 6
    }

    public class User
    {
        public Dictionary<Perk, bool> Perks { get; set; }
        public Rank Rank { get; set; }
        public double Bux { get; set; }

        public ulong id { get; }

        public User(string Serial)
        {
            var x = JsonConvert.DeserializeObject<Dictionary<string, object>>(Serial);
            this.id = Convert.ToUInt64(x["id"]);
            this.Rank = (Rank)Convert.ToInt32(x["Rank"]);
            this.Bux = Convert.ToDouble(x["Bux"]);
            this.Perks = new Dictionary<Perk, bool>();
            this.Perks[Perk.DoubleChances] = false;
            this.Perks[Perk.DoubleIncome] = false;
            this.Perks[Perk.BagsOCash] = false;
            this.Perks[Perk.Rigged] = false;
            this.Perks[Perk.TrueRigged] = false;
            foreach (int P in JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(x["Perks"])))
            {
                this.Perks[(Perk)Convert.ToInt32(P)] = true;
            }
        }

        public User(ulong id, Rank Rank = Rank.User, double Bux = 50, params Perk[] Perks)
        {
            this.id = id;
            if (this.id == Program.CreatorID) this.Rank = Rank.Creator;
            else this.Rank = Rank;
            this.Bux = Bux;
            this.Perks = new Dictionary<Perk, bool>();
            this.Perks[Perk.DoubleChances] = this.id == Program.CreatorID;
            this.Perks[Perk.DoubleIncome] = this.id == Program.CreatorID;
            this.Perks[Perk.BagsOCash] = this.id == Program.CreatorID;
            this.Perks[Perk.Rigged] = this.id == Program.CreatorID;
            this.Perks[Perk.TrueRigged] = this.id == Program.CreatorID;
            if (this.id != Program.CreatorID)
                foreach (Perk P in Perks)
                {
                    this.Perks[P] = true;
                }
        }

        public bool TogglePerk(Perk Perk, bool Toggle = true)
        {
            Perks[Perk] = Toggle;
            return true;
        }

        public bool Transaction(TransactionType Type, params object[] Args)
        {
            switch (Type)
            {
                case TransactionType.Add:
                    {
                        var Amount = (double)Args[0];
                        if (Amount < 0)
                        {
                            throw new ArgumentOutOfRangeException("Amount");
                        }
                        this.Bux = Bux + Amount;
                        return true;
                    }
                case TransactionType.Remove:
                    {
                        var Amount = (double)Args[0];
                        if (Amount < 0)
                        {
                            throw new ArgumentOutOfRangeException("Amount");
                        }
                        this.Bux = Bux - Amount;
                        return true;
                    }
                case TransactionType.Pay:
                    {
                        var Amount = (double)Args[0];
                        var User = (User)Args[1];
                        if (User == null)
                        {
                            throw new ArgumentNullException("User");
                        }
                        if (this.Bux < Amount)
                        {
                            throw new Exception("User has insufficient balance to complete this transaction.");
                        }
                        this.Transaction(TransactionType.Remove, Amount);
                        User.Transaction(TransactionType.Add, Amount);
                        return true;
                    }
                case TransactionType.Reward:
                    {
                        var Amount = (double)Args[0];
                        this.Transaction(TransactionType.Add, Amount);
                        return true;
                    }
                case TransactionType.Fine:
                    {
                        var Amount = (double)Args[0];
                        this.Transaction(TransactionType.Remove, Amount);
                        return true;
                    }
                case TransactionType.RawAdd:
                    {
                        var Amount = (double)Args[0];
                        Bux = Bux + Amount;
                        return true;
                    }
                case TransactionType.RawSet:
                    {
                        var Amount = (double)Args[0];
                        Bux = Amount;
                        return true;
                    }
                default:
                    throw new ArgumentNullException("Type");
            }
        }

        public string Serialize()
        {
            var x = new Dictionary<string, object>();
            x["id"] = id;
            x["Bux"] = Bux;
            x["Rank"] = (int)Rank;
            x["Perks"] = new List<int>();
            foreach (KeyValuePair<Perk, bool> P in Perks)
            {
                if (P.Value)
                {
                    ((List<int>)x["Perks"]).Add((int)P.Key);
                }
            }
            return JsonConvert.SerializeObject(x);
        }
    }

    public class Guild
    {
        // PLACEHOLDER
    }

    public class GlobalStorage
    {
        public Dictionary<ulong, User> UserStorage { get; }
        public Dictionary<ulong, Guild> GuildStorage { get; }

        public GlobalStorage()
        {
            UserStorage = new Dictionary<ulong, User>();
            GuildStorage = new Dictionary<ulong, Guild>();
        }

        public GlobalStorage(string Serial)
        {
            var x = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(Serial);
            UserStorage = new Dictionary<ulong, User>();
            GuildStorage = new Dictionary<ulong, Guild>();
            foreach (KeyValuePair<string, JToken> y in x["UserStorage"])
            {
                var i = y.Key;
                var v = y.Value;
                UserStorage[Convert.ToUInt64(i)] = new User(v.ToString());
            }
        }

        public async Task Add(DiscordGuild G)
        {
            var Members = await G.GetAllMembersAsync();
            foreach (DiscordMember M in Members)
            {
                await this.Add(M);
            }
        }

        public async Task Add(DiscordMember M)
        {
            await this.Add(new User(M.Id));
        }

        public async Task Add(User U)
        {
            if (!UserStorage.ContainsKey(U.id))
            {
                UserStorage[U.id] = U;
            }
        }

        public async Task<User> Get(ulong M)
        {
            if (this.UserStorage.ContainsKey(M))
                return this.UserStorage[M];
            await this.Add(new User(M));
            return await this.Get(M);
        }

        public string Serialize()
        {
            var x = new Dictionary<string, Dictionary<ulong, string>>();
            x["UserStorage"] = new Dictionary<ulong, string>();
            foreach (KeyValuePair<ulong, User> U in UserStorage)
            {
                x["UserStorage"][U.Key] = U.Value.Serialize();
            }
            return JsonConvert.SerializeObject(x);
        }
    }
}
