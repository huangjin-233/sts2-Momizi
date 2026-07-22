using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace Momizi
{
    [RegisterNodeAttachment(
        typeof(NCombatUi),
        "Alert",
        NodeName = "Alert",
        DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName)]
    public sealed partial class TestTurnCounter : Label, INodeAttachmentSetup
    {
        public void Setup(Node parent, Node node)
        {
            Text = "Alert";
            Position = new Vector2(40f, 84f);
        }
    }

    [ModInitializer(nameof(Initialize))]
    public static class ModInitializer
    {
        public const string ModId = "Momizi";
        public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);
        public static readonly ConcurrentDictionary<string, PackedScene> ModSceneCache = new();

        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
            ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
            LoadScenes();
            var harmony = new Harmony("Momizi.neow.patch");
            harmony.PatchAll(); // 自动扫描当前程序集的所有补丁

            Log.Info("犬走椛模组已加载");
        }

        static void LoadScenes()
        {
            //你的场景字符串列表
            var paths = new List<string>
            {
            };
            foreach (var path in paths)
            {
                if (ModSceneCache.ContainsKey(path)) continue;
                var scene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
                if (scene != null)
                {
                    ModSceneCache[path] = scene;
                }
            }
        }

    }
}
