﻿using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.QoL.Res;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.QoL.SubModules
{
    public abstract class SubModule
    {
        private bool _unloaded;
        private bool _enabled;
        private Func<string> _localizedName;
        private Func<string> _localizedDescription;

        private SettingCollection _settings;
        protected SettingCollection Settings;
        protected SubModuleUI UI_Elements = new();

        public SubModule(SettingCollection settings)
        {
            _settings = settings;
        }

        public SubModuleType SubModuleType { get; protected set; } = SubModuleType.None;

        public bool Enabled { get => _enabled; set => Common.SetProperty(ref _enabled, value, OnEnabledChanged); }

        public Func<string> LocalizedName { get => _localizedName; set => Common.SetProperty(ref _localizedName, value); }

        public Func<string> LocalizedDescription { get => _localizedDescription; set => Common.SetProperty(ref _localizedDescription, value); }

        public ImageToggle ToggleControl { get; private set; }

        public DetailedTexture Icon { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public SettingEntry<bool> EnabledSetting { get; set; }

        public SettingEntry<bool> ShowInHotbar { get; set; }

        public SettingEntry<KeyBinding> HotKey { get; set; }

        public abstract void Update(GameTime gameTime);

        private void OnEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            (e.NewValue ? (Action)Enable : Disable)();
            EnabledSetting.Value = e.NewValue;
        }
        
        private void LocalizingService_LocaleChanged(object sender = null, EventArgs e = null)
        {
            SwitchLanguage();
        }

        protected virtual void Enable()
        {

        }

        protected virtual void Disable()
        {

        }

        protected virtual void SwitchLanguage()
        {
            Name = LocalizedName?.Invoke() ?? Name;
            Description = LocalizedDescription?.Invoke() ?? Description;
        }

        protected virtual void DefineSettings(SettingCollection settings)
        {
            Debug.WriteLine($"DefineSettings for {SubModuleType}");

            Settings = settings.AddSubCollection($"{SubModuleType}", true);
            Settings.RenderInUi = true;

            EnabledSetting = Settings.DefineSetting(nameof(EnabledSetting), false);

            HotKey = Settings.DefineSetting(nameof(HotKey), new KeyBinding(Keys.None), 
                () => string.Format(strings.HotkeyEntry_Name, $"{SubModuleType}"), 
                () => string.Format(strings.HotkeyEntry_Description, $"{SubModuleType}"));

            ShowInHotbar = Settings.DefineSetting(nameof(ShowInHotbar), true, 
                () => string.Format(strings.ShowInHotbar_Name, $"{SubModuleType}"), 
                () => string.Format(strings.ShowInHotbar_Description, $"{SubModuleType}"));

            HotKey.Value.Enabled = true;
            HotKey.Value.Activated += HotKey_Activated;
        }

        private void HotKey_Activated(object sender, EventArgs e)
        {
            Enabled = !Enabled;

            if(ToggleControl is ImageToggle toggle)
                toggle.Checked = Enabled;
        }

        public virtual void Load()
        {
            LocalizingService.LocaleChanged += LocalizingService_LocaleChanged;

            LocalizingService_LocaleChanged();
            DefineSettings(_settings);

            ToggleControl = ShowInHotbar.Value == false ? null : new ImageToggle()
            {
                Texture = Icon.Texture,
                HoveredTexture = Icon.HoveredTexture,
                BasicTooltipText = SubModuleType.ToString(),
                ActiveColor = Colors.Chardonnay,
                Checked = EnabledSetting.Value,
                Size = new(32),
                Visible = EnabledSetting.Value,
                OnCheckChanged = (b) =>
                {
                    EnabledSetting.Value = b;
                    ToggleControl?.Parent?.RecalculateLayout();
                },
            };
        }

        public virtual void Unload()
        {
            if (_unloaded) return;
            _unloaded = true;

            UI_Elements.DisposeAll();
            HotKey.Value.Activated -= HotKey_Activated;
            LocalizingService.LocaleChanged -= LocalizingService_LocaleChanged;
        }
    }
}
