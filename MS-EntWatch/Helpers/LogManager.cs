using Serilog;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MS_EntWatch.Helpers
{
    class LogCfg
    {
        public string Type { get; set; }        //File or Discord
        public string Send { get; set; }    //FileName or WebHook
        public string Lang { get; set; }       //Language to translate

        public bool ItemInfo { get; set; }
        public bool AdminInfo { get; set; }
        public bool SystemInfo { get; set; }
        public bool CvarInfo { get; set; }

        public Serilog.Core.Logger? LWritter;

        public LogCfg()
        {
            Type = "File";
            Send = "EntWatch";
            Lang = "en";
            ItemInfo = true;
            AdminInfo = true;
            SystemInfo = true;
            CvarInfo = true;
            LWritter = null;
        }
    }
    public static partial class LogManager
    {
        static readonly List<LogCfg> LM_CFG = [];
        static readonly HttpClient _httpClient = new();
        static string ReplaceInvalid(string str) => _InvalidRegex().Replace(str, "");

        public static void LoadConfig()
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                cfg.LWritter?.Dispose();
                cfg.LWritter = null;
            }
            LM_CFG.Clear();
            string sConfig = $"{Path.Join(EntWatch._dllPath!, "log_config.json")}";
            string sData;
            if (File.Exists(sConfig))
            {
                sData = File.ReadAllText(sConfig);
                List<LogCfg>? CFGBuffer = JsonSerializer.Deserialize<List<LogCfg>>(sData);
                if (CFGBuffer == null) return;
                foreach (LogCfg cfg in CFGBuffer.ToList())
                {
                    ValidateCFG(cfg);
                }
            }
        }

        public static void UnInit()
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                cfg.LWritter?.Dispose();
                cfg.LWritter = null;
            }
        }

        static void ValidateCFG(LogCfg CfgTest)
        {
            if (!(CfgTest.ItemInfo || CfgTest.AdminInfo || CfgTest.SystemInfo || CfgTest.CvarInfo)) return;

            if (string.IsNullOrEmpty(CfgTest.Type) || string.IsNullOrEmpty(CfgTest.Send) || string.IsNullOrEmpty(CfgTest.Lang)) return;

            byte iSend = 0;
            if (string.Equals(CfgTest.Type.ToLower(), "file")) { iSend = 1; }
            if (string.Equals(CfgTest.Type.ToLower(), "discord")) { iSend = 2; }

            if (iSend == 1) CfgTest.Send = ReplaceInvalid(CfgTest.Send);
            else if (iSend == 2) { if (!CfgTest.Send.StartsWith("https://discord.com/api/webhooks/")) return; }
            else return;

            if (string.IsNullOrEmpty(CfgTest.Send)) return;

            bool bNotFound = true;

            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                if (string.Equals(CfgTest.Type.ToLower(), cfg.Type.ToLower()) && string.Equals(CfgTest.Send, cfg.Send))
                {
                    if (iSend == 2 && CultureInfo.GetCultureInfo(CfgTest.Lang) != CultureInfo.GetCultureInfo(cfg.Lang)) continue;
                    if (CfgTest.ItemInfo) cfg.ItemInfo = true;
                    if (CfgTest.AdminInfo) cfg.AdminInfo = true;
                    if (CfgTest.SystemInfo) cfg.SystemInfo = true;
                    if (CfgTest.CvarInfo) cfg.CvarInfo = true;
                    bNotFound = false;
                    break;
                }
            }
            if (bNotFound)
            {
                LM_CFG.Add(CfgTest);
                if (iSend == 1)
                {
                    CfgTest.LWritter = new LoggerConfiguration()
                        .WriteTo.File($"{EntWatch._sharpPath!}/logs/EntWatch/{CfgTest.Send}-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff(zzz)} [{Level:w3}] {Message:l}{NewLine}{Exception}")
                        .CreateLogger();
                }
            }
        }

        static async Task SendToDistord(string sWebHook, string sMessage, bool bMapName = true)
        {
            try
            {
                string sMapname = "<none>";
                if (bMapName && EntWatch._modSharp!.GetMapName() is { } mapname) sMapname = mapname;
                var body = JsonSerializer.Serialize(new { content = $"*{sMapname} - {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}* ```{sMessage}```" });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage res = (await _httpClient.PostAsync($"{sWebHook}", content)).EnsureSuccessStatusCode();
            }
            catch (Exception) { }
        }

        public static void ItemAction(string sMessage, params object[] arg)
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                if (cfg.ItemInfo)
                {
                    string sMsg = UI.LocalizerFormat(CultureInfo.GetCultureInfo(cfg.Lang), sMessage, arg);
                    if (string.Equals(cfg.Type.ToLower(), "file") && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
                    else if (string.Equals(cfg.Type.ToLower(), "discord"))
                    {
                        Task.Run(async () =>
                        {
                            await SendToDistord(cfg.Send, sMsg);
                        });
                    }
                }
            }
        }

        public static void AdminAction(string sMessage, params object[] arg)
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                if (cfg.AdminInfo)
                {
                    string sMsg = UI.LocalizerFormat(CultureInfo.GetCultureInfo(cfg.Lang), sMessage, arg);
                    if (string.Equals(cfg.Type.ToLower(), "file") && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
                    else if (string.Equals(cfg.Type.ToLower(), "discord"))
                    {
                        Task.Run(async () =>
                        {
                            await SendToDistord(cfg.Send, sMsg);
                        });
                    }
                }
            }
        }

        public static void SystemAction(string sMessage, bool bMapName, params object[] arg)
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                if (cfg.SystemInfo)
                {
                    string sMsg = UI.LocalizerFormat(CultureInfo.GetCultureInfo(cfg.Lang), sMessage, arg);
                    if (string.Equals(cfg.Type.ToLower(), "file") && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
                    else if (string.Equals(cfg.Type.ToLower(), "discord"))
                    {
                        Task.Run(async () =>
                        {
                            await SendToDistord(cfg.Send, sMsg, bMapName);
                        });
                    }
                }
            }
        }

        public static void CvarAction(string sCvarName, string sCvarValue)
        {
            foreach (LogCfg cfg in LM_CFG.ToList())
            {
                if (cfg.CvarInfo)
                {
                    string sMsg = UI.LocalizerFormat(CultureInfo.GetCultureInfo(cfg.Lang), "EntWatch.Cvar.Notify", sCvarName, sCvarValue);
                    if (string.Equals(cfg.Type.ToLower(), "file") && cfg.LWritter != null) cfg.LWritter.Information(sMsg);
                    else if (string.Equals(cfg.Type.ToLower(), "discord"))
                    {
                        Task.Run(async () =>
                        {
                            await SendToDistord(cfg.Send, sMsg);
                        });
                    }
                }
            }
        }

        [GeneratedRegex(@"[^\w\(\s!@\#\$%\^&\*\(\)_\+=\-'\\:\|/`~\.,\{}\)]+")]
        private static partial Regex _InvalidRegex();
    }
}
