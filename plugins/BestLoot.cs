using System;
using System.Collections.Generic;
using System.Linq;

using Oxide.Core;
using Rust;

using Random = System.Random;

namespace Oxide.Plugins
{
    [Info("Best Loot", "Jacob", "1.0.0", ResourceId = 0)]
    [Description("Complex loot managment system.")]

    class BestLoot : RustPlugin
    {
        #region Configuration 

        private class Configuration
        {

            public Configuration()
            { 
            }

            private void GetConfig<T>(ref T variable, params string[] path)
            {
                if (path.Length == 0) return;

                if (Instance.Config.Get(path) == null)
                {
                    SetConfig(ref variable, path);
                    Instance.PrintWarning($"Added field to config: {string.Join("/", path)}");
                }

                variable = (T)Convert.ChangeType(Instance.Config.Get(path), typeof(T));
            }

            private void SetConfig<T>(ref T variable, params string[] path) => Instance.Config.Set(path.Concat(new object[] { variable }).ToArray());
        }

        protected override void LoadDefaultConfig() => PrintWarning("Generating new config file...");

        #endregion

        #region Data

        private class Data
        {
            public Dictionary<string, object> ItemRarities = new Dictionary<string, object>();

            public Data()
            {
                ReadData(ref ItemRarities, "ItemRarities");

                foreach (var itemDefinition in ItemManager.itemList)
                {
                    object rarity;
                    if (ItemRarities.TryGetValue(itemDefinition.shortname, out rarity))
                    {
                        itemDefinition.rarity = (Rarity) Convert.ToInt32(rarity);
                        continue;
                    }

                    ItemRarities.Add(itemDefinition.shortname, itemDefinition.rarity);
                }

                SaveData(ItemRarities, "ItemRarities");
            }

            private void SaveData<T>(T data, string file) => Interface.Oxide.DataFileSystem.WriteObject($"{Instance.Name}/{file}", data);

            private void ReadData<T>(ref T data, string file) => data = Interface.Oxide.DataFileSystem.ReadObject<T>($"{Instance.Name}/{file}");
        }

        private class LootTuple
        {
            public bool Blueprint { get; }
            public bool Rarity { get; set; }
            public string ShortName { get; }
            public bool Skinned { get; }

            public LootTuple()
            {

            }
        }

        #endregion

        #region Fields

        public static BestLoot Instance;
        private Configuration configuration;
        private static Data data;

        private Random random = new Random();
        private bool initialized;

        #endregion

        #region Oxide Hooks

        private void Init() => Instance = this;

        private void OnServerInitialized()
        {
            if (ItemManager.itemList == null)
                timer.Once(1f, OnServerInitialized);


            configuration = new Configuration();
            data = new Data();
        }

        #endregion
    }
}