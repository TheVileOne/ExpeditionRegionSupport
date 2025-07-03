using LogUtils.Diagnostics;
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

        private static ThreadLocal<bool> isHookEntered = new ThreadLocal<bool>();

        //private static void appendFormatHook(StringBuilder self, IFormatProvider formatter, string format, object paramsArray)
        //{
        //    if (isHookEntered.Value)
        //    {
        //        isHookEntered.Value = false; //We only need to check this once per hook call. Setting to false allows recursive calls to AppendFormatHelper to function
        //        return;
        //    }

        //    isHookEntered.Value = true;

        //    try
        //    {
        //        DynamicMethodDefinition test = null;
        //        appendHelperMethod.Invoke(self, new object[] { formatter, format, paramsArray });
        //    }
        //    finally
        //    {
        //        //Cleanup
        //    }
        //    return;
        //}

        private static StringBuilder StringBuilder_AppendFormatHelper<T>(orig_AppendFormatHelper<T> orig, StringBuilder self, IFormatProvider provider, string format, T args)
        {
            var formatter = provider as IColorFormatProvider;

            //The formatter must implement an interface that LogUtils recognizes
            if (formatter == null)
                return orig(self, provider, format, args);

            UtilityLogger.DebugLog("New entry");
            var data = formatter.GetData();
            data.AddNodeEntry(self);

            if (data.RangeCounter > 0)
                UtilityLogger.DebugLog($"Expecting {data.RangeCounter} more characters");

            try
            {
                return orig(self, provider, format, args);
            }
            finally
            {
                UtilityLogger.DebugLog("Finally");
                LinkedListNode<NodeData> currentNode = data.Entries.Last;
                NodeData currentBuildEntry = currentNode.Value;

                if (data.UpdateBuildLength())
                {
                    //Handle color reset
                    currentBuildEntry.Current = new FormatData()
                    {
                        Position = currentBuildEntry.LastCheckedBuildLength
                    };
                    formatter.ResetColor(self);

                    if (currentBuildEntry.LastCheckedBuildLength != currentBuildEntry.Builder.Length)
                    {
                        int charsRemaining = currentBuildEntry.Builder.Length - currentBuildEntry.LastCheckedBuildLength - 1;
                        UtilityLogger.DebugLog($"Substring '{currentBuildEntry.Builder.ToString().Substring(currentBuildEntry.Builder.Length - charsRemaining)}' has the reset color");
                    }
                }

                int lastBuildLength = currentBuildEntry.LastCheckedBuildLength = currentBuildEntry.Builder.Length;
                data.RemoveLastNodeEntry();

                currentNode = data.Entries.Last;
                if (currentNode != null)
                    currentNode.Value.LastCheckedBuildLength += lastBuildLength;

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
            cursor.EmitDelegate(resolveArgumentData);

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
            var provider = formatter as IColorFormatProvider;

            if (provider != null)
            {
                UtilityLogger.DebugLog("Placeholder start");
                var data = provider.GetData();

                LinkedListNode<NodeData> currentNode = data.Entries.Last;
                LinkedListNode<NodeData> previousNode = currentNode.Previous;

                NodeData currentBuildEntry = currentNode.Value;
                StringBuilder currentBuilder = currentBuildEntry.Builder;

                //Position in the string is the combined length of strings built up until this point. Since this value is cumulative, we only need to reference the
                //format data of the last build node for an accurate length
                int positionOffset = 0;
                if (previousNode != null)
                    positionOffset = previousNode.Value.Current.Position;

                int positionInString = currentBuilder.Length + positionOffset;

                //UtilityLogger.DebugLog("Build content: " + currentBuilder);
                //UtilityLogger.DebugLog("Build position: " + currentBuildEntry.LastCheckedBuildLength);
                if (data.UpdateBuildLength())
                {
                    //Handle color reset
                    currentBuildEntry.Current = new FormatData()
                    {
                        Position = currentBuildEntry.LastCheckedBuildLength + positionOffset
                    };
                    provider.ResetColor(currentBuilder);
                }

                //This will replace the last FormatData instance with the current one - this is by design
                currentBuildEntry.Current = new FormatData()
                {
                    Position = positionInString
                };
            }
        }

        private static object resolveArgumentData(object formatArgument, ICustomFormatter formatter, int commaValue)
        {
            var provider = formatter as IColorFormatProvider;

            if (provider != null)
            {
                var data = provider.GetData();

                LinkedListNode<NodeData> currentNode = data.Entries.Last;

                FormatData currentEntry = currentNode.Value.Current;
                currentEntry.Argument = formatArgument;

                if (currentEntry.IsColorData)
                {
                    Assert.That(data.RangeCounter).IsZero();

                    currentEntry.Range = commaValue;
                    data.RangeCounter = Math.Max(currentEntry.Range, 0);
                    return currentEntry;
                }
            }
            return formatArgument;
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
