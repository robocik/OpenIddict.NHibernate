using System;
using System.ComponentModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using OpenIddict.NHibernate.Models;

namespace OpenIddict.NHibernate
{
	/// <summary>
	/// Defines a relational mapping for the Scope entity.
	/// </summary>
	/// <typeparam name="TScope">The type of the Scope entity.</typeparam>
	/// <typeparam name="TKey">The type of the Key entity.</typeparam>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class OpenIddictNHibernateScopeMapping<TScope, TKey> : ClassMapping<TScope>
		where TScope : OpenIddictNHibernateScope<TKey>
		where TKey : notnull, IEquatable<TKey>
	{
		public OpenIddictNHibernateScopeMapping()
		{
			this.Id(scope => scope.Id, map =>
			{
				map.Generator(Generators.Assigned);
			});

			this.Version(scope => scope.ConcurrencyToken, map =>
			{
			map.Insert(true);
		});

		this.Property(scope => scope.Description);
		this.Property(scope => scope.Descriptions, map =>
		{
			map.Length(10000);
		});

		this.Property(scope => scope.DisplayName);
		this.Property(scope => scope.DisplayNames, map =>
		{
			map.Length(10000);
		});

		this.Property(scope => scope.Name, map =>
		{
			map.Length(200);
			map.Unique(true);
		});			this.Property(scope => scope.Properties, map =>
			{
				map.Length(10000);
			});

			this.Property(scope => scope.Resources, map =>
			{
				map.Length(10000);
			});

			this.Table("OpenIddictScopes");
		}
	}
}
