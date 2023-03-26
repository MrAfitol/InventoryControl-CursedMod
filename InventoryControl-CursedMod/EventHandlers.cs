using CursedMod.Events.Arguments.Player;
using CursedMod.Features.Wrappers.Inventory.Items;
using CursedMod.Features.Wrappers.Inventory.Items.Firearms;
using CursedMod.Features.Wrappers.Player;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace InventoryControl_CursedMod
{
    public class EventHandlers
    {
        public void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            try
            {
                if (ev.Player == null || !Round.IsRoundStarted) return;

                if (Plugin.Instance.Config.InventoryRank?.Count > 0)
                    if (Plugin.Instance.Config.InventoryRank.ContainsKey(GetPlayerGroupName(ev.Player))) { SetRankRoleItem(ev.Player, ev.NewRole, GetPlayerGroupName(ev.Player)); return; }

                if (Plugin.Instance.Config.Inventory?.Count > 0)
                    if (Plugin.Instance.Config.Inventory.ContainsKey(ev.NewRole)) SetRoleItem(ev.Player, ev.NewRole);
            }
            catch (Exception e)
            {
                Log.Error("[InventoryControl] [Event: OnPlayerChangingRole] " + e.ToString());
            }
        }

        private void SetRoleItem(CursedPlayer player, RoleTypeId newRole)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    Dictionary<ItemType, ushort> Ammos = new Dictionary<ItemType, ushort>();

                    foreach (KeyValuePair<ItemType, ushort> item in player.ReferenceHub.inventory.UserInventory.ReserveAmmo)
                        Ammos.Add(item.Key, item.Value);

                    for (int ammo = 0; ammo < player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Count; ammo++)
                        player.SetAmmo(player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ElementAt(ammo).Key, 0);

                    KeyValuePair<RoleTypeId, InventoryItem> RoleInventory = Plugin.Instance.Config.Inventory.First(x => x.Key == newRole);

                    if (!RoleInventory.Value.keepItems)
                        player.ClearInventory(false);

                    foreach (KeyValuePair<ItemType, int> Item in RoleInventory.Value.Items)
                        if (Item.Value >= Random.Range(0, 101))
                        {
                            CursedItem itemBase = player.AddItem(Item.Key);

                            if (itemBase is CursedFirearmItem firearm)
                            {
                                if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out var value) && value.TryGetValue(itemBase.ItemType, out var value2))
                                    firearm.FirearmBase.ApplyAttachmentsCode(value2, reValidate: true);

                                FirearmStatusFlags firearmStatusFlags = FirearmStatusFlags.MagazineInserted;
                                if (firearm.FirearmBase.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
                                    firearmStatusFlags |= FirearmStatusFlags.FlashlightEnabled;

                                firearm.Status = new FirearmStatus(firearm.AmmoManagerModule.MaxAmmo, firearmStatusFlags, firearm.FirearmBase.GetCurrentAttachmentsCode());
                            }
                        }

                    for (int ammo = 0; ammo < Ammos.Count; ammo++)
                        if (Ammos.ElementAt(ammo).Value > 0)
                            player.SetAmmo(Ammos.ElementAt(ammo).Key, Ammos.ElementAt(ammo).Value);
                }
                catch (Exception e)
                {
                    Log.Error("[InventoryControl] [Event: SetRoleItem] " + e.ToString());
                }
            });
        }

        private void SetRankRoleItem(CursedPlayer player, RoleTypeId newRole, string groupName)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    if (Plugin.Instance.Config.InventoryRank.ContainsKey(ServerStatic.PermissionsHandler._members[player.UserId]))
                    {
                        if (!Plugin.Instance.Config.InventoryRank[groupName].ContainsKey(player.Role)) return;

                        Dictionary<ItemType, ushort> Ammos = new Dictionary<ItemType, ushort>();

                        foreach (KeyValuePair<ItemType, ushort> item in player.ReferenceHub.inventory.UserInventory.ReserveAmmo)
                            Ammos.Add(item.Key, item.Value);

                        for (int ammo = 0; ammo < player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Count; ammo++)
                            player.SetAmmo(player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ElementAt(ammo).Key, 0);

                        KeyValuePair<RoleTypeId, InventoryItem> RoleInventory = Plugin.Instance.Config.InventoryRank[ServerStatic.PermissionsHandler._members[player.UserId]].First(x => x.Key == newRole);

                        if (!RoleInventory.Value.keepItems)
                            player.ClearInventory(false);

                        foreach (KeyValuePair<ItemType, int> Item in RoleInventory.Value.Items)
                            if (Item.Value >= Random.Range(0, 101))
                            {
                                CursedItem itemBase = player.AddItem(Item.Key);

                                if (itemBase is CursedFirearmItem firearm)
                                {
                                    if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out var value) && value.TryGetValue(itemBase.ItemType, out var value2))
                                        firearm.FirearmBase.ApplyAttachmentsCode(value2, reValidate: true);

                                    FirearmStatusFlags firearmStatusFlags = FirearmStatusFlags.MagazineInserted;
                                    if (firearm.FirearmBase.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
                                        firearmStatusFlags |= FirearmStatusFlags.FlashlightEnabled;

                                    firearm.Status = new FirearmStatus(firearm.AmmoManagerModule.MaxAmmo, firearmStatusFlags, firearm.FirearmBase.GetCurrentAttachmentsCode());
                                }
                            }

                        for (int ammo = 0; ammo < Ammos.Count; ammo++)
                            if (Ammos.ElementAt(ammo).Value > 0)
                                player.SetAmmo(Ammos.ElementAt(ammo).Key, Ammos.ElementAt(ammo).Value);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[InventoryControl] [Event: SetRankRoleItem] " + e.ToString());
                }
            });
        }

        private string GetPlayerGroupName(CursedPlayer player)
        {
            try
            {
                if (player.UserId == null) return string.Empty;

                if (ServerStatic.PermissionsHandler._members.ContainsKey(player.UserId))
                {
                    return ServerStatic.PermissionsHandler._members[player.UserId];
                }
                else
                {
                    return player.ReferenceHub.serverRoles.Group != null ? ServerStatic.GetPermissionsHandler()._groups.First(g => EqualsTo(g.Value, player.ReferenceHub.serverRoles.Group)).Key : string.Empty;
                }
            }
            catch (Exception e)
            {
                Log.Error("[InventoryControl] [Event: GetPlayerGroupName] " + e.ToString());
                return string.Empty;
            }
        }

        private bool EqualsTo(UserGroup check, UserGroup player)
            => check.BadgeColor == player.BadgeColor
               && check.BadgeText == player.BadgeText
               && check.Permissions == player.Permissions
               && check.Cover == player.Cover
               && check.HiddenByDefault == player.HiddenByDefault
               && check.Shared == player.Shared
               && check.KickPower == player.KickPower
               && check.RequiredKickPower == player.RequiredKickPower;
    }
}
