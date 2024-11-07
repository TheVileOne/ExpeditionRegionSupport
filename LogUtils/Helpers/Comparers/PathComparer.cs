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

    public override int Compare(string path, string pathOther)
    {
        //Make sure we are comparing path data, not keywords
        path = PathUtils.GetPathFromKeyword(path);
        pathOther = PathUtils.GetPathFromKeyword(path);

        return InternalCompare(path, pathOther);
    }

    public override bool Equals(string path, string pathOther)
    {
        //Make sure we are comparing path data, not keywords
        path = PathUtils.GetPathFromKeyword(path);
        pathOther = PathUtils.GetPathFromKeyword(path);

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

        path = Path.GetFullPath(path).TrimEnd('\\');
        pathOther = Path.GetFullPath(pathOther).TrimEnd('\\');

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

        path = Path.GetFullPath(path).TrimEnd('\\');
        pathOther = Path.GetFullPath(pathOther).TrimEnd('\\');

        return base.Equals(path, pathOther);
    }
}
