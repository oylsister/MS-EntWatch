using Sharp.Shared.Objects;

namespace MS_EntWatch.Helpers
{
    public static class TargetManager
    {
        public static List<IGameClient> Find(IGameClient invoker, string? selector)
        {
            selector = (selector ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(selector)) return [];

            if (EntWatch._targetingManager is { } tm)
            {
                IEnumerable<IGameClient> enumlist = tm.GetByTarget(invoker, selector);
                if (enumlist.Any()) return [.. enumlist];
            }

            return FindSmart(selector);
        }

        private static List<IGameClient> All()
        {
            return [.. EntWatch._clients!.GetGameClients(true).ToList()];
        }

        private static List<IGameClient> FindSmart(string selector)
        {
            if (selector[0] == '#')
            {
                var raw = selector[1..];
                if (string.IsNullOrEmpty(raw)) return [];

                //UserID
                if (int.TryParse(raw, out var UserID))
                {
                    foreach (var client in All())
                    {
                        if (client.UserId == UserID)
                        {
                            return [client];
                        }
                    }
                }

                //SteamID
                if (raw.StartsWith("steam", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var client in All())
                    {
                        if (raw.Equals(EW.ConvertSteamID64ToSteamID(client.SteamId.ToString()), StringComparison.OrdinalIgnoreCase))
                        {
                            return [client];
                        }
                    }
                }
            }

            // name contains
            return [.. All().Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(selector, StringComparison.OrdinalIgnoreCase))];
        }
    }
}
