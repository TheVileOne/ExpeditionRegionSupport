using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using System;
using System.IO;

public class PathComparer : ComparerBase<string>
{
    public PathComparer() : base()
    {
    }

    public PathComparer(StringComparison comparisonOption) : base(comparisonOption)
    {
    }

    /// <summary>
    /// Compares two paths (with or without a filename)
    /// </summary>
    public int CompareFilenameAndPath(string path, string pathOther, bool ignoreExtensions)
    {
        //GetPathFromKeyword will strip the filename
        if (PathUtils.IsPathKeyword(path))
            path = PathUtils.GetPathFromKeyword(path);

        if (PathUtils.IsPathKeyword(pathOther))
            pathOther = PathUtils.GetPathFromKeyword(pathOther);

        if (ignoreExtensions)
        {
            path = FileUtils.RemoveExtension(path);
            pathOther = FileUtils.RemoveExtension(pathOther);
        }

        return InternalCompare(path, pathOther);
    }

    public override int Compare(string path, string pathOther)
    {
        //Make sure we are comparing path data, not keywords
        path = PathUtils.GetPathFromKeyword(path);
        pathOther = PathUtils.GetPathFromKeyword(pathOther);

        return InternalCompare(path, pathOther);
    }

    public override bool Equals(string path, string pathOther)
    {
        //Make sure we are comparing path data, not keywords
        path = PathUtils.GetPathFromKeyword(path);
        pathOther = PathUtils.GetPathFromKeyword(pathOther);

        return InternalEquals(path, pathOther);
    }

    /// <summary>
    /// Assumes path info is being compared, not keywords
    /// </summary>
    internal int InternalCompare(string path, string pathOther)
    {
        if (path == null)
            return pathOther != null ? int.MinValue : 0;

        if (pathOther == null)
            return int.MaxValue;

        path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        pathOther = Path.GetFullPath(pathOther).TrimEnd(Path.DirectorySeparatorChar);

        return base.Compare(path, pathOther);
    }

    /// <summary>
    /// Assumes path info is being compared, not keywords
    /// </summary>
    internal bool InternalEquals(string path, string pathOther)
    {
        if (path == null)
            return pathOther == null;

        if (pathOther == null)
            return false;

        path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        pathOther = Path.GetFullPath(pathOther).TrimEnd(Path.DirectorySeparatorChar);

        return base.Equals(path, pathOther);
    }
}
