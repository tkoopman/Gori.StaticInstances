// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "By design we add these attributes to the main namespace.", Scope = "namespace", Target = "~N:Gori.StaticInstances")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "N/A", Scope = "member", Target = "~M:Gori.StaticInstances.StaticInstanceAttribute.#ctor(System.Object[])")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "N/A", Scope = "member", Target = "~M:Gori.StaticInstances.StaticInstanceAttribute.#ctor(System.Object[])")]
