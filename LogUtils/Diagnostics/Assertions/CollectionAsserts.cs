using LogUtils.Diagnostics.Extensions;
using System.Collections.Generic;
using AssertDocs = LogUtils.Documentation.AssertDocumentation;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.CollectionAssert.HasItems{T}(Condition{IEnumerable{T}})"/>
        public static Condition<IEnumerable<T>> HasItems<T>(this Condition<IEnumerable<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<IEnumerable<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.HasItems{T}(Condition{IEnumerable{T}})"/>
        public static Condition<ICollection<T>> HasItems<T>(this Condition<ICollection<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<ICollection<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.HasItems{T}(Condition{IEnumerable{T}})"/>
        public static Condition<IList<T>> HasItems<T>(this Condition<IList<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<IList<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.HasItems{T}(Condition{IEnumerable{T}})"/>
        public static Condition<List<T>> HasItems<T>(this Condition<List<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<List<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.HasItems{T}(Condition{IEnumerable{T}})"/>
        public static Condition<T[]> HasItems<T>(this Condition<T[]> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<T[], T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.IsNullOrEmpty{T}(Condition{IEnumerable{T}})"/>
        public static Condition<IEnumerable<T>> IsNullOrEmpty<T>(this Condition<IEnumerable<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<IEnumerable<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.IsNullOrEmpty{T}(Condition{IEnumerable{T}})"/>
        public static Condition<ICollection<T>> IsNullOrEmpty<T>(this Condition<ICollection<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<ICollection<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.IsNullOrEmpty{T}(Condition{IEnumerable{T}})"/>
        public static Condition<IList<T>> IsNullOrEmpty<T>(this Condition<IList<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<IList<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.IsNullOrEmpty{T}(Condition{IEnumerable{T}})"/>
        public static Condition<List<T>> IsNullOrEmpty<T>(this Condition<List<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<List<T>, T>(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.CollectionAssert.IsNullOrEmpty{T}(Condition{IEnumerable{T}})"/>
        public static Condition<T[]> IsNullOrEmpty<T>(this Condition<T[]> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<T[], T>(ref condition);
            return condition;
        }
    }
}