﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SemVer;
using System;
using System.Collections;
using System.Collections.Generic;
using Kenedia.Modules.Core.Models;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Version = SemVer.Version;
using Kenedia.Modules.BuildsManager.Services;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class SemverVersionConverter : JsonConverter<Version>
    {
        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : new((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class JsonSemverVersionAttribute : Attribute
    {
    }

    public class SemverVersionContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (Attribute.IsDefined(member, typeof(JsonSemverVersionAttribute)))
            {
                property.Converter = new SemverVersionConverter();
            }

            return property;
        }
    }

    public class StaticVersion
    {
        public StaticVersion()
        {

        }

        public StaticVersion(Version version)
        {
            Nourishments = version;
            Enhancements = version;
            PveRunes = version;
            PvpRunes = version;
            PveSigils = version;
            PvpSigils = version;
            Infusions = version;
            Enrichments = version;
            Trinkets = version;
            Backs = version;
            Weapons = version;
            Armors = version;
            PowerCores = version;
            Relics = version;
            PvpAmulets = version;
            Stats = version;
        }

        [JsonSemverVersion]
        public Version Nourishments { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Enhancements { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PveRunes { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpRunes { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PveSigils { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpSigils { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Infusions { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Enrichments { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Trinkets { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Backs { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Weapons { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Armors { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PowerCores { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Relics { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpAmulets { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Stats { get; set; } = new(0, 0, 0);

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, Version>> GetEnumerator()
        {
            yield return new KeyValuePair<string, Version>(nameof(Nourishments), Nourishments);
            yield return new KeyValuePair<string, Version>(nameof(Enhancements), Enhancements);
            yield return new KeyValuePair<string, Version>(nameof(PveRunes), PveRunes);
            yield return new KeyValuePair<string, Version>(nameof(PvpRunes), PvpRunes);
            yield return new KeyValuePair<string, Version>(nameof(PveSigils), PveSigils);
            yield return new KeyValuePair<string, Version>(nameof(PvpSigils), PvpSigils);
            yield return new KeyValuePair<string, Version>(nameof(Infusions), Infusions);
            yield return new KeyValuePair<string, Version>(nameof(Enrichments), Enrichments);
            yield return new KeyValuePair<string, Version>(nameof(Trinkets), Trinkets);
            yield return new KeyValuePair<string, Version>(nameof(Backs), Backs);
            yield return new KeyValuePair<string, Version>(nameof(Weapons), Weapons);
            yield return new KeyValuePair<string, Version>(nameof(Armors), Armors);
            yield return new KeyValuePair<string, Version>(nameof(PowerCores), PowerCores);
            yield return new KeyValuePair<string, Version>(nameof(Relics), Relics);
            yield return new KeyValuePair<string, Version>(nameof(PvpAmulets), PvpAmulets);
        }

        public void SaveToJson(string path)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new SemverVersionContractResolver(),
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
            System.IO.File.WriteAllText(path, json);
        }

        public Version this[string propertyName]
        {
            get
            {
                var propertyInfo = GetType().GetProperty(propertyName);

                return propertyInfo != null
                    ? (Version)propertyInfo.GetValue(this)
                    : throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
            }
            set
            {
                var propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, value);
                }
                else
                {
                    throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
                }
            }
        }
    }

    public class ByteIntMap
    {
        public ByteIntMap() { }

        public ByteIntMap(Version version)
        {
            Version = version;
        }

        public Dictionary<byte, int> Items { get; } = new();

        [JsonSemverVersion]
        public Version Version { get; set; } = new(0, 0, 0);

        public int this[byte key]
        {
            get => Items[key];
            set => Items[key] = value;
        }

        public void Add(byte key, int value)
        {
            Items.Add(key, value);
        }

        public void Remove(byte key)
        {
            _ = Items.Remove(key);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool ContainsKey(byte key)
        {
            return Items.ContainsKey(key);
        }

        public bool TryGetValue(byte key, out int value)
        {
            return Items.TryGetValue(key, out value);
        }

        [JsonIgnore]
        public int Count => Items.Count;

        [JsonIgnore]
        public IEnumerable<byte> Keys => Items.Keys;

        [JsonIgnore]
        public IEnumerable<int> Values => Items.Values;

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<byte, int>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void SaveToJson(string path)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new SemverVersionContractResolver(),
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(path, json);
        }
    }

    public class ByteIntMapCollection
    {
        private readonly Paths _paths;

        public ByteIntMapCollection(Paths paths)
        {
            _paths = paths;
        }

        public ByteIntMapCollection(Version version, Paths paths) : this(paths)
        {
            foreach (var itemMap in this)
            {
                itemMap.Value.Version = version;
            }
        }

        public ByteIntMap Nourishments { get; } = new();

        public ByteIntMap Enhancements { get; } = new();

        public ByteIntMap PveRunes { get; } = new();

        public ByteIntMap PvpRunes { get; } = new();

        public ByteIntMap PveSigils { get; } = new();

        public ByteIntMap PvpSigils { get; } = new();

        public ByteIntMap Infusions { get; } = new();

        public ByteIntMap Enrichments { get; } = new();

        public ByteIntMap Trinkets { get; } = new();

        public ByteIntMap Backs { get; } = new();

        public ByteIntMap Weapons { get; } = new();

        public ByteIntMap Armors { get; } = new();

        public ByteIntMap PowerCores { get; } = new();

        public ByteIntMap Relics { get; } = new();

        public ByteIntMap PvpAmulets { get; } = new();

        public ByteIntMap Stats { get; } = new();

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, ByteIntMap>> GetEnumerator()
        {
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Nourishments), Nourishments);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Enhancements), Enhancements);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PveRunes), PveRunes);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpRunes), PvpRunes);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PveSigils), PveSigils);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpSigils), PvpSigils);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Infusions), Infusions);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Enrichments), Enrichments);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Trinkets), Trinkets);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Backs), Backs);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Weapons), Weapons);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Armors), Armors);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PowerCores), PowerCores);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Relics), Relics);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpAmulets), PvpAmulets);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Stats), Stats);
        }

        public async Task<bool> FetchAndLoad()
        {
            try
            {
                var versions = await StaticHosting.GetStaticVersion();
                string path = _paths.ItemMapPath;

                foreach (var itemMap in this)
                {
                    string filePath = Path.Combine(path, $"{itemMap.Key}.json");
                    var prop = typeof(ByteIntMapCollection).GetProperty(itemMap.Key);

                    if (prop != null)
                    {
                        ByteIntMap value = null;

                        if (File.Exists(filePath))
                        {
                            string json = File.ReadAllText(filePath);
                            value = JsonConvert.DeserializeObject<ByteIntMap>(json);
                        }

                        if (value is null || value.Version < versions[itemMap.Key])
                        {
                            BuildsManager.Logger.Info($"Updating {itemMap.Key} item map from version {value?.Version?.ToString() ?? "0.0.0"} to {versions[itemMap.Key]}");
                            value = await StaticHosting.GetItemMap(itemMap.Key);

                            BuildsManager.Logger.Info($"Added {(value?.Count ?? 0) - (itemMap.Value?.Count ?? 0)} new mapped entries.");
                            value?.SaveToJson(filePath);
                        }
                        else
                        {
                            BuildsManager.Logger.Info($"Loaded {itemMap.Key} item map version {value.Version} which is the most recent version.");
                        }

                        prop.SetValue(this, value);
                    }
                }

                return true;
            }
            catch { }

            return false;
        }

        public void Save()
        {
            try
            {
                foreach (var itemMap in this)
                {
                    string filePath = Path.Combine(_paths.ItemMapPath, $"{itemMap.Key}.json");

                    itemMap.Value?.SaveToJson(filePath);
                }
            }
            catch { }
        }
    }
}