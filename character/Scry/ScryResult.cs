// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Watcher, Version=1.5.2.0, Culture=neutral, PublicKeyToken=null
// Watcher.Code.Commands.ScryResult

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using MegaCrit.Sts2.Core.Models;

namespace Momizi.character.Scry;

public readonly struct ScryResult : IEquatable<ScryResult>
{
	private readonly IReadOnlyList<CardModel>? _discarded;

	public IReadOnlyList<CardModel> Discarded => _discarded ?? Array.Empty<CardModel>();

	public static ScryResult Empty => default(ScryResult);

	public ScryResult(IReadOnlyList<CardModel> discarded)
	{
		_discarded = discarded;
	}

	[CompilerGenerated]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ScryResult");
		stringBuilder.Append(" { ");
		if (PrintMembers(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("Discarded = ");
		builder.Append(Discarded);
		return true;
	}

	[CompilerGenerated]
	public static bool operator !=(ScryResult left, ScryResult right)
	{
		return !(left == right);
	}

	[CompilerGenerated]
	public static bool operator ==(ScryResult left, ScryResult right)
	{
		return left.Equals(right);
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		Debug.Assert(_discarded != null, nameof(_discarded) + " != null");
		return EqualityComparer<IReadOnlyList<CardModel>>.Default.GetHashCode(_discarded);
	}

	[CompilerGenerated]
	public override bool Equals(object? obj)
	{
		if (obj is ScryResult)
		{
			return Equals((ScryResult)obj);
		}
		return false;
	}

	[CompilerGenerated]
	public bool Equals(ScryResult other)
	{
		return EqualityComparer<IReadOnlyList<CardModel>>.Default.Equals(_discarded, other._discarded);
	}
}