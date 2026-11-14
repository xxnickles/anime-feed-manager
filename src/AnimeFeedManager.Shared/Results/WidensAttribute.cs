using System.Runtime.CompilerServices;

namespace AnimeFeedManager.Shared.Results;

// Consumed unions are struct unions (what `union` declares) holding only cases of the marked union (WID002).

/// <summary>Generates <see cref="IWidensFrom{TSelf, TFamily}"/> for each case of the <c>partial union</c> it marks.</summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
public sealed class WidensAttribute : Attribute;

/// <summary>Widens from each case and from <typeparamref name="TUnion1"/>.</summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
public sealed class WidensAttribute<TUnion1> : Attribute
    where TUnion1 : struct, IUnion;

/// <summary>Widens from each case and from each consumed union.</summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
public sealed class WidensAttribute<TUnion1, TUnion2> : Attribute
    where TUnion1 : struct, IUnion
    where TUnion2 : struct, IUnion;

/// <summary>Widens from each case and from each consumed union.</summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
public sealed class WidensAttribute<TUnion1, TUnion2, TUnion3> : Attribute
    where TUnion1 : struct, IUnion
    where TUnion2 : struct, IUnion
    where TUnion3 : struct, IUnion;

/// <summary>Widens from each case and from each consumed union.</summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
public sealed class WidensAttribute<TUnion1, TUnion2, TUnion3, TUnion4> : Attribute
    where TUnion1 : struct, IUnion
    where TUnion2 : struct, IUnion
    where TUnion3 : struct, IUnion
    where TUnion4 : struct, IUnion;
