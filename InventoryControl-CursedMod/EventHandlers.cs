namespace InventoryControl_CursedMod
{
    using CursedMod.Events.Arguments.Player;
    using CursedMod.Features.Logger;
    using CursedMod.Features.Wrappers.Inventory.Items;
    using CursedMod.Features.Wrappers.Inventory.Items.Firearms;
    using CursedMod.Features.Wrappers.Player;
    using CursedMod.Features.Wrappers.Round;
    using MEC;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Random = UnityEngine.Random;
    using InventorySystem;

    public class EventHandlers
    {
        public void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            try
            {
                if (ev.Player == null || !CursedRound.HasStarted) return;

                if (Plugin.Instance.Config.InventoryRank?.Count > 0)
                    if (Plugin.Instance.Config.InventoryRank.ContainsKey(GetPlayerGroupName(ev.Player)))
                        if (Plugin.Instance.Config.InventoryRank[GetPlayerGroupName(ev.Player)].TryGetValue(ev.NewRole, out InventoryItem inventoryRankItem))
                        {
                            SetRoleItem(ev.Player, inventoryRankItem);
                            return;
                        }

                if (Plugin.Instance.Config.Inventory?.Count > 0)
                    if (Plugin.Instance.Config.Inventory.TryGetValue(ev.NewRole, out InventoryItem inventoryItem)) SetRoleItem(ev.Player, inventoryItem);
            }
            catch (Exception e)
            {
                CursedLogger.LogError("[InventoryControl] [Event: OnPlayerChangingRole] " + e.ToString());
            }
        }

        private void SetRoleItem(CursedPlayer player, InventoryItem inventoryItem)
        {
            Timing.CallDelayed(0.3f, () =>
            {
                try
                {
                    Dictionary<ItemType, ushort> Ammos = new Dictionary<ItemType, ushort>();

                    foreach (KeyValuePair<ItemType, ushort> item in player.Ammo)
                        Ammos.Add(item.Key, item.Value);

                    for (int ammo = 0; ammo < player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Count; ammo++)
                        player.SetAmmo(player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ElementAt(ammo).Key, 0);

                    int itemCount = player.ReferenceHub.inventory.UserInventory.Items.Count;

                    if (!inventoryItem.keepItems)
                    {
                        var inventory = player.ReferenceHub.inventory.UserInventory;
                        while (inventory.Items.Count > 0)
                        {
                            player.ReferenceHub.inventory.ServerRemoveItem(inventory.Items.ElementAt(0).Key, null);
                        }
                    }

                    foreach (KeyValuePair<ItemType, int> Item in inventoryItem.Items)
                        if (Item.Value >= Random.Range(0, 101))
                        {
                            if (player.ReferenceHub.inventory.UserInventory.Items.Count >= 8) return;

                            CursedItem itemBase = player.AddItem(Item.Key);

                            if (itemBase is CursedFirearmItem firearm) firearm.SetPlayerAttachments(player);
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
