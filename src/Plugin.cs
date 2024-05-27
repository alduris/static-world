using BepInEx;
using MonoMod.RuntimeDetour;
using System.Security.Permissions;
using UnityEngine;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TestMod;

[BepInPlugin("alduris.static", "Static World", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    bool init;
    static Options options;

    public void OnEnable()
    {
        // Add hooks here
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (init) return;
        init = true;

        try
        {
            _ = new NativeDetour(typeof(Random).GetProperty(nameof(Random.value)).GetGetMethod(), typeof(Plugin).GetMethod(nameof(RandomValue)));
            _ = new NativeDetour(typeof(Random).GetMethod(nameof(Random.Range), [typeof(float), typeof(float)]), typeof(Plugin).GetMethod(nameof(RandomRangeFloat)));
            _ = new Hook(typeof(Random).GetMethod(nameof(Random.Range), [typeof(int), typeof(int)]), RandomRangeInt);

            options = new Options();
            MachineConnector.SetRegisteredOI("alduris.static", options);
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Randomness could not be controlled :(");
            Logger.LogError(ex);
        }

        // Initialize assets, your mod config, and anything that uses RainWorld here
        Logger.LogDebug("Hello world!");
    }

    public static float RandomValue() => options.LerpAmt.Value;
    public static float RandomRangeFloat(float min, float max) => Mathf.Lerp(min, max, options.LerpAmt.Value);
    public static int RandomRangeInt(int min, int max) => (int)Mathf.Lerp(min, max, options.LerpAmt.Value * 0.99999f); // because it can never equal max, which it will if set to 1f

    public class Options : OptionInterface
    {
        public Options()
        {
            LerpAmt = config.Bind("Static_amt", 0.5f);
        }
        public readonly Configurable<float> LerpAmt;
    }
}
