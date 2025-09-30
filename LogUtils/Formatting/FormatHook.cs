using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static LogUtils.Formatting.FormatDataAccess;

namespace LogUtils.Formatting
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hooks should ignore capitalization rules")]
    internal static class FormatHook
    {
        internal static IDetour[] Create()
        {
            MethodInfo appendFormatHelper = typeof(StringBuilder).GetMethod("AppendFormatHelper", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo hookMethod = typeof(FormatHook).GetMethod(nameof(StringBuilder_AppendFormatHelper), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            //This is required, because this method involves an internal class that cannot be referenced through the IDE
            var args = appendFormatHelper.GetParameters();

            IDetour[] hooks = new IDetour[]
            {
                new Hook(appendFormatHelper, hookMethod.MakeGenericMethod(args[2].ParameterType)),
                new ILHook(appendFormatHelper, IL_StringBuilder_AppendFormatHelper)
            };
            return hooks;
        }

        private static StringBuilder StringBuilder_AppendFormatHelper<T>(orig_AppendFormatHelper<T> orig, StringBuilder self, IFormatProvider provider, string format, T args)
        {
            var formatter = provider as IColorFormatProvider;

            //The formatter must implement an interface that LogUtils recognizes
            if (formatter == null)
                return orig(self, provider, format, args);

            var data = formatter.GetData();

            try
            {
                data.SetEntry(self);
                return orig(self, provider, format, args);
            }
            finally
            {
                data.EntryComplete(formatter);
            }
        }

        private static void IL_StringBuilder_AppendFormatHelper(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Match the third char comparison check - that's where the starting curly brace code is handled
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));

            cursor.Emit(OpCodes.Ldloc_3); //ICustomFormatter
            cursor.EmitDelegate(formatPlaceholderStart);

            cursor.GotoNext(MoveType.After, x => x.MatchLdarga(3),       //args array is accessed
                                            x => x.MatchLdloc(out _),    //Array indexer
                                            x => x.Match(OpCodes.Call)); //Argument at the provided index is fetched from the args array

            //Handle the char position of the right-most curly brace
            cursor.Emit(OpCodes.Ldloc_3); //ICustomFormatter
            cursor.Emit(OpCodes.Ldloc, 6); //Argument comma value
            cursor.EmitDelegate<Func<object, ICustomFormatter, int, object>>(FormatData.ResolveArgument);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(11)); //Assignment of local variable responsible for padding spaces

            ILLabel branchTarget = il.DefineLabel();

            //Put it back on the stack, and check that is not 0
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Brfalse, branchTarget);

            //When value is not 0, we need to check if we dealing with the right formatter object
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate(adjustFormatPadding);
            cursor.Emit(OpCodes.Stloc, 11); //Update padding value
            cursor.MarkLabel(branchTarget);
        }

        private static void formatPlaceholderStart(ICustomFormatter formatter)
        {
            FormatData.UpdateData(formatter as IColorFormatProvider);
        }

        private static int adjustFormatPadding(int paddingValue, ICustomFormatter formatter)
        {
            var provider = formatter as IColorFormatProvider;

            if (provider != null)
            {
                var data = provider.GetData();

                LinkedListNode<NodeData> currentNode = data.Entries.Last;
                FormatData placeholderData = currentNode.Value.Current;

                //The padding syntax is being borrowed - this will prevent any padding from being assigned when we are working with a color argument
                if (placeholderData.IsColorData)
                    return 0;
            }
            return paddingValue;
        }

        private delegate StringBuilder orig_AppendFormatHelper<T>(StringBuilder self, IFormatProvider provider, string format, T args);
        private delegate StringBuilder hook_AppendFormatHelper<T>(orig_AppendFormatHelper<T> orig, StringBuilder self, IFormatProvider provider, string format, T args);
    }
}
