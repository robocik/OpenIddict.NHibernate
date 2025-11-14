using System;
using System.ComponentModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using OpenIddict.NHibernate.Models;

namespace OpenIddict.NHibernate
{
	/// <summary>
	/// Defines a relational mapping for the Token entity.
	/// </summary>
	/// <typeparam name="TToken">The type of the Token entity.</typeparam>
	/// <typeparam name="TApplication">The type of the Application entity.</typeparam>
	/// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
	/// <typeparam name="TKey">The type of the Key entity.</typeparam>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class OpenIddictNHibernateTokenMapping<TToken, TApplication, TAuthorization, TKey> : ClassMapping<TToken>
		where TToken : OpenIddictNHibernateToken<TKey, TApplication, TAuthorization>
		where TApplication : OpenIddictNHibernateApplication<TKey, TAuthorization, TToken>
		where TAuthorization : OpenIddictNHibernateAuthorization<TKey, TApplication, TToken>
		where TKey : notnull, IEquatable<TKey>
	{
		public OpenIddictNHibernateTokenMapping()
		{
			this.Id(token => token.Id, map =>
			{
				map.Generator(Generators.Identity);
			});

			this.Version(token => token.ConcurrencyToken, map =>
			{
			map.Insert(true);
		});

		this.Property(token => token.CreationDate);
		this.Property(token => token.ExpirationDate);
		this.Property(token => token.RedemptionDate);

		this.Property(token => token.Payload, map =>
		{
			map.Length(10000);
		});

		this.Property(token => token.Properties, map =>
		{
			map.Length(10000);
		});

		this.Property(token => token.ReferenceId, map =>
		{
			map.Length(100);
			map.Index("IX_OpenIddictTokens_ReferenceId");
		});
		this.Property(token => token.Status, map =>
		{
			map.Length(50);
			map.Index("IX_OpenIddictTokens_Status");
		});
		this.Property(token => token.Subject, map =>
		{
			map.Length(400);
			map.Index("IX_OpenIddictTokens_Subject");
		});
		this.Property(token => token.Type, map =>
		{
			map.Length(150);
			map.Index("IX_OpenIddictTokens_Type");
		});

		this.ManyToOne(token => token.Application, map =>
		{
			map.Column("ApplicationId");
			map.ForeignKey("FK_OpenIddictTokens_OpenIddictApplications");
			map.Index("IX_OpenIddictTokens_ApplicationId");
		});

		this.ManyToOne(token => token.Authorization, map =>
		{
			map.Column("AuthorizationId");
			map.ForeignKey("FK_OpenIddictTokens_OpenIddictAuthorizations");
		});			this.Table("OpenIddictTokens");
		}
	}
}
