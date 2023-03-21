﻿using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum EncounterFlag : long
    {
        None = 0,
        NormalMode = 1L << 1,
        ChallengeMode = 1L << 2,
        ValeGuardian = 1L << 3,
        Gorseval = 1L << 4,
        Sabetha = 1L << 5,
        Slothasor = 1L << 6,
        BanditTrio = 1L << 7,
        Matthias = 1L << 8,
        Escort = 1L << 9,
        KeepConstruct = 1L << 10,
        Xera = 1L << 11,
        Cairn = 1L << 12,
        MursaatOverseer = 1L << 13,
        Samarog = 1L << 14,
        Deimos = 1L << 15,
        SoullessHorror = 1L << 16,
        River = 1L << 17,
        Statues = 1L << 18,
        Dhuum = 1L << 19,
        ConjuredAmalgamate = 1L << 20,
        TwinLargos = 1L << 21,
        Qadim1 = 1L << 22,
        Sabir = 1L << 23,
        Adina = 1L << 24,
        Qadim2 = 1L << 25,
        Shiverpeaks = 1L << 26,
        KodanTwins = 1L << 27,
        Fraenir = 1L << 28,
        Boneskinner = 1L << 29,
        WhisperOfJormag = 1L << 30,
        ForgingSteel = 1L << 31,
        ColdWar = 1L << 32,
        OldLionsCourt = 1L << 33,
        Aetherblade = 1L << 34,
        Junkyard = 1L << 35,
        KainengOverlook = 1L << 36,
        HarvestTemple = 1L << 37,
    }

    [Flags]
    public enum TemplateFlag
    {
        None = 0,
        Favorite = 1 << 0,
        Pve = 1 << 1,
        Pvp = 1 << 2,
        Wvw = 1 << 3,
        OpenWorld = 1 << 4,
        Dungeons = 1 << 5,
        Fractals = 1 << 6,
        Raids = 1 << 7,
        Power = 1 << 8,
        Condition = 1 << 9,
        Tank = 1 << 10,
        Support = 1 << 11,
        Heal = 1 << 12,
        Quickness = 1 << 13,
        Alacrity = 1 << 14,
        WorldCompletion = 1 << 15,
        Leveling = 1 << 16,
        Farming = 1 << 17,
    }

    public static class TemplateTagTextures
    {
        private static readonly Dictionary<EncounterFlag, AsyncTexture2D> s_encounterTextures = new()
        {
            {EncounterFlag.None, null },
            {EncounterFlag.NormalMode, AsyncTexture2D.FromAssetId(741055)},
            {EncounterFlag.ChallengeMode, AsyncTexture2D.FromAssetId(741057)},
            {EncounterFlag.ValeGuardian, AsyncTexture2D.FromAssetId(1301792)},
            {EncounterFlag.Gorseval, AsyncTexture2D.FromAssetId(1301787)},
            {EncounterFlag.Sabetha, AsyncTexture2D.FromAssetId(1301795)},
            {EncounterFlag.Slothasor, AsyncTexture2D.FromAssetId(1377392)},
            {EncounterFlag.BanditTrio, AsyncTexture2D.FromAssetId(1377389)},
            {EncounterFlag.Matthias, AsyncTexture2D.FromAssetId(1377391)},
            {EncounterFlag.Escort, AsyncTexture2D.FromAssetId(1451172)},
            {EncounterFlag.KeepConstruct, AsyncTexture2D.FromAssetId(1451173)},
            {EncounterFlag.Xera, AsyncTexture2D.FromAssetId(1451174)},
            {EncounterFlag.Cairn, AsyncTexture2D.FromAssetId(1633961)},
            {EncounterFlag.MursaatOverseer, AsyncTexture2D.FromAssetId(1633963)},
            {EncounterFlag.Samarog, AsyncTexture2D.FromAssetId(1633967)},
            {EncounterFlag.Deimos, AsyncTexture2D.FromAssetId(1633966)},
            {EncounterFlag.SoullessHorror, AsyncTexture2D.FromAssetId(1894936)},
            {EncounterFlag.Statues, AsyncTexture2D.FromAssetId(1894799)},
            {EncounterFlag.River, AsyncTexture2D.FromAssetId(1894803)},
            {EncounterFlag.Dhuum, AsyncTexture2D.FromAssetId(1894937)},
            {EncounterFlag.ConjuredAmalgamate, AsyncTexture2D.FromAssetId(2038799)},
            {EncounterFlag.TwinLargos, AsyncTexture2D.FromAssetId(2038615)},
            {EncounterFlag.Qadim1, AsyncTexture2D.FromAssetId(2038618)},
            {EncounterFlag.Sabir, AsyncTexture2D.FromAssetId(1766790)},
            {EncounterFlag.Adina, AsyncTexture2D.FromAssetId(1766806)},
            {EncounterFlag.Qadim2, AsyncTexture2D.FromAssetId(2155914)},
            {EncounterFlag.Shiverpeaks, AsyncTexture2D.FromAssetId(2221486)},
            {EncounterFlag.KodanTwins, AsyncTexture2D.FromAssetId(771054)},
            {EncounterFlag.Fraenir, AsyncTexture2D.FromAssetId(2200036)},
            {EncounterFlag.Boneskinner, AsyncTexture2D.FromAssetId(2221487)},
            {EncounterFlag.WhisperOfJormag, AsyncTexture2D.FromAssetId(2247615)},
            {EncounterFlag.ForgingSteel, AsyncTexture2D.FromAssetId(2270861)},
            {EncounterFlag.ColdWar, AsyncTexture2D.FromAssetId(2293648)},
            {EncounterFlag.Aetherblade, AsyncTexture2D.FromAssetId(740290)},
            {EncounterFlag.Junkyard, AsyncTexture2D.FromAssetId(638233)},
            {EncounterFlag.KainengOverlook, AsyncTexture2D.FromAssetId(2752298)},
            {EncounterFlag.HarvestTemple, AsyncTexture2D.FromAssetId(2595195)},
            {EncounterFlag.OldLionsCourt, AsyncTexture2D.FromAssetId(2759435)},
        };

        private static readonly Dictionary<TemplateFlag, AsyncTexture2D> s_tagTextures = new()
        {
            {TemplateFlag.None, null },
            { TemplateFlag.Favorite, AsyncTexture2D.FromAssetId(547827) }, // 156331
            { TemplateFlag.Pve, AsyncTexture2D.FromAssetId(157085) },
            { TemplateFlag.Pvp, AsyncTexture2D.FromAssetId(157119) },
            { TemplateFlag.Wvw, AsyncTexture2D.FromAssetId(255428)}, //102491
            { TemplateFlag.OpenWorld, AsyncTexture2D.FromAssetId(255280) }, //460029 , 156625
            { TemplateFlag.Dungeons, AsyncTexture2D.FromAssetId(102478) }, //102478 , 866140
            { TemplateFlag.Fractals,AsyncTexture2D.FromAssetId(514379) }, // 1441449
            { TemplateFlag.Raids,AsyncTexture2D.FromAssetId(1128644) },
            { TemplateFlag.Power, AsyncTexture2D.FromAssetId(66722) },
            { TemplateFlag.Condition, AsyncTexture2D.FromAssetId(156600) },
            { TemplateFlag.Tank, AsyncTexture2D.FromAssetId(536048) },
            { TemplateFlag.Support, AsyncTexture2D.FromAssetId(156599) },
            { TemplateFlag.Heal, AsyncTexture2D.FromAssetId(536052) },
            { TemplateFlag.Quickness, AsyncTexture2D.FromAssetId(1012835) },
            { TemplateFlag.Alacrity, AsyncTexture2D.FromAssetId(1938787) },
            { TemplateFlag.WorldCompletion, AsyncTexture2D.FromAssetId(460029) },
            { TemplateFlag.Leveling, AsyncTexture2D.FromAssetId(993668) },
            { TemplateFlag.Farming, AsyncTexture2D.FromAssetId(784331) },
        };

        private static readonly Dictionary<EncounterFlag, Rectangle> s_encounterTexturesRegions = new()
        {

        };

        private static readonly Dictionary<TemplateFlag, Rectangle> s_tagTextureRegions = new()
        {
            {TemplateFlag.None, Rectangle.Empty },
            { TemplateFlag.Favorite, new(4, 4, 24, 24)},
            { TemplateFlag.Pve, Rectangle.Empty },
            { TemplateFlag.Pvp,  new(-2, -2, 36, 36) },
            { TemplateFlag.Wvw,  new(2,  2, 28, 28) },
            { TemplateFlag.OpenWorld, new(2,  2, 28, 28) },
            { TemplateFlag.Dungeons, new(-2,  -2, 36, 36) },
            { TemplateFlag.Fractals,  new(-4, -4, 40, 40) },
            { TemplateFlag.Raids,  new(-2, -2, 36, 36) },
            { TemplateFlag.Power, new(2, 2, 28, 28) },
            { TemplateFlag.Condition, new(2, 2, 28, 28) },
            { TemplateFlag.Tank, new(2, 2, 28, 28) },
            { TemplateFlag.Support, new(2, 2, 28, 28) },
            { TemplateFlag.Heal, new(2, 2, 28, 28) },
            { TemplateFlag.Quickness, new(-4, -4, 40, 40)},
            { TemplateFlag.Alacrity, new(-4, -4, 40, 40) },
            { TemplateFlag.WorldCompletion, new(-16, -16, 160, 160)},
            { TemplateFlag.Leveling,Rectangle.Empty },
            { TemplateFlag.Farming, new(-4, -4, 40, 40) },
        };

        public static TagTexture GetDetailedTexture(this TemplateFlag tag)
        {
            return s_tagTextures.TryGetValue(tag, out var texture) ? new(texture)
            {
                TextureRegion = s_tagTextureRegions.ContainsKey(tag) && s_tagTextureRegions[tag] != Rectangle.Empty ? s_tagTextureRegions[tag] : texture.Texture.Bounds,
                TemplateTag = tag,
            } : null;
        }

        public static TagTexture GetDetailedTexture(this EncounterFlag tag)
        {
            return s_encounterTextures.TryGetValue(tag, out var texture) ? new(texture)
            {
                TextureRegion = s_encounterTexturesRegions.ContainsKey(tag) && s_encounterTexturesRegions[tag] != Rectangle.Empty ? s_encounterTexturesRegions[tag] : texture.Texture.Bounds,
                TemplateTag = tag,
            } : null;
        }

        public static AsyncTexture2D GetTexture(this TemplateFlag tag)
        {
            return s_tagTextures.TryGetValue(tag, out var texture) ? texture : null;
        }

        public static AsyncTexture2D GetTexture(this EncounterFlag tag)
        {
            return s_encounterTextures.TryGetValue(tag, out var texture) ? texture : null;
        }
    }

    [DataContract]
    public class Template
    {
        private readonly BuildTemplate _buildTemplate = new();
        private readonly GearTemplate _gearTemplate = new();
        private ProfessionType _profession;
        private string _description;
        private string _name = "Power Deadeye - Dagger/Dagger | Shortbow";
        private string _id;
        private bool _terrestrial = true;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Earth;
        private LegendSlot _legendSlot = LegendSlot.TerrestrialActive;
        private bool loaded = true;

        public Template()
        {
            _buildTemplate.Changed += TemplateChanged;
            _gearTemplate.PropertyChanged += TemplateChanged;

        }

        public event PropertyChangedEventHandler Changed;

        //[DataMember]
        //public string Id { get => _id; set => Common.SetProperty(ref _id, value, Changed); }

        public ObservableList<string> TextTags { get; private set; } = new();

        [DataMember]
        public TemplateFlag Tags { get => _tags; set => Common.SetProperty(ref _tags , value, TemplateChanged); }

        [DataMember]
        public EncounterFlag Encounters { get => _encounters; set => Common.SetProperty(ref _encounters , value, TemplateChanged); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, TemplateChanged); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, TemplateChanged); }

        [DataMember]
        public string GearCode
        {
            get => GearTemplate?.ParseGearCode();
            set => Gearcode = value;
        }

        [DataMember]
        public string BuildCode
        {
            get => BuildTemplate?.ParseBuildCode();
            set => Buildcode = value;
        }

        public ProfessionType Profession
        {
            get => BuildTemplate.Profession;
            set
            {
                if (Common.SetProperty(ref _profession, value, TemplateChanged))
                {
                    if (BuildTemplate != null)
                    {
                        BuildTemplate.Profession = value;
                        //GearTemplate.Profession = value;
                    }
                }
            }
        }

        [DataMember]
        public Races Race = Races.None;
        private bool _pvE = true;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _eventCancellationTokenSource;
        private TemplateFlag _tags;
        private EncounterFlag _encounters;

        private string Gearcode
        {
            set
            {
                loaded = false;
                GearTemplate?.LoadFromCode(value);
                loaded = true;
            }
        }

        private string Buildcode
        {
            set
            {
                loaded = false;
                BuildTemplate?.LoadFromCode(value);
                loaded = true;
            }
        }

        public TemplateAttributes Attributes { get; private set; } = new();

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, TemplateChanged); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, TemplateChanged); }

        public bool Terrestrial
        {
            get => LegendSlot is LegendSlot.TerrestrialActive or LegendSlot.TerrestrialInactive;
            set
            {
                switch (LegendSlot)
                {
                    case LegendSlot.AquaticActive:
                    case LegendSlot.AquaticInactive:
                        LegendSlot newTerrestialSlot = LegendSlot is LegendSlot.AquaticActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;
                        _ = Common.SetProperty(ref _legendSlot, newTerrestialSlot, TemplateChanged);

                        break;

                    case LegendSlot.TerrestrialActive:
                    case LegendSlot.TerrestrialInactive:
                        LegendSlot newAquaticSlot = LegendSlot is LegendSlot.TerrestrialActive ? LegendSlot.AquaticActive : LegendSlot.AquaticInactive;
                        _ = Common.SetProperty(ref _legendSlot, newAquaticSlot, TemplateChanged);
                        break;
                }
            }
        }

        public LegendSlot LegendSlot
        {
            get => _legendSlot;
            set => Common.SetProperty(ref _legendSlot, value, TemplateChanged);
        }

        /// <summary>
        /// Active Transform Skill which sets weapon skills to its childs and disables all others
        /// </summary>
        public Skill ActiveTransform { get; set; }

        /// <summary>
        /// Active Bundle Skill which sets weapon skills to its childs
        /// </summary>
        public Skill ActiveBundle { get; set; }

        public BuildTemplate BuildTemplate
        {
            get => _buildTemplate; 
            //set
            //{
            //    var prev = _buildTemplate;

            //    if (Common.SetProperty(ref _buildTemplate, value, Changed))
            //    {
            //        if (prev != null) prev.Changed -= TemplateChanged;

            //        _buildTemplate ??= new();
            //        _buildTemplate.Changed += TemplateChanged;
            //    }
            //}
        }

        public GearTemplate GearTemplate
        {
            get => _gearTemplate; 
            //set
            //{
            //    var prev = _gearTemplate;
            //    if (Common.SetProperty(ref _gearTemplate, value, Changed))
            //    {
            //        if (prev != null) prev.PropertyChanged -= TemplateChanged;

            //        _gearTemplate ??= new();
            //        _gearTemplate.PropertyChanged += TemplateChanged;
            //    }
            //}
        }

        public bool PvE { get => _pvE; internal set => Common.SetProperty(ref _pvE, value, TemplateChanged); }

        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        public SkillCollection GetActiveSkills()
        {
            return LegendSlot switch
            {
                LegendSlot.AquaticInactive => BuildTemplate.InactiveAquaticSkills,
                LegendSlot.TerrestrialInactive => BuildTemplate.InactiveTerrestrialSkills,
                LegendSlot.AquaticActive => BuildTemplate.AquaticSkills,
                LegendSlot.TerrestrialActive => BuildTemplate.TerrestrialSkills,
                _ => null,
            };
        }

        private async void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MainAttunement != AltAttunement && EliteSpecialization?.Id != (int)SpecializationType.Weaver)
            {
                AltAttunement = MainAttunement;
            }

            Changed?.Invoke(sender, e);
            await Save();
        }

        private async Task TriggerChanged(object sender, PropertyChangedEventArgs e)
        {
            _eventCancellationTokenSource?.Cancel();
            _eventCancellationTokenSource = new();

            try
            {
                await Task.Delay(1, _eventCancellationTokenSource.Token);
                if (!_eventCancellationTokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine($"{nameof(Template)} Trigger Changed Event now.");
                    Changed?.Invoke(sender, e);
                }
            }
            catch(Exception)
            {

            }
        }

        public async Task<bool> ChangeName(string name)
        {
            bool unlocked = await FileExtension.WaitForFileUnlock(FilePath);

            if (!unlocked)
            {
                return false;
            }

            File.Delete(FilePath);
            Name = name;

            await Save();
            return true;
        }

        public async Task Save()
        {
            if (!loaded) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string path = BuildsManager.ModuleInstance.Paths.TemplatesPath;
                    if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);
                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText($@"{path}\{Common.MakeValidFileName(Name.Trim())}.json", json);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
