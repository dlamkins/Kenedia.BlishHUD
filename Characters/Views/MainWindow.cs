﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Characters.Res;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Controls.SideMenu;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using static Kenedia.Modules.Core.Controls.AnchoredContainer;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;

namespace Kenedia.Modules.Characters.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly CharacterSorting _characterSorting;
        private readonly SettingsModel _settings;

        private readonly AsyncTexture2D _windowEmblem = AsyncTexture2D.FromAssetId(156015);

        private readonly ImageButton _toggleSideMenuButton;
        private readonly ImageButton _displaySettingsButton;
        private readonly ImageButton _randomButton;
        private readonly ImageButton _clearButton;
        private readonly FlowPanel _dropdownPanel;
        private readonly SideMenu _sideMenu;
        private readonly FlowPanel _buttonPanel;
        private readonly TextBox _filterBox;

        private readonly bool _created;
        private bool _uniformCharacters;
        private bool _filterCharacters;
        private double _filterTick = 0;

        private Rectangle _emblemRectangle;
        private Rectangle _titleRectangle;
        private BitmapFont _titleFont;

        public MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, CharacterSorting characterSorting, SettingsModel settings)
            : base(background, windowRegion, contentRegion)
        {
            _characterSorting = characterSorting;
            _settings= settings;

            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 35),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                AutoSizePadding = new(5, 5),
            };

            _ = new Dummy()
            {
                Parent = ContentPanel,
                Width = ContentPanel.Width,
                Height = 3,
            };

            CharactersPanel = new FlowPanel()
            {
                Parent = ContentPanel,
                Size = Size,
                ControlPadding = new Vector2(2, 4),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            DraggingControl.LeftMouseButtonReleased += DraggingControl_LeftMouseButtonReleased;

            _dropdownPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 2),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 0),
            };

            _filterBox = new TextBox()
            {
                Parent = _dropdownPanel,
                PlaceholderText = strings.Search,
                Width = 100,
            };
            _filterBox.TextChanged += FilterCharacters;
            _filterBox.Click += FilterBox_Click;
            _filterBox.EnterPressed += FilterBox_EnterPressed;

            _clearButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175783),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new Point(20, 20),
                BasicTooltipText = strings.ClearFilters,
                Visible = false,
            };
            _clearButton.Click += ClearButton_Click;

            _buttonPanel = new FlowPanel()
            {
                Parent = _dropdownPanel,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Padding = new(15),
            };
            _buttonPanel.Resized += ButtonPanel_Resized;
            _randomButton = new()
            {
                Parent = _buttonPanel,
                Size = new Point(25, 25),
                BasicTooltipText = strings.RandomButton_Tooltip,
                Texture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Dice),
                HoveredTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Dice_Hovered),
                Visible = _settings.ShowRandomButton.Value,
            };
            _randomButton.Click += RandomButton_Click;
            _settings.ShowRandomButton.SettingChanged += ShowRandomButton_SettingChanged;

            _displaySettingsButton = new()
            {
                Parent = _buttonPanel,
                Texture = AsyncTexture2D.FromAssetId(155052),
                HoveredTexture = AsyncTexture2D.FromAssetId(157110),
                Size = new Point(25, 25),
                BasicTooltipText = string.Format(strings.ShowItem, string.Format(strings.ItemSettings, strings.Display)),
            };

            _toggleSideMenuButton = new()
            {
                Parent = _buttonPanel,
                Texture = AsyncTexture2D.FromAssetId(605018),
                Size = new Point(25, 25),
                Color = ContentService.Colors.ColonialWhite,
                ColorHovered = Color.White,
                BasicTooltipText = string.Format(strings.Toggle, "Side Menu"),
            };
            _toggleSideMenuButton.Click += ToggleSideMenuButton_Click;

            _displaySettingsButton.Click += DisplaySettingsButton_Click;

            CharacterEdit = new CharacterEdit()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Anchor = this,
                AnchorPosition = AnchorPos.AutoHorizontal,
                RelativePosition = new(0, 45, 0, 0),
                Visible = false,
                FadeOut = !_settings.PinSideMenus.Value,
            };

            _sideMenu = new(_characterSorting)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Anchor = this,
                AnchorPosition = AnchorPos.AutoHorizontal,
                RelativePosition = new(0, 45, 0, 0),
                Visible = false,
                FadeOut = !_settings.PinSideMenus.Value,
            };

            AttachContainer(CharacterEdit);
            AttachContainer(_sideMenu);

            _settings.AppearanceSettingChanged += UniformCharacterCards;
            _settings.PinSideMenus.SettingChanged += PinSideMenus_SettingChanged;
            CreateCharacterControls(Characters.ModuleInstance.CharacterModels);
            _created = true;
        }

        private void PinSideMenus_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            CharacterEdit.FadeOut = !e.NewValue;
            _sideMenu.FadeOut = !e.NewValue;
        }

        private void ToggleSideMenuButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ShowAttached(_sideMenu.Visible ? null : _sideMenu);
        }

        private void ShowRandomButton_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _randomButton.Visible = _settings.ShowRandomButton.Value;
            _buttonPanel.Invalidate();
        }

        private void RandomButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            var selection = CharactersPanel.Children.Where(e => e.Visible).ToList();
            int r = RandomService.Rnd.Next(selection.Count);
            var entry = (CharacterCard)selection[r];

            entry?.Character.Swap();
        }

        private void ButtonPanel_Resized(object sender, ResizedEventArgs e)
        {
            _filterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
            _clearButton.Location = new Point(_filterBox.LocalBounds.Right - 25, _filterBox.LocalBounds.Top + 5);
        }

        private Dictionary<string, SearchFilter<Character_Model>> TagFilters => Characters.ModuleInstance.TagFilters;

        private Dictionary<string, SearchFilter<Character_Model>> SearchFilters => Characters.ModuleInstance.SearchFilters;

        public List<CharacterCard> CharacterCards { get; set; } = new List<CharacterCard>();

        public CharacterEdit CharacterEdit { get; set; }

        public DraggingControl DraggingControl { get; set; } = new DraggingControl()
        {
            Parent = GameService.Graphics.SpriteScreen,
            Visible = false,
            ZIndex = int.MaxValue - 1,
            Enabled = false,
        };

        public FlowPanel CharactersPanel { get; private set; }

        public FlowPanel ContentPanel { get; private set; }

        public void FilterCharacters(object sender = null, EventArgs e = null)
        {
            _filterCharacters = true;
        }

        public void PerformFiltering()
        {
            Regex regex = _settings.FilterAsOne.Value ? new("^(?!\\s*$).+") : new(@"\w+|""[\w\s]*""");
            var strings = regex.Matches(_filterBox.Text.Trim().ToLower()).Cast<Match>().ToList();

            List<string> textStrings = new();

            var stringFilters = new List<KeyValuePair<string, SearchFilter<Character_Model>>>();

            string SearchableString(string s)
            {
                return (_settings.FilterDiacriticsInsensitive.Value ? s.RemoveDiacritics() : s).ToLower();
            }

            foreach (object match in strings)
            {
                string string_text = SearchableString(match.ToString().Replace("\"", ""));

                if (_settings.DisplayToggles.Value["Name"].Check) stringFilters.Add(new("Name", new((c) => SearchableString(c.Name).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Profession"].Check) stringFilters.Add(new("Specialization", new((c) => SearchableString(c.SpecializationName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Profession"].Check) stringFilters.Add(new("Profession", new((c) => SearchableString(c.ProfessionName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Level"].Check) stringFilters.Add(new("Level", new((c) => SearchableString(c.Level.ToString()).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Race"].Check) stringFilters.Add(new("Race", new((c) => SearchableString(c.RaceName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Map"].Check) stringFilters.Add(new("Map", new((c) => SearchableString(c.MapName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Gender"].Check) stringFilters.Add(new("Gender", new((c) => SearchableString(c.Gender.ToString()).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["CraftingProfession"].Check)
                {
                    stringFilters.Add(new("CraftingProfession", new((c) =>
                    {
                        foreach (KeyValuePair<int, Data.CrafingProfession> craft in c.CraftingDisciplines)
                        {
                            if (!_settings.DisplayToggles.Value["OnlyMaxCrafting"].Check || craft.Key == craft.Value.MaxRating)
                            {
                                if (SearchableString(craft.Value.Name).Contains(string_text)) return true;
                            }
                        }

                        return false;
                    }, true)));
                }

                if (_settings.DisplayToggles.Value["Tags"].Check)
                {
                    stringFilters.Add(new("Tags", new((c) => { foreach (string tag in c.Tags) { if (SearchableString(tag).Contains(string_text)) return true; } return false; }, true)));
                }
            }

            bool matchAny = _settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAny;
            bool matchAll = _settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAll;

            bool include = _settings.ResultFilterBehavior.Value == FilterBehavior.Include;

            var toggleFilters = SearchFilters.Where(e => e.Value.IsEnabled).ToList();
            var tagFilters = TagFilters.Where(e => e.Value.IsEnabled).ToList();

            bool TagResult(Character_Model c)
            {
                var results = new List<bool>();
                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in tagFilters)
                {
                    bool result = filter.Value.CheckForMatch(c);
                    results.Add(result);

                    if (result)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }
                }

                return matchAll && results.Count(e => e == true) == tagFilters.Count;
            }

            bool FilterResult(Character_Model c)
            {
                var results = new List<bool>();
                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in toggleFilters)
                {
                    bool result = filter.Value.CheckForMatch(c);
                    results.Add(result);

                    if (result)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }
                }

                return matchAll && results.Count(e => e == true) == toggleFilters.Count;
            }

            bool StringFilterResult(Character_Model c)
            {
                var results = new List<bool>();

                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in stringFilters)
                {
                    bool matched = filter.Value.CheckForMatch(c);

                    if (matched)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }

                    if (matched) results.Add(matched);
                }

                return matchAll && results.Count(e => e == true) >= strings.Count;
            }

            foreach ((CharacterCard ctrl, bool toggleResult, bool stringsResult, bool tagsResult) in from CharacterCard ctrl in CharactersPanel.Children
                                                                                                        let c = ctrl.Character
                                                                                                        where c != null
                                                                                                        let toggleResult = toggleFilters.Count == 0 || (include == FilterResult(c))
                                                                                                        let stringsResult = stringFilters.Count == 0 || (include == StringFilterResult(c))
                                                                                                        let tagsResult = tagFilters.Count == 0 || (include == TagResult(c))
                                                                                                        select (ctrl, toggleResult, stringsResult, tagsResult))
            {
                ctrl.Visible = toggleResult && stringsResult && tagsResult;
            }

            SortCharacters();
            CharactersPanel.Invalidate();

            _clearButton.Visible = stringFilters.Count > 0 || toggleFilters.Count > 0 || tagFilters.Count > 0;
        }

        public void CreateCharacterControls(IEnumerable<Character_Model> models)
        {
            foreach (Character_Model c in models)
            {
                if (CharacterCards.Find(e => e.Character.Name == c.Name) == null)
                {
                    CharacterCards.Add(new CharacterCard()
                    {
                        Character = c,
                        Parent = CharactersPanel,
                        AttachedCards = CharacterCards,
                    });
                }
            }

            FilterCharacters();
            UniformCharacterCards();
            _uniformCharacters = true;
        }

        public void RequestUniform()
        {
            _uniformCharacters = true;
        }

        public void SortCharacters()
        {
            ESortOrder order = _settings.SortOrder.Value;
            ESortType sort = _settings.SortType.Value;

            switch (sort)
            {
                case ESortType.SortByName:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Character.Name.CompareTo(b.Character.Name));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => b.Character.Name.CompareTo(a.Character.Name));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByLastLogin:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => b.Character.LastLogin.CompareTo(a.Character.LastLogin));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Character.LastLogin.CompareTo(b.Character.LastLogin));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByMap:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Character.Map.CompareTo(b.Character.Map));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => b.Character.Map.CompareTo(a.Character.Map));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByProfession:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Character.Profession.CompareTo(b.Character.Profession));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterCard>((a, b) => b.Character.Profession.CompareTo(a.Character.Profession));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByTag:
                    {
                        break;
                    }

                case ESortType.Custom:
                    {
                        CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Index.CompareTo(b.Index));

                        int i = 0;
                        foreach (CharacterCard c in CharactersPanel.Children.Cast<CharacterCard>())
                        {
                            c.Index = i;
                            i++;
                        }

                        break;
                    }
            }
        }
                
        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            string versionText = $"v. {Characters.ModuleInstance.Version}";
            BasicTooltipText = MouseOverTitleBar ? versionText : null;

            if (_filterCharacters && gameTime.TotalGameTime.TotalMilliseconds - _filterTick > _settings.FilterDelay.Value)
            {
                _filterTick = gameTime.TotalGameTime.TotalMilliseconds;
                PerformFiltering();
                _filterCharacters = false;
            }

            if (_uniformCharacters)
            {
                _ = Task.Run(async () => { await Task.Delay(50); UniformCharacterCards(); });
                _uniformCharacters = false;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _emblemRectangle = new(-43, -58, 128, 128);

            _titleFont = GameService.Content.DefaultFont32;
            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(Characters.ModuleInstance.Name);

            if (titleBounds.Width > LocalBounds.Width - (_emblemRectangle.Width - 15))
            {
                _titleFont = GameService.Content.DefaultFont18;
                titleBounds = _titleFont.GetStringRectangle(Characters.ModuleInstance.Name);
            }

            _titleRectangle = new(65, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }
        
        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                _windowEmblem,
                _emblemRectangle,
                _windowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (_titleRectangle.Width < bounds.Width - (_emblemRectangle.Width - 20))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    $"{Characters.ModuleInstance.Name}",
                    _titleFont,
                    _titleRectangle,
                    ContentService.Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            CharacterCards?.ForEach(c => c.HideTooltips());
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_created)
            {
                if (ContentPanel != null)
                {
                    ContentPanel.Size = new Point(ContentRegion.Size.X, ContentRegion.Size.Y - 35);
                }

                if (_dropdownPanel != null)
                {
                    //_dropdownPanel.Size = new Point(ContentRegion.Size.X, 31);
                    _filterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
                    _clearButton.Location = new Point(_filterBox.LocalBounds.Right - 23, _filterBox.LocalBounds.Top + 6);
                }

                if (e.CurrentSize.Y < 135)
                {
                    Size = new Point(Size.X, 135);
                }

                _settings.WindowSize.Value = Size;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            DraggingControl.LeftMouseButtonReleased -= DraggingControl_LeftMouseButtonReleased;
            _filterBox.TextChanged -= FilterCharacters;
            _filterBox.Click -= FilterBox_Click;
            _filterBox.EnterPressed -= FilterBox_EnterPressed;
            _clearButton.Click -= ClearButton_Click;
            _buttonPanel.Resized -= ButtonPanel_Resized;
            _randomButton.Click -= RandomButton_Click;
            _displaySettingsButton.Click -= DisplaySettingsButton_Click;

            _settings.ShowRandomButton.SettingChanged -= ShowRandomButton_SettingChanged;
            _settings.AppearanceSettingChanged -= UniformCharacterCards;

            if (CharacterCards.Count > 0) CharacterCards?.DisposeAll();
            ContentPanel?.DisposeAll();
            CharactersPanel?.Dispose();
            DraggingControl?.Dispose();
            CharacterEdit?.Dispose();

            _dropdownPanel?.Dispose();
            _displaySettingsButton?.Dispose();
            _filterBox?.Dispose();
            _sideMenu?.Dispose();
        }

        private void FilterBox_EnterPressed(object sender, EventArgs e)
        {
            if (_settings.EnterToLogin.Value)
            {
                PerformFiltering();
                var c = (CharacterCard)CharactersPanel.Children.Where(e => e.Visible).FirstOrDefault();

                if (c != null)
                {
                    Characters.ModuleInstance.SwapTo(c.Character);
                }
            }
        }

        private void ClearButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _filterBox.Text = null;
            _filterCharacters = true;
            _sideMenu.ResetToggles();
        }

        private void FilterBox_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (_settings.OpenSidemenuOnSearch.Value) ShowAttached(_sideMenu);
        }

        private void DisplaySettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Characters.ModuleInstance.SettingsWindow.ToggleWindow();
        }

        private void DraggingControl_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SetNewIndex(DraggingControl.CharacterControl);
            DraggingControl.CharacterControl = null;
        }

        private void UniformCharacterCards(object sender = null, EventArgs e = null)
        {
            if (CharacterCards.Count > 0)
            {
                int maxWidth = CharacterCards.Max(e => e.CalculateLayout().Width);

                foreach (CharacterCard c in CharactersPanel.Children.Cast<CharacterCard>())
                {
                    c.ControlContentBounds = new(c.ControlContentBounds.Location, new(maxWidth, c.ControlContentBounds.Height));
                }
            }
        }

        private void SetNewIndex(CharacterCard characterControl)
        {
            characterControl.Index = GetHoveredIndex(characterControl);

            SortCharacters();
        }

        private double GetHoveredIndex(CharacterCard characterControl)
        {
            Blish_HUD.Input.MouseHandler m = Input.Mouse;
            CharacterCard lastControl = characterControl;

            int i = 0;
            foreach (CharacterCard c in CharactersPanel.Children.Cast<CharacterCard>())
            {
                c.Index = i;
                i++;
            }

            foreach (CharacterCard c in CharactersPanel.Children.Cast<CharacterCard>())
            {
                if (c.AbsoluteBounds.Contains(m.Position))
                {
                    return characterControl.Index > c.Index ? c.Index - 0.1 : c.Index + 0.1;
                }

                lastControl = c;
            }

            return lastControl.AbsoluteBounds.Bottom < m.Position.Y || (lastControl.AbsoluteBounds.Top < m.Position.Y && lastControl.AbsoluteBounds.Right < m.Position.X)
                ? CharacterCards.Count + 1
                : characterControl.Index;
        }
    }
}