﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Characters.Res;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuToggles : FlowTab, ILocalizable
    {
        private List<Tag> _tags = new();
        private readonly FlowPanel _toggleFlowPanel;
        private readonly FlowPanel _tagFlowPanel;
        private readonly List<KeyValuePair<ImageColorToggle, Action>> _toggles = new();
        private readonly TextureManager _textureManager;
        private readonly SearchFilterCollection _tagFilters;
        private readonly SearchFilterCollection _searchFilters;
        private readonly Action _onFilterChanged;
        private readonly TagList _allTags;
        private readonly Data _data;
        private Rectangle _contentRectangle;

        public event EventHandler TogglesChanged;

        public SideMenuToggles(TextureManager textureManager, SearchFilterCollection tagFilters, SearchFilterCollection searchFilters, Action onFilterChanged, TagList allTags, Data data)
        {
            _textureManager = textureManager;
            _tagFilters = tagFilters;
            _searchFilters = searchFilters;
            _onFilterChanged = onFilterChanged;
            _allTags = allTags;
            _data = data;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            _toggleFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 3),
                Height = 286,
                Width = Width,
            };

            _tagFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 3),
                Width = Width,
            };

            CreateToggles();
            CreateTags();

            _ = Task.Run(async () => { await Task.Delay(250); CalculateTagPanelSize(); });
            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            _allTags.CollectionChanged += Tags_CollectionChanged;
            OnLanguageChanged();
        }

        public void ResetToggles()
        {
            _tags.ForEach(t => t.SetActive(false));
            _toggles.ForEach(t => t.Key.Active = false);

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in _searchFilters)
            {
                t.Value.IsEnabled = false;
            }

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in _tagFilters)
            {
                t.Value.IsEnabled = false;
            }
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreateTags();
            CalculateTagPanelSize();
        }

        private void CalculateTagPanelSize()
        {
            if (Visible)
            {
                int width = _tagFlowPanel.Width - (int)(_tagFlowPanel.OuterControlPadding.X * 2);
                int? height = null;

                int curWidth = 0;
                foreach (Tag tag in _tags)
                {
                    height ??= tag.Height + (int)_tagFlowPanel.ControlPadding.Y + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);

                    int newWidth = curWidth + tag.Width + (int)_tagFlowPanel.ControlPadding.X;

                    if (newWidth >= width)
                    {
                        height += tag.Height + (int)_tagFlowPanel.ControlPadding.Y;
                        curWidth = 0;
                    }

                    curWidth += tag.Width + (int)_tagFlowPanel.ControlPadding.X;
                }

                _tagFlowPanel.Height = (height ?? 0) + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);
            }
        }

        private void CreateTags()
        {
            _tags.ForEach(t => { t.ActiveChanged -= Tag_ActiveChanged; t.Deleted-= Tag_Deleted; });
            _tags.DisposeAll();
            _tags.Clear();

            _tagFlowPanel.Children.Clear();
            _tagFilters.Clear();

            foreach (string tag in _allTags)
            {
                if (!_tagFilters.ContainsKey(tag))
                {
                    Tag t;
                    _tags.Add(t = new Tag()
                    {
                        Parent = _tagFlowPanel,
                        Text = tag,
                        CanInteract = true,
                        ShowDelete = true,
                    });

                    _tagFilters.Add(tag, new((c) => c.Tags.Contains(tag), false));

                    t.SetActive(false);
                    t.ActiveChanged += Tag_ActiveChanged;
                    t.Deleted += Tag_Deleted;
                }
            }
        }

        private void Tag_Deleted(object sender, EventArgs e)
        {
            _ = _tags.Remove(sender as Tag);
            _ = _allTags.Remove((sender as Tag).Text);
        }

        private void Tag_ActiveChanged(object sender, EventArgs e)
        {
            var t = (Tag)sender;
            _tagFilters[t.Text].IsEnabled = t.Active;
            _onFilterChanged?.Invoke();
        }

        private void CreateToggles()
        {
            void action(bool active, string entry)
            {
                _searchFilters[entry].IsEnabled = active;
                _onFilterChanged?.Invoke();
            }

            var profs = _data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
            profs = profs.OrderBy(e => e.Value.WeightClass).ThenBy(e => e.Value.APIId).ToDictionary(e => e.Key, e => e.Value);

            // Profession All Specs
            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                var t = new ImageColorToggle((b) => action(b, $"Core {profession.Value.Name}"))
                {
                    Texture = profession.Value.IconBig,
                    UseGrayScale = false,
                    ColorActive = profession.Value.Color,
                    ColorHovered = profession.Value.Color,
                    ColorInActive = profession.Value.Color * 0.5f,
                    Active = _searchFilters[$"Core {profession.Value.Name}"].IsEnabled,
                    BasicTooltipText = $"Core {profession.Value.Name}",
                    Alpha = 0.7f,
                };

                KeyValuePair<ImageColorToggle, Action> tt = new (t, () => t.BasicTooltipText = $"Core {profession.Value.Name}");
                _toggles.Add(tt);
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                var t = new ImageColorToggle((b) => action(b, profession.Value.Name))
                {
                    Texture = profession.Value.IconBig,
                    Active = _searchFilters[profession.Value.Name].IsEnabled,
                    BasicTooltipText = profession.Value.Name,                    
                };
                _toggles.Add(new(t, () => t.BasicTooltipText = profession.Value.Name));
            }

            List<KeyValuePair<ImageColorToggle, Action>> specToggles = new();
            foreach (KeyValuePair<SpecializationType, Data.Specialization> specialization in _data.Specializations)
            {
                var t = new ImageColorToggle((b) => action(b, specialization.Value.Name))
                {
                    Texture = specialization.Value.IconBig,
                    Profession = specialization.Value.Profession,
                    Active = _searchFilters[specialization.Value.Name].IsEnabled,
                    BasicTooltipText = specialization.Value.Name,
                };
                specToggles.Add(new(t, () => t.BasicTooltipText = specialization.Value.Name));
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> p in profs)
                {
                    KeyValuePair<ImageColorToggle, Action> t = specToggles.Find(e => p.Key == e.Key.Profession && !_toggles.Contains(e));
                    if (t.Key != null)
                    {
                        _toggles.Add(t);
                    }
                }
            }

            // Crafting Professions
            foreach (KeyValuePair<int, Data.CraftingProfession> crafting in _data.CrafingProfessions)
            {

                if (crafting.Key > 0)
                {
                    ImageColorToggle img = new((b) => action(b, crafting.Value.Name))
                    {
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24),
                        SizeRectangle = new Rectangle(4, 4, 20, 20),
                        Active = _searchFilters[crafting.Value.Name].IsEnabled,
                        BasicTooltipText = crafting.Value.Name,
                    };
                    _toggles.Add(new(img, () => img.BasicTooltipText = crafting.Value.Name));
                }
            }

            var hidden = new ImageColorToggle((b) => action(b, "Hidden"))
            {
                Texture = AsyncTexture2D.FromAssetId(605021),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                BasicTooltipText = strings.ShowHidden_Tooltip,
            };
            _toggles.Add(new(hidden, () => hidden.BasicTooltipText = strings.ShowHidden_Tooltip));

            var birthday = new ImageColorToggle((b) => action(b, "Birthday"))
            {
                Texture = AsyncTexture2D.FromAssetId(593864),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(birthday, () => birthday.BasicTooltipText = strings.Show_Birthday_Tooltip));

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in _data.Races)
            {
                var t = new ImageColorToggle((b) => action(b, race.Value.Name))
                {
                    Texture = race.Value.Icon,
                    UseGrayScale = true,
                    BasicTooltipText = race.Value.Name 
                };

                _toggles.Add(new(t, () => t.BasicTooltipText = race.Value.Name));
            }

            var male = new ImageColorToggle((b) => action(b, "Male"))
            {
                Texture = _textureManager.GetIcon(TextureManager.Icons.Male),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(male, () => male.BasicTooltipText = strings.Male));

            var female = new ImageColorToggle((b) => action(b, "Female"))
            {
                Texture = _textureManager.GetIcon(TextureManager.Icons.Female),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(female, () => female.BasicTooltipText = strings.Female));

            var j = 0;
            foreach (KeyValuePair<ImageColorToggle, Action> t in _toggles)
            {
                j++;
                t.Key.Parent = _toggleFlowPanel;
                t.Key.Size = new Point(29, 29);
            }
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _toggles.ForEach(t => t.Value.Invoke());
        }

        public void OnTogglesChanged(object s = null, EventArgs e = null)
        {
            TogglesChanged?.Invoke(this, e);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            GameService.Overlay.UserLocale.SettingChanged -= OnLanguageChanged;
            _allTags.CollectionChanged -= Tags_CollectionChanged;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _contentRectangle = new Rectangle((int)OuterControlPadding.X, (int)OuterControlPadding.Y, Width - ((int)OuterControlPadding.X * 2), Height - ((int)OuterControlPadding.Y * 2));
            _toggleFlowPanel.Width = _contentRectangle.Width;

            _tagFlowPanel.Width = _contentRectangle.Width;
            CalculateTagPanelSize();
        }
    }
}
