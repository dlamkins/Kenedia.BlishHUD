﻿using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using System.Linq;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;
using Kenedia.Modules.BuildsManager.Res;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class WeaponSlot : GearSlot
    {
        private readonly DetailedTexture _changeWeaponTexture = new(2338896, 2338895)
        {
            TextureRegion = new(4, 4, 24, 24),
            DrawColor = Color.White * 0.5F,
            HoverDrawColor = Color.White,
        };

        private readonly ItemControl _sigilControl = new(new(784324) { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _pvpSigilControl = new(new(784324) { TextureRegion = new(38, 38, 52, 52) }) { Visible = false };
        private readonly ItemControl _infusionControl = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;
        private Sigil _sigil;
        private Sigil _pvpSigil;
        private Infusion _infusion;

        private Rectangle _sigilBounds;
        private Rectangle _pvpSigilBounds;
        private Rectangle _infusionBounds;
        private WeaponSlot _otherHandSlot;

        public WeaponSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _sigilControl.Parent = this;
            _pvpSigilControl.Parent = this;
            _infusionControl.Parent = this;

            TemplatePresenter.GameModeChanged += TemplatePresenter_GameModeChanged;
        }

        public event EventHandler<Weapon> WeaponChanged;
        public event EventHandler<Stat> StatChanged;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil { get => _sigil; set => Common.SetProperty(ref _sigil, value, OnSigilChanged); }

        public Sigil PvpSigil { get => _pvpSigil; set => Common.SetProperty(ref _pvpSigil, value, OnPvpSigilChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public WeaponSlot OtherHandSlot { get => _otherHandSlot; set => Common.SetProperty(ref _otherHandSlot, value, OnOtherHandSlotChanged); }

        private void OnOtherHandSlotChanged(object sender, Core.Models.ValueChangedEventArgs<WeaponSlot> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.WeaponChanged -= OtherHandSlot_WeaponChanged;
                e.OldValue.StatChanged -= OtherHandSlot_StatChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.WeaponChanged += OtherHandSlot_WeaponChanged;
                e.NewValue.StatChanged += OtherHandSlot_StatChanged;
            }
        }

        private void OtherHandSlot_StatChanged(object sender, Stat e)
        {
            if (OtherHandSlot is not null && OtherHandSlot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand && (OtherHandSlot.Item as Weapon)?.WeaponType.IsTwoHanded() == true && OtherHandSlot.Stat != Stat)
            {
                Stat = OtherHandSlot.Stat;
                return;
            }
        }

        private void OtherHandSlot_WeaponChanged(object sender, Weapon e)
        {
            AdjustForOtherSlot();
        }

        private void AdjustForOtherSlot()
        {
            if (OtherHandSlot is not null && OtherHandSlot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand)
            {
                var otherHandWeapon = OtherHandSlot.Item as Weapon;
                if (otherHandWeapon?.WeaponType.IsTwoHanded() != false)
                {
                    Item = OtherHandSlot.Item;
                    Stat = OtherHandSlot.Stat;

                    ItemControl.Opacity = 0.6F;
                    return;
                }
                else if ((Item as Weapon)?.WeaponType.IsTwoHanded() == true)
                {
                    Item = null;
                    Stat = null;
                    ItemControl.Opacity = 1F;
                    return;
                }
            }

            ItemControl.Opacity = 1F;
        }

        private void TemplatePresenter_GameModeChanged(object sender, Core.Models.ValueChangedEventArgs<GameModeType> e)
        {
            if (e.NewValue == GameModeType.PvP)
            {
                _sigilControl.Visible = false;
                _infusionControl.Visible = false;
                _pvpSigilControl.Visible = true;
            }
            else
            {
                _sigilControl.Visible = true;
                _infusionControl.Visible = true;
                _pvpSigilControl.Visible = false;
            }
        }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Stat = overrideExisting ? stat : entry.Stat ?? stat;
                    (slot as AquaticWeaponSlot).Stat = overrideExisting ? stat : (slot as AquaticWeaponSlot).Stat ?? stat;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Stat = overrideExisting ? stat : entry.Stat ?? stat;
                    (slot as WeaponSlot).Stat = overrideExisting ? stat : (slot as WeaponSlot).Stat ?? stat;
                }
            }
        }

        private void SetGroupSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Sigil1 = overrideExisting ? sigil : entry.Sigil1 ?? sigil;
                    entry.Sigil2 = overrideExisting ? sigil : entry.Sigil2 ?? sigil;
                    (slot as AquaticWeaponSlot).Sigil1 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil1 ?? sigil;
                    (slot as AquaticWeaponSlot).Sigil2 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil2 ?? sigil;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Sigil = overrideExisting ? sigil : entry.Sigil ?? sigil;
                    (slot as WeaponSlot).Sigil = overrideExisting ? sigil : (slot as WeaponSlot).Sigil ?? sigil;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Infusion1 = overrideExisting ? infusion : entry.Infusion1 ?? infusion;
                    entry.Infusion2 = overrideExisting ? infusion : entry.Infusion2 ?? infusion;
                    (slot as AquaticWeaponSlot).Infusion1 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion1 ?? infusion;
                    (slot as AquaticWeaponSlot).Infusion2 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion2 ?? infusion;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Infusion = overrideExisting ? infusion : entry.Infusion ?? infusion;
                    (slot as WeaponSlot).Infusion = overrideExisting ? infusion : (slot as WeaponSlot).Infusion ?? infusion;
                }
            }
        }

        private void SetGroupPvpSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {

                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.PvpSigil = overrideExisting ? sigil : entry.PvpSigil ?? sigil;
                    (slot as WeaponSlot).PvpSigil = overrideExisting ? sigil : (slot as WeaponSlot).PvpSigil ?? sigil;
                }
            }
        }

        private void SetGroupWeapon(Weapon item = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    if (overrideExisting || (slot as AquaticWeaponSlot).Item == null)
                        (slot as AquaticWeaponSlot).SelectWeapon(item);
                }
                else
                {
                    if (overrideExisting || ((slot as WeaponSlot).Item == null && (slot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand || (slot as WeaponSlot).OtherHandSlot.Item == null)))
                        (slot as WeaponSlot).SelectWeapon(item);
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (ItemControl.LocalBounds.Size.Y - 4) / 2;
            int iconPadding = Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand ? 7 : 0;
            int textPadding = Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand ? 8 : 5;

            int pvpUpgradeSize = 48;
            int size = Math.Min(Width, Height);
            int padding = 2;
            _changeWeaponTexture.Bounds = new(new(ItemControl.LocalBounds.Left + padding, padding), new((int)((size - (padding * 2)) / 2.5)));

            _sigilControl.SetBounds(new(ItemControl.Right + padding, 0, upgradeSize, upgradeSize));
            _infusionControl.SetBounds(new(ItemControl.Right + padding, ItemControl.Bottom - upgradeSize, upgradeSize, upgradeSize));

            _pvpSigilControl.SetBounds(new(ItemControl.LocalBounds.Right + 2 + 5 + iconPadding, (ItemControl.LocalBounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize));

            _pvpSigilBounds = new(_pvpSigilControl.Right + 10, _pvpSigilControl.Top, Width - (_pvpSigilControl.Right + 2), _pvpSigilControl.Height);

            int x = _sigilControl.Right + textPadding + 4;
            _sigilBounds = new(x, _sigilControl.Top - 1, Width - x, _sigilControl.Height);
            _infusionBounds = new(x, _infusionControl.Top, Width - x, _infusionControl.Height);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (TemplatePresenter.IsPve != false)
            {
                _changeWeaponTexture.Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil?.DisplayText ?? string.Empty), UpgradeFont, _sigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
            else if (TemplatePresenter.IsPvp)
            {
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(PvpSigil?.DisplayText ?? string.Empty), UpgradeFont, _pvpSigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var weapon = TemplatePresenter?.Template?[Slot] as WeaponTemplateEntry;

            Infusion = weapon?.Infusion;
            Sigil = weapon?.Sigil;
            PvpSigil = weapon?.PvpSigil;
            Stat = weapon?.Stat;
            Item = weapon?.Weapon;

            AdjustForOtherSlot();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver && TemplatePresenter.IsPve)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.StatChoices ?? BuildsManager.Data.Weapons.Values.FirstOrDefault()?.StatChoices,
                (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_pvpSigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_pvpSigilControl, new Rectangle(a.Location, Point.Zero).Add(_pvpSigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).PvpSigil = sigil;
                    PvpSigil = sigil;
                });
            }

            if (_sigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigilControl, new Rectangle(a.Location, Point.Zero).Add(_sigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Sigil = sigil;
                    Sigil = sigil;
                });
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusionControl, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }

            if (_changeWeaponTexture.Hovered || (ItemControl.MouseOver && TemplatePresenter.IsPvp))
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, SelectWeapon);
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion}"), () =>
            {
                Stat = null;
                Sigil = null;
                PvpSigil = null;
                Infusion = null;
                Item = null;
            }, new()
            {
                new(() => strings.Weapon, () => string.Format(strings.ResetEntry, strings.Weapon), () => Item = null ),
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => Stat = null ),
                new(() => strings.Sigil, () =>  string.Format(strings.ResetEntry, strings.Sigil), () => Sigil = null ),
                new(() => strings.PvpSigil, () => string.Format(strings.ResetEntry, strings.PvpSigil), () => PvpSigil = null ),
                new(() => strings.Infusion, () => string.Format(strings.ResetEntry, strings.Infusion), () => Infusion = null ),
                });

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion} {strings.EmptyWeaponSlots}"), () =>
                 {
                     SetGroupWeapon(Item as Weapon, false);
                     SetGroupStat(Stat, false);
                     SetGroupSigil(Sigil, false);
                     SetGroupPvpSigil(PvpSigil, false);
                     SetGroupInfusion(Infusion, false);
                 }, new()
                 {
                new(() => strings.Weapon, () => string.Format(strings.FillEntry, $"{strings.Weapon} {strings.EmptyWeaponSlots}"), () => SetGroupWeapon(Item as Weapon, false)),
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyWeaponSlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigil} {strings.EmptyWeaponSlots}"), () => SetGroupSigil(Sigil, false)),
                new(() => strings.PvpSigil, () => string.Format(strings.FillEntry, $"{strings.PvpSigil} {strings.EmptyWeaponSlots}"), () => SetGroupPvpSigil(PvpSigil, false)),
                new(() => strings.Infusion, () => string.Format(strings.FillEntry, $"{strings.Infusion} {strings.EmptyWeaponSlots}"), () => SetGroupInfusion(Infusion, false)),
                     });

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil, true);
                SetGroupPvpSigil(PvpSigil, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => strings.Weapon, () => string.Format(strings.OverrideEntry, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(Item as Weapon, true)),
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.WeaponSlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Sigil, () => string.Format(strings.OverrideEntry, $"{strings.Sigil} {strings.WeaponSlots}"), () => SetGroupSigil(Sigil, true)),
                new(() => strings.PvpSigil, () => string.Format(strings.OverrideEntry, $"{strings.PvpSigil} {strings.WeaponSlots}"), () => SetGroupPvpSigil(PvpSigil, true)),
                new(() => strings.Infusion, () => string.Format(strings.OverrideEntry, $"{strings.Infusion} {strings.WeaponSlots}"), () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Weapons), () => string.Format(strings.ResetEntry, $"{strings.Weapons}, {strings.Stats} , {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupPvpSigil(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => strings.Weapons, () => string.Format(strings.ResetAll, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(null, true)),
                new(() => strings.Stats, () => string.Format(strings.ResetAll, $"{strings.Stats} {strings.WeaponSlots}"), () => SetGroupStat(null, true)),
                new(() => strings.Sigils, () => string.Format(strings.ResetAll, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(null, true)),
                new(() => strings.PvpSigils, () => string.Format(strings.ResetAll, $"{strings.PvpSigils} {strings.WeaponSlots}"), () => SetGroupPvpSigil(null, true)),
                new(() => strings.Infusions, () => string.Format(strings.ResetAll, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(null, true) ),
                });
        }

        public void SelectWeapon(Weapon item)
        {
            if (item == null)
            {
                (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon = item;
                Item = item;
                return;
            }

            if (item.WeaponType is ItemWeaponType.Trident or ItemWeaponType.Speargun or ItemWeaponType.Harpoon)
                return;

            if (item.WeaponType.IsTwoHanded() && Slot is not TemplateSlotType.MainHand and not TemplateSlotType.AltMainHand)
                return;

            var template = TemplatePresenter.Template;
            var otherHand =
                Slot is TemplateSlotType.MainHand ? TemplateSlotType.OffHand :
                Slot is TemplateSlotType.AltMainHand ? TemplateSlotType.AltOffHand :
                Slot is TemplateSlotType.OffHand ? TemplateSlotType.MainHand :
                Slot is TemplateSlotType.AltOffHand ? TemplateSlotType.AltMainHand :
                TemplateSlotType.None;

            if (item.WeaponType.IsTwoHanded() || (Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand && (OtherHandSlot?.Item as Weapon)?.WeaponType.IsTwoHanded() == true))
            {
                if (template[otherHand] is WeaponTemplateEntry offHand)
                {
                    offHand.Weapon = null;
                    if (OtherHandSlot is not null) OtherHandSlot.Item = null;
                }
            }

            (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon = item;
            Item = item;
            ItemControl.Item = item;

            ItemControl.Opacity = 1F;
            WeaponChanged?.Invoke(this, item);
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            if (OtherHandSlot is not null && OtherHandSlot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand && (OtherHandSlot.Item as Weapon)?.WeaponType.IsTwoHanded() == true && OtherHandSlot.Stat != Stat)
            {
                Stat = OtherHandSlot.Stat;
                return;
            }

            ItemControl.Stat = Stat;
            StatChanged?.Invoke(this, Stat);
        }

        private void OnSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigilControl.Item = Sigil;
        }

        private void OnPvpSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _pvpSigilControl.Item = PvpSigil;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionControl.Item = Infusion;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Sigil = null;
            PvpSigil = null;
            Infusion = null;
            OtherHandSlot = null;
        }
    }
}
