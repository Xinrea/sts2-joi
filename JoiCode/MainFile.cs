using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Joi;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Joi"; //Used for resource filepath

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Logger.Info("[JOI] Initializing Joi mod");

        Harmony harmony = new(ModId);
        harmony.PatchAll();

        Logger.Info("[JOI] Mod initialization complete");
    }
}
