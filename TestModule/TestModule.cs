﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Kenedia.Modules.TestModule
{
    [Export(typeof(Module))]
    public class TestModule : BaseModule<TestModule, StandardWindow, BaseSettingsModel>
    {
        private double _tick;

        [ImportingConstructor]
        public TestModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());            
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);

        }

        private void FetchAPI()
        {


        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

            }
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}