using CursedMod.Events.Handlers.Player;
using CursedMod.Loader;
using CursedMod.Loader.Modules;
using CursedMod.Loader.Modules.Enums;

namespace InventoryControl_CursedMod
{
    public class Plugin : CursedModule
    {
        public override string ModuleName => "InventoryControl";
        public override string ModuleAuthor => "MrAfitol";
        public override string ModuleVersion => "1.0.0";
        public override byte LoadPriority => (byte)ModulePriority.Medium;
        public override string CursedModVersion => CursedModInformation.Version;

        public static Plugin Instance { get; private set; }

        public Config Config;

        public EventHandlers EventHandlers;

        public override void OnLoaded()
        {
            Instance = this;
            Config = GetConfig<Config>("config");
            EventHandlers = new EventHandlers();

            PlayerEventsHandler.ChangingRole += EventHandlers.OnPlayerChangingRole;

            base.OnLoaded();
        }

        public override void OnUnloaded()
        {
            PlayerEventsHandler.ChangingRole -= EventHandlers.OnPlayerChangingRole;

            EventHandlers = null;
            Instance = null;

            base.OnUnloaded();
        }
    }
}
