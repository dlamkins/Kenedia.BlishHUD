﻿using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD;
using System;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using static Blish_HUD.ContentService;
using System.Diagnostics;
using Blish_HUD.Content;
using System.Diagnostics.Eventing.Reader;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _texture = new();
        private readonly DetailedTexture _statTexture = new() { };

        private DetailedTexture _placeholder = new();
        private BaseItem _item;
        private Stat _stat;
        private int _frameThickness = 2;
        private Color _frameColor = Color.White * 0.15F;

        public ItemControl()
        {
            Tooltip = new ItemTooltip();
        }

        public ItemControl(DetailedTexture placeholder) : this()
        {
            Placeholder = placeholder;
        }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, ApplyStat); }

        public Color TextureColor { get => _texture.DrawColor ?? Color.White; set => _texture.DrawColor = value; }

        public DetailedTexture Placeholder { get => _placeholder; set => Common.SetProperty(ref _placeholder, value, ApplyPlaceholder); }

        private void ApplyPlaceholder(object sender, Core.Models.ValueChangedEventArgs<DetailedTexture> e)
        {

        }

        private void ApplyStat(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;

            if (Tooltip is ItemTooltip itemTooltip)
                itemTooltip.Stat = Stat;
        }

        private void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<BaseItem> e)
        {
            _frameColor = Item?.Rarity.GetColor() ?? Color.White * 0.15F;
            _texture.Texture = Item?.Icon;

            if (Tooltip is ItemTooltip itemTooltip)
                itemTooltip.Item = Item;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _frameThickness = CalculateFrameThickness();

            _statTexture.Bounds = new(_texture.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));
            _texture.Bounds = new(_frameThickness, _frameThickness, Width - (_frameThickness * 2), Height - (_frameThickness * 2));
            _placeholder.Bounds = new(_frameThickness, _frameThickness, Width - (_frameThickness * 2), Height - (_frameThickness * 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            if (Item == null)
            {
                _placeholder.Draw(this, spriteBatch, RelativeMousePosition, TextureColor);
            }
            else
            {
                _texture.Draw(this, spriteBatch, RelativeMousePosition, TextureColor);
            }

            spriteBatch.DrawFrame(this, bounds, _frameColor, _frameThickness);
            _statTexture.Draw(this, spriteBatch);
        }

        private int CalculateFrameThickness()
        {
            int size = Math.Min(Width, Height);

            return Math.Max(2, size / 24);
        }
    }
}
