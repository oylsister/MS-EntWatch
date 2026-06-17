using MS_EntWatch.Helpers;
using MS_EntWatch.Items;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace MS_EntWatch.Modules
{
    static class SpawnItem
    {
        public static (ItemConfig?, int) Spawn(IGameClient client, IGameClient receiver, string sItemName, bool bStrip, bool bChat, bool bNotice)
        {
            if (EW.g_Scheme == null || EW.g_ItemConfig == null) return (null, 0);
            if (receiver.GetPlayerController() is not { } receiverplayer || receiverplayer.GetPlayerPawn() is not { } receiverpawn)
            {
                if (bNotice) UI.ReplyToCommand(client, "EntWatch.Reply.No_matching_client", bChat, EW.g_Scheme.Color_warning);
                return (null, 1);
            }

            int iCount = 0;
            ItemConfig Item = new();
            foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
            {
                if ((ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(ItemTest.SpawnerID) && !string.Equals(ItemTest.SpawnerID, "0"))
                {
                    iCount++;
                    Item = ItemTest;
                }
            }
            if (iCount < 1)
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.NoItem", bChat, EW.g_Scheme.Color_warning);
                return (null, 0);
            }
            if (iCount > 1)
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.ManyItems", bChat, EW.g_Scheme.Color_warning);
                foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
                {
                    if ((ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(ItemTest.SpawnerID) && !string.Equals(ItemTest.SpawnerID, "0"))
                    {
                        UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.ItemName", bChat, EW.g_Scheme.Color_warning, ItemTest.Color, ItemTest.Name, ItemTest.ShortName);
                    }
                }
                return (null, 0);
            }
            if (string.IsNullOrEmpty(Item.SpawnerID) || string.Equals(Item.SpawnerID, "0"))
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.NoCfgSpawner", bChat, EW.g_Scheme.Color_warning);
                return (null, 0);
            }

            IBaseEntity? entPT = null;
            foreach (var entity in EntWatch._entities!.GetAllEntitiesByClassname("point_template"))
            {
                if (entity is { IsValidEntity:true } && string.Equals(entity.HammerId, Item.SpawnerID))
                {
                    entPT = entity;
                    break;
                }
            }

            if (entPT is { IsValidEntity:true } pt)
            {
                if (bStrip) receiverpawn.RemoveAllItems();

                Vector vecReceiver = receiverpawn.GetCenter();

                foreach (var cl in EntWatch._clients!.GetGameClients(true).ToArray())
                {
                    if (EW.CheckDictionary(cl) && cl.GetPlayerController() is { } pl && pl.GetPlayerPawn() is { } plpawn && EW.Distance(vecReceiver, plpawn.GetCenter()) <= 64.0) EW.g_EWPlayer[cl].BannedPlayer.bFixSpawnItem = true;
                }

                var kv = new Dictionary<string, KeyValuesVariantValueItem>
                {
                    {"EntityTemplate", entPT.Name},
                    {"spawnflags", 0}
                };
                if (EntWatch._entities!.SpawnEntitySync<IWorldText>("env_entity_maker", kv) is { } maker)
                {
                    maker.DispatchSpawn();
                    maker.SetAbsOrigin(vecReceiver);
                    maker.AcceptInput("ForceSpawn");
                    maker.Kill();
                }

                EntWatch._modSharp!.PushTimer(() =>
                {
                    foreach (var cl in EntWatch._clients!.GetGameClients(true).ToArray())
                    {
                        if (EW.CheckDictionary(cl)) EW.g_EWPlayer[cl].BannedPlayer.bFixSpawnItem = false;
                    }
                }, 0.2f);

                if (bNotice) UI.EWChatAdminSpawn(UI.PlayerInfoFormat(client), UI.PlayerInfoFormat(receiver), $"{Item.Color}{Item.Name}({Item.ShortName}){EW.g_Scheme.Color_warning}");
                EW.g_cAPI.OnAdminSpawnItem(client, Item.Name, receiver);
                return (Item, 2);
            }
            else
            {
                UI.ReplyToCommand(client, "EntWatch.Reply.Spawn.NoSpawner", bChat, EW.g_Scheme.Color_warning);
                return (null, 0);
            }
        }
    }
}
