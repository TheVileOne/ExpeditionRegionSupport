using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace LogUtils.Formatting
{
    internal static class FormatHook
    {
        internal static IDetour[] Create()
        {
            MethodInfo appendFormatHelper = typeof(StringBuilder).GetMethod("AppendFormatHelper", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo hookMethod = typeof(FormatHook).GetMethod("StringBuilder_AppendFormatHelper", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            
            //This is required, because this method involves an internal class that cannot be referenced through the IDE
            var args = appendFormatHelper.GetParameters();

            IDetour[] hooks = new IDetour[]
            {
                new Hook(appendFormatHelper, hookMethod.MakeGenericMethod(args[2].ParameterType)),
                new ILHook(appendFormatHelper, IL_StringBuilder_AppendFormatHelper)
            };
            return hooks;
        }

        private static ThreadLocal<int> recursiveAccessCount = new ThreadLocal<int>();

        private static StringBuilder StringBuilder_AppendFormatHelper<T>(orig_AppendFormatHelper<T> orig, StringBuilder self, IFormatProvider provider, string format, T args)
        {
            var formatter = provider as IColorFormatProvider;
            FormatDataCWT.Data formatCWT = null;

            if (formatter != null)
            {
                formatCWT = formatter.GetData();
                formatCWT.OnFormat();
            }

            try
            {
                recursiveAccessCount.Value++;
                return orig(self, provider, format, args);
            }
            finally
            {
                recursiveAccessCount.Value--;

                if (formatter != null && recursiveAccessCount.Value == 0)
                    formatCWT.CollectData();
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
            cursor.Emit(OpCodes.Ldloc_0); //Current index of format string
            cursor.EmitDelegate(formatPlaceholderStart);

            cursor.GotoNext(MoveType.After, x => x.MatchLdarga(3),       //args array is accessed
                                            x => x.MatchLdloc(out _),    //Array indexer
                                            x => x.Match(OpCodes.Call)); //Argument at the provided index is fetched from the args array

            //Handle the char position of the right-most curly brace
            cursor.Emit(OpCodes.Ldloc_3); //ICustomFormatter
            cursor.Emit(OpCodes.Ldarg_2); //Format string
            cursor.Emit(OpCodes.Ldloc, 4); //Argument index
            cursor.Emit(OpCodes.Ldloc, 6); //Argument comma value
            cursor.Emit(OpCodes.Ldloc_0); //Current index within format string
            cursor.EmitDelegate((object formatArgument, ICustomFormatter formatter, string format, int argIndex, int commaArg, int formatIndex) =>
            {
                var provider = formatter as IColorFormatProvider;

                if (provider != null)
                {
                    var formatData = provider.GetData();
                    var placeholderData = formatData.CurrentPlaceholder;

                    placeholderData.ArgumentIndex = argIndex;
                    placeholderData.CommaArgument = commaArg;

                    int placeholderStart = placeholderData.Position;
                    int placeholderLength = formatIndex - placeholderStart;

                    placeholderData.Format = format.Substring(placeholderStart, placeholderLength);

                    //UtilityLogger.Log("PLACEHOLDER FORMAT: " + placeholderData.Format);

                    if (formatArgument is Color)
                        formatArgument = new ColorPlaceholder((Color)formatArgument, placeholderData);

                    placeholderData.Argument = formatArgument;

                    //Replaced original struct with the more updated copy
                    formatData.CurrentPlaceholder = placeholderData;
                }
                return formatArgument;
            });

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(11)); //Assignment of local variable responsible for padding spaces

            ILLabel branchTarget = il.DefineLabel();

            //Put it back on the stack, and check that is not 0
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Brfalse, branchTarget);

            //When value is not 0, we need to check if we dealing with the right formatter object
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate((int value, ICustomFormatter formatter) =>
            {
                var provider = formatter as IColorFormatProvider;

                if (provider != null)
                {
                    var formatData = provider.GetData();
                    var placeholderData = formatData.CurrentPlaceholder;

                    //The padding syntax is being borrowed - this will prevent any padding from being assigned when we are working with a color argument
                    if (placeholderData.Argument is ColorPlaceholder)
                        return 0;
                }
                return value;
            });
            cursor.Emit(OpCodes.Stloc, 11); //Update padding value
            cursor.MarkLabel(branchTarget);
        }

        private static void formatPlaceholderStart(ICustomFormatter formatter, int index)
        {
            //We only need to touch CWT data in the context of dealing with a IColorFormatProvider implementation
            var provider = formatter as IColorFormatProvider;

            if (provider != null)
            {
                //UtilityLogger.Log("Placeholder start");
                var data = provider.GetData();
                data.CurrentPlaceholder.Position = index;
            }
        }

        private delegate StringBuilder orig_AppendFormatHelper<T>(StringBuilder self, IFormatProvider provider, string format, T args);
        private delegate StringBuilder hook_AppendFormatHelper<T>(orig_AppendFormatHelper<T> orig, StringBuilder self, IFormatProvider provider, string format, T args);
    }
}
