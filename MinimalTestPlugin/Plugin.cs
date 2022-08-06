using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;

namespace MinimalTestPlugin
{
    public unsafe class Plugin : IDalamudPlugin
    {
        public string Name => "MinimalTestPlugin";
        delegate long OnEmoteFuncDelegate(IntPtr a1, IntPtr source, ushort emoteId, long targetId, long a5);
        [Signature("E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ?? 4C 8B CE", DetourName = nameof(OnEmoteFuncDetour))]
        Hook<OnEmoteFuncDelegate> OnEmoteFuncHook;
        private readonly DalamudPluginInterface pi;
        private readonly ChatGui Chat;
        private readonly ObjectTable Objects;
        private readonly DataManager Data;

        public Plugin(DalamudPluginInterface pi, ChatGui Chat, ObjectTable Objects, DataManager Data)
        {
            this.pi = pi;
            this.Chat = Chat;
            this.Objects = Objects;
            this.Data = Data;
            SignatureHelper.Initialise(this);
            OnEmoteFuncHook.Enable();
        }

        public void Dispose()
        {
            OnEmoteFuncHook.Dispose();
        }
        

        long OnEmoteFuncDetour(IntPtr a1, IntPtr source, ushort emoteId, long targetId, long a5)
        {
            try
            {
                Chat.Print($"{source:X16}");
                var gameObject = Objects.CreateObjectReference(source);
                var emoteName = Data.GetExcelSheet<Emote>().GetRow(emoteId).Name;
                var target = Objects.FirstOrDefault(x => ((GameObject*)x.Address)->GetObjectID() == targetId);
                Chat.Print($">> {gameObject.Name} used emote {emoteName}" + (target != null ? $" on {target.Name}" : ""));
            }
            catch (Exception e)
            {
                Chat.Print($"{e.Message}\n{e.StackTrace}");
            }
            return OnEmoteFuncHook.Original(a1, source, emoteId, targetId, a5);
        }
    }
}
