namespace InventoryControl_CursedMod
{
    using CursedMod.Events.Arguments.Player;
    using CursedMod.Features.Logger;
    using CursedMod.Features.Wrappers.Inventory.Items;
    using CursedMod.Features.Wrappers.Inventory.Items.Firearms;
    using CursedMod.Features.Wrappers.Player;
    using CursedMod.Features.Wrappers.Round;
    using InventorySystem.Items.Firearms;
    using InventorySystem.Items.Firearms.Attachments;
    using MEC;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Random = UnityEngine.Random;

    public class EventHandlers
    {
        public void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            try
            {
                if (ev.Player == null || !CursedRound.HasStarted) return;

                if (Plugin.Instance.Config.InventoryRank?.Count > 0)
                    if (Plugin.Instance.Config.InventoryRank.ContainsKey(GetPlayerGroupName(ev.Player))) { SetRankRoleItem(ev.Player, ev.NewRole, GetPlayerGroupName(ev.Player)); return; }

                if (Plugin.Instance.Config.Inventory?.Count > 0)
                    if (Plugin.Instance.Config.Inventory.ContainsKey(ev.NewRole)) SetRoleItem(ev.Player, ev.NewRole);
            }
            catch (Exception e)
            {
                CursedLogger.LogError("[InventoryControl] [Event: OnPlayerChangingRole] " + e.ToString());
            }
        }

        private void SetRoleItem(CursedPlayer player, RoleTypeId newRole)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    if (!Plugin.Instance.Config.Inventory.TryGetValue(newRole, out InventoryItem inventoryItem)) return;

                    Dictionary<ItemType, ushort> Ammos = new Dictionary<ItemType, ushort>();

                    foreach (KeyValuePair<ItemType, ushort> item in player.ReferenceHub.inventory.UserInventory.ReserveAmmo)
                        Ammos.Add(item.Key, item.Value);

                    for (int ammo = 0; ammo < player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Count; ammo++)
                        player.SetAmmo(player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ElementAt(ammo).Key, 0);

                    if (!inventoryItem.keepItems)
                        player.ClearInventory(false);

                    foreach (KeyValuePair<ItemType, int> Item in inventoryItem.Items)
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
                    CursedLogger.LogError("[InventoryControl] [Event: SetRoleItem] " + e.ToString());
                }
            });
        }

        private void SetRankRoleItem(CursedPlayer player, RoleTypeId newRole, string groupName)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    if (Plugin.Instance.Config.InventoryRank.TryGetValue(groupName, out Dictionary<RoleTypeId, InventoryItem> rankItems))
                    {
                        if (!rankItems.TryGetValue(newRole, out InventoryItem inventoryItem)) return;

                        Dictionary<ItemType, ushort> Ammos = new Dictionary<ItemType, ushort>();

                        foreach (KeyValuePair<ItemType, ushort> item in player.Ammo)
                            Ammos.Add(item.Key, item.Value);

                        if (!inventoryItem.keepItems)
                            player.ClearInventory(false);

                        foreach (KeyValuePair<ItemType, int> Item in inventoryItem.Items)
                            if (Item.Value >= Random.Range(0, 101))
                            {
                                CursedItem itemBase = player.AddItem(Item.Key);

                                if (itemBase is CursedFirearmItem firearm) firearm.SetPlayerAttachments(player);
                            }

                        for (int ammo = 0; ammo < Ammos.Count; ammo++)
                            if (Ammos.ElementAt(ammo).Value > 0)
                                player.SetAmmo(Ammos.ElementAt(ammo).Key, Ammos.ElementAt(ammo).Value);
                    }
                }
                catch (Exception e)
                {
                    CursedLogger.LogError("[InventoryControl] [Event: SetRankRoleItem] " + e.ToString());
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
                CursedLogger.LogError("[InventoryControl] [Event: GetPlayerGroupName] " + e.ToString());
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
