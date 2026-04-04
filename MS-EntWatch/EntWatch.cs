using Microsoft.Extensions.Configuration;
using MS_EntWatch.Helpers;
using MS_EntWatch.Modules.Eban;
using MS_EntWatch_Shared;
using MS_GameHUD_Shared;
using Sharp.Modules.AdminManager.Shared;
using Sharp.Modules.ClientPreferences.Shared;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Modules.TargetingManager.Shared;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.HookParams;
using Sharp.Shared.Hooks;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using System.Runtime.CompilerServices;

[assembly: DisableRuntimeMarshalling]

namespace MS_EntWatch
{
    public partial class EntWatch: IModSharpModule, IGameListener, IEntityListener, IClientListener
    {
        public string DisplayName => "EntWatch";
        public string DisplayAuthor => "DarkerZ[RUS]";
#pragma warning disable IDE0060
        public EntWatch(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version version, IConfiguration coreConfiguration, bool hotReload)
#pragma warning restore IDE0060
        {
            _modSharp = sharedSystem.GetModSharp();
            _modules = sharedSystem.GetSharpModuleManager();
            _convars = sharedSystem.GetConVarManager();
            _clients = sharedSystem.GetClientManager();
            _hooks = sharedSystem.GetHookManager();
            _entities = sharedSystem.GetEntityManager();
            _transmits = sharedSystem.GetTransmitManager();
            _events = sharedSystem.GetEventManager();
            _physicsQuery = sharedSystem.GetPhysicsQueryManager();
            _dllPath = dllPath;
            _sharpPath = sharpPath;
            _virtualHook = _hooks.CreateVirtualHook();
        }
#pragma warning disable CA2211
        public static IModSharp? _modSharp;
        private static ISharpModuleManager? _modules;
        private readonly IConVarManager _convars;
        public static IClientManager? _clients;
        private readonly IHookManager _hooks;
        public static IEntityManager? _entities;
        public static ITransmitManager? _transmits;
        public static IEventManager? _events;
        public static IPhysicsQueryManager? _physicsQuery;
        public static string? _dllPath;
        public static string? _sharpPath;
        private IDisposable? _callback;
        private readonly IVirtualHook _virtualHook;
        public static ITargetingManager? _targetingManager;
#pragma warning restore CA2211

        private static IModSharpModuleInterface<ILocalizerManager>? _localizer;
        private IModSharpModuleInterface<IClientPreference>? _icp;
        private static IModSharpModuleInterface<IGameHUDAPI>? _igamehud;
        private static IModSharpModuleInterface<IAdminManager>? _adminManager;
        private static bool _AMInit = false;

        public bool Init()
        {
            RegisterCvars();
            if (!Hook_TriggersTouch()) return false;
            _modSharp!.InstallGameListener(this);
            _entities!.InstallEntityListener(this);
            _clients!.InstallClientListener(this);
            _hooks.TerminateRound.InstallHookPost(OnTerminateRoundPost);
            _hooks.PlayerRunCommand.InstallHookPost(OnPlayerRunCommandPost);
            _hooks.PlayerDropWeapon.InstallForward(OnPlayerDropWeapon);
            _hooks.PlayerEquipWeapon.InstallForward(OnPlayerEquipWeapon);
            _hooks.PlayerWeaponCanUse.InstallHookPre(OnPlayerWeaponCanUse);
            _hooks.PlayerKilledPre.InstallForward(OnPlayerKilledPre);
            RegCommands();
            RegMapCommands();
            return true;
        }

        public void PostInit()
        {
            _modules!.RegisterSharpModuleInterface<IEntWatchAPI>(this, IEntWatchAPI.Identity, EW.g_cAPI);
            _entities!.HookEntityOutput("func_button", "OnPressed");
            _entities!.HookEntityOutput("func_rot_button", "OnPressed");
            _entities!.HookEntityOutput("func_door", "OnOpen");
            _entities!.HookEntityOutput("func_door_rotating", "OnOpen");
            _entities!.HookEntityOutput("func_physbox", "OnPlayerUse");
            _entities!.HookEntityInput("logic_case", "InValue");
            _modSharp!.PushTimer(OnEntityTransmit, 5.0, GameTimerFlags.Repeatable);
            TryResolveTargetingManager();
            TryResolveAdminManager();
        }

        public void OnAllModulesLoaded()
        {
            GetClientPrefs();
            GetLocalizer()?.LoadLocaleFile("EntWatch");
            LogManager.LoadConfig();
            GetGameHUD();
            EW.InitTimers();
            EbanDB.Init_DB();
            TryResolveTargetingManager();
            TryResolveAdminManager();
        }

        public void OnLibraryConnected(string name)
        {
            if (name.Equals("ClientPreferences")) GetClientPrefs();
            if (name.Equals("GameHUD")) GetGameHUD();
            if (name.Equals("Sharp.Modules.TargetingManager", StringComparison.OrdinalIgnoreCase)) TryResolveTargetingManager();
            if (name.Equals("Sharp.Modules.AdminManager", StringComparison.OrdinalIgnoreCase)) TryResolveAdminManager();
        }

        public void OnLibraryDisconnect(string name)
        {
            if (name.Equals("ClientPreferences")) _icp = null;
            if (name.Equals("GameHUD")) _igamehud = null;
            if (name.Equals("Sharp.Modules.TargetingManager", StringComparison.OrdinalIgnoreCase))
            {
                _targetingManager = null;
            }
        }

        private void OnCookieLoad(IGameClient client)
        {
            LoadClientPrefs(client);
        }

        public void Shutdown()
        {
            _virtualHook.Uninstall();
            LogManager.UnInit();
            _modSharp!.RemoveGameListener(this);
            _entities!.RemoveEntityListener(this);
            _clients!.RemoveClientListener(this);
            _hooks.TerminateRound.RemoveHookPost(OnTerminateRoundPost);
            _hooks.PlayerRunCommand.RemoveHookPost(OnPlayerRunCommandPost);
            _hooks.PlayerDropWeapon.RemoveForward(OnPlayerDropWeapon);
            _hooks.PlayerEquipWeapon.RemoveForward(OnPlayerEquipWeapon);
            _hooks.PlayerWeaponCanUse.RemoveHookPre(OnPlayerWeaponCanUse);
            _hooks.PlayerKilledPre.RemoveForward(OnPlayerKilledPre);
            UnRegMapCommands();
            UnRegCommands();
            EW.RemoveTimers();
            _callback?.Dispose();
            UnRegisterCvars();
            EbanDB.db?.AnyDB.UnSet();
        }

        public void OnResourcePrecache() //Precache
        {
            OnPrecacheResources();
        }

        public void OnGameActivate() //OnMapStart
        {
            OnMapStart_Listener();
        }

        public void OnGameDeactivate() //OnMapEnd
        {
            OnMapEnd_Listener();
        }

        public void OnRoundRestart() //OnRoundPreStart
        {
            OnEventRoundStart();
        }

        public void OnEntitySpawned(IBaseEntity entity)
        {
            OnEntitySpawned_Listener(entity);
        }

        public void OnEntityDeleted(IBaseEntity entity)
        {
            OnEntityDeleted_Listener(entity);
        }

        private void OnTerminateRoundPost(ITerminateRoundHookParams @params, HookReturnValue<EmptyHookReturn> value) //OnRoundEnd
        {
            OnEventRoundEnd();
        }

        public void OnClientPutInServer(IGameClient client)
        {
            OnEventPlayerConnectFull(client);
        }

        public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
        {
            OnEventPlayerDisconnect(client);
        }

        private void OnPlayerRunCommandPost(IPlayerRunCommandHookParams @params, HookReturnValue<EmptyHookReturn> @return)
        {
            OnEventPlayerPressUse(@params);
        }

        private void OnPlayerEquipWeapon(IPlayerEquipWeaponForwardParams @params)
        {
            OnWeaponPickup(@params);
        }

        private void OnPlayerDropWeapon(IPlayerDropWeaponForwardParams @params)
        {
            OnWeaponDrop(@params);
        }

        private HookReturnValue<bool> OnPlayerWeaponCanUse(IPlayerWeaponCanUseHookParams @params, HookReturnValue<bool> value)
        {
            return OnWeaponCanUse(@params);
        }

        private void OnPlayerKilledPre(IPlayerKilledForwardParams @params)
        {
            OnEventPlayerDeath(@params);
        }

        public EHookAction OnEntityFireOutput(IBaseEntity entity, string output, IBaseEntity? activator, float delay)
        {
            return OnButtonPressed(entity, activator);
        }

        public EHookAction OnEntityAcceptInput(IBaseEntity entity, string input, in EntityVariant value, IBaseEntity? activator, IBaseEntity? caller)
        {
            return OnInput(entity, input, value, activator);
        }

        private void OnEntityTransmit()
        {
            OnTransmit();
        }

        public static ILocalizerManager? GetLocalizer()
        {
            if (_localizer?.Instance is null)
            {
                _localizer = _modules!.GetOptionalSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity);
            }
            return _localizer?.Instance;
        }

        private IClientPreference? GetClientPrefs()
        {
            if (_icp?.Instance is null)
            {
                _icp = _modules!.GetOptionalSharpModuleInterface<IClientPreference>(IClientPreference.Identity);
                if (_icp?.Instance is { } instance) _callback = instance.ListenOnLoad(OnCookieLoad);
            }
            return _icp?.Instance;
        }
        public static IGameHUDAPI? GetGameHUD()
        {
            if (_igamehud?.Instance is null) _igamehud = _modules!.GetOptionalSharpModuleInterface<IGameHUDAPI>(IGameHUDAPI.Identity);
            return _igamehud?.Instance;
        }

        private static void TryResolveTargetingManager()
        {
            if (_targetingManager is not null) return;

            _targetingManager = _modules!.GetOptionalSharpModuleInterface<ITargetingManager>(ITargetingManager.Identity)?.Instance;

            if (_targetingManager is null)
            {
                UI.EWSysInfo("EntWatch.Info.Error", 15, "TargetingManager is not installed. Target selectors will be limited.");
                return;
            }
        }

        private void TryResolveAdminManager()
        {
            if (_adminManager?.Instance is not null) return;

            _adminManager = _modules!.GetOptionalSharpModuleInterface<IAdminManager>(IAdminManager.Identity);

            if (_adminManager?.Instance is null)
            {
                UI.EWSysInfo("EntWatch.Info.Error", 15, "AdminManager is not installed. Admin commands will not work.");
                return;
            }

            AdminCommands_InitializePermissions();
        }

        int IGameListener.ListenerVersion => IGameListener.ApiVersion;
        int IGameListener.ListenerPriority => 0;
        int IEntityListener.ListenerVersion => IEntityListener.ApiVersion;
        int IEntityListener.ListenerPriority => 0;
        int IClientListener.ListenerVersion => IClientListener.ApiVersion;
        int IClientListener.ListenerPriority => 0;
    }
}
