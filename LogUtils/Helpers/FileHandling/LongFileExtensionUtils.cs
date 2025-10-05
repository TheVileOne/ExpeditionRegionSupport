using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    internal static class LongFileExtensionUtils
    {
        internal static string RemoveIgnore(string target, out string fileExt)
        {
            FileExtensionInfo extInfo = FileExtensionInfo.FromFilename(target);

            bool extensionCanBeRemoved = !extInfo.IsEmpty && !extInfo.IsLong;

            if (!extensionCanBeRemoved)
            {
                //Since we cannot extract file extension info, we will default to an empty string
                fileExt = string.Empty;
                return target;
            }

            //Assign the correct file extension, and return the target substring without the extension
            fileExt = extInfo.Extension;
            target = target.TrimEnd(); //Account for trailing whitespace

            int newTargetLength = target.Length - fileExt.Length;
            return target.Substring(0, newTargetLength);
        }

        internal static string RemoveSupportedOnly(string target, out string fileExt)
        {
            FileExtensionInfo extInfo = FileExtensionInfo.FromFilename(target);

            bool extensionCanBeRemoved = !extInfo.IsEmpty && (!extInfo.IsLong || extInfo.IsSupported);

            if (!extensionCanBeRemoved)
            {
                //Since we cannot extract file extension info, we will default to an empty string
                fileExt = string.Empty;
                return target;
            }

            //Assign the correct file extension, and return the target substring without the extension
            fileExt = extInfo.Extension;
            target = target.TrimEnd(); //Account for trailing whitespace

            int newTargetLength = target.Length - fileExt.Length;
            return target.Substring(0, newTargetLength);
        }

        internal static string ReplaceIgnore(string target, string extension)
        {
            FileExtensionInfo extensionProvided = FileExtensionInfo.FromExtension(extension),
                              extensionTarget = FileExtensionInfo.FromFilename(target);

            bool extensionCanBeProvided = !extensionProvided.IsLong;
            bool extensionCanBeReplaced = !extensionTarget.IsLong;

            if (extensionCanBeProvided)
            {
                if (extensionCanBeReplaced)
                    return Path.ChangeExtension(target, extensionProvided.Extension);
                return target + extensionProvided.Extension;
            }
            return target;
        }

        internal static string ReplaceSupportedOnly(string target, string extension)
        {
            FileExtensionInfo extensionProvided = FileExtensionInfo.FromExtension(extension),
                              extensionTarget = FileExtensionInfo.FromFilename(target);
            /*
             * A file extension must satisfy one of these conditions to be provided, or replaced
             * I.  The file extension is not a long file extension
             * II. The file extension is a supported extension (i.e. LogUtils recognizes and supports the extension)
             */
            bool extensionCanBeProvided = !extensionProvided.IsLong || extensionProvided.IsSupported;
            bool extensionCanBeReplaced = !extensionTarget.IsLong || extensionTarget.IsSupported;

            if (extensionCanBeProvided)
            {
                if (extensionCanBeReplaced)
                    return Path.ChangeExtension(target, extensionProvided.Extension);
                return target + extensionProvided.Extension;
            }
            return target;
        }
    }
}
