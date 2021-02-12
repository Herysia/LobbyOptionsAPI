using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace LobbyOptionsAPI
{
    public abstract class LobbyOptions : ScrollerOptionsManager
    {
        private static List<LobbyOptions> lobbyOptions = new List<LobbyOptions>();

        protected StringBuilder settings = new StringBuilder(2048);
        private string fileName;
        private byte rpcId;
        private int priority;

        public LobbyOptions(string fileName, byte rpcId, int priority = Priority.Normal)
        {
            this.fileName = fileName;
            this.rpcId = rpcId;
            this.priority = priority;
            lobbyOptions.Add(this);
        }

        public abstract void SetRecommendations();
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
        public abstract void Deserialize(MessageReader reader);

        public override string ToString()
        {
            return this.ToHudString();
        }

        public abstract string ToHudString();

        public byte[] ToBytes()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    this.Serialize(binaryWriter);
                    binaryWriter.Flush();
                    memoryStream.Position = 0L;
                    result = memoryStream.ToArray();
                }
            }

            return result;
        }

        public void FromBytes(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    this.Deserialize(binaryReader);
                }
            }
        }

        public void LoadGameOptions(string filename)
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path))
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        Deserialize(binaryReader);
                    }
                }
            }
        }

        private void SaveGameOptions(string filename)
        {
            using (FileStream fileStream = new FileStream(Path.Combine(Application.persistentDataPath, filename),
                FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    Serialize(binaryWriter);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        public static class PlayerControl_RpcSyncSettings
        {
            public static void Postfix(GameOptionsData IOFBPLNIJIC)
            {
                if (PlayerControl.AllPlayerControls.Count > 1)
                {
                    foreach (var opt in lobbyOptions)
                    {
                        MessageWriter messageWriter =
                            AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, opt.rpcId,
                                SendOption.Reliable);
                        messageWriter.WriteBytesAndSize(opt.ToBytes());
                        messageWriter.EndMessage();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class PlayerControl_HandleRpc
        {
            public static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                foreach (var opt in lobbyOptions)
                {
                    if (HKHMBLJFLMC == opt.rpcId)
                    {
                        opt.FromBytes(ALMCIJKELCP.ReadBytesAndSize());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_68))] //SetRecommendations
        public static class GameOptionsData_SetRecommendations
        {
            public static void Postfix()
            {
                foreach (var opt in lobbyOptions)
                {
                    opt.SetRecommendations();
                }
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Method_47))] //LoadGameOptions
        public static class SaveManager_LoadGameOptions
        {
            public static void Postfix(string HAGKNHFOMIJ)
            {
                if (HAGKNHFOMIJ == "gameHostOptions")
                {
                    foreach (var opt in lobbyOptions)
                    {
                        opt.LoadGameOptions(opt.fileName);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveGameOptions))] //SaveGameOptions
        public static class SaveManager_SaveGameOptions
        {
            public static void Postfix(string HAGKNHFOMIJ)
            {
                if (HAGKNHFOMIJ == "gameHostOptions")
                {
                    foreach (var opt in lobbyOptions)
                    {
                        opt.SaveGameOptions(opt.fileName);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
        static class GameOptionsData_ToHudString
        {
            [HarmonyPriority(Priority.Normal - 1)]
            static void Postfix(ref string __result)
            {
                var builder = new System.Text.StringBuilder(__result);
                foreach (var lby in lobbyOptions)
                {
                    builder.Append(lby.ToHudString());
                }

                __result = builder.ToString();

                DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.5f;
            }
        }
    }
}