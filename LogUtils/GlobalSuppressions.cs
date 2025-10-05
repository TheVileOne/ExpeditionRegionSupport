// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "Naming the folder Properties changes the icon", Scope = "namespace", Target = "~N:LogUtils.Properties")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "Naming the folder Properties changes the icon", Scope = "namespace", Target = "~N:LogUtils.Properties.Custom")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "Naming the folder Properties changes the icon", Scope = "namespace", Target = "~N:LogUtils.Properties.Formatting")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "This is a special namespace", Scope = "namespace", Target = "~N:LogUtils.Diagnostics.Extensions")]
[assembly: SuppressMessage("Style", "IDE0180:Use tuple to swap values", Justification = "Preference", Scope = "member", Target = "~M:LogUtils.Diagnostics.Extensions.AssertHelper.MustBeBetween``1(LogUtils.Diagnostics.Condition{``0}@,``0,``0)")]
[assembly: SuppressMessage("Style", "IDE0180:Use tuple to swap values", Justification = "Preference", Scope = "member", Target = "~M:LogUtils.Diagnostics.Extensions.AssertHelper.MustBeBetween``1(LogUtils.Diagnostics.Condition{System.Nullable{``0}}@,``0,``0)")]
[assembly: SuppressMessage("Style", "IDE0180:Use tuple to swap values", Justification = "Preference", Scope = "member", Target = "~M:LogUtils.Helpers.Comparers.ExceptionComparer.checkMatchLineByLine(LogUtils.ExceptionInfo,LogUtils.ExceptionInfo)~System.Boolean")]
