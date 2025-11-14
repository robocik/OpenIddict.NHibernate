using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NHibernate;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.NHibernate.Models;

namespace OpenIddict.NHibernate
{
	/// <summary>
	/// Exposes the necessary methods required to configure the OpenIddict NHibernate services.
	/// </summary>
	public sealed class OpenIddictNHibernateBuilder
	{
		/// <summary>
		/// Initializes a new instance of <see cref="OpenIddictNHibernateBuilder"/>.
		/// </summary>
		/// <param name="services">The services collection.</param>
		public OpenIddictNHibernateBuilder(IServiceCollection? services)
		{
			ArgumentNullException.ThrowIfNull(services);

			this.Services = services;
		}

		/// <summary>
		/// Gets the services collection.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IServiceCollection Services { get; }

		/// <summary>
		/// Amends the default OpenIddict NHibernate configuration.
		/// </summary>
		/// <param name="configuration">The delegate used to configure the OpenIddict options.</param>
		/// <remarks>This extension can be safely called multiple times.</remarks>
		/// <returns>The <see cref="OpenIddictNHibernateBuilder"/>.</returns>
		public OpenIddictNHibernateBuilder Configure(Action<OpenIddictNHibernateOptions>? configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			this.Services.Configure(configuration);

			return this;
		}

		/// <summary>
		/// Configures the NHibernate stores to use the specified session factory
		/// instead of retrieving it from the dependency injection container.
		/// </summary>
		/// <param name="factory">The <see cref="ISessionFactory"/>.</param>
		/// <returns>The <see cref="OpenIddictNHibernateBuilder"/>.</returns>
		public OpenIddictNHibernateBuilder UseSessionFactory(ISessionFactory? factory)
		{
			ArgumentNullException.ThrowIfNull(factory);

			return this.Configure(options => options.SessionFactory = factory);
		}

		/// <summary>
		/// Configures OpenIddict to use the default OpenIddict Entity Framework entities, with the specified key type.
		/// </summary>
		/// <returns>The <see cref="OpenIddictNHibernateBuilder"/>.</returns>
		public OpenIddictNHibernateBuilder ReplaceDefaultEntities<TKey>()
			where TKey : notnull, IEquatable<TKey>
		{
			return this.ReplaceDefaultEntities<OpenIddictNHibernateApplication<TKey>, OpenIddictNHibernateAuthorization<TKey>, OpenIddictNHibernateScope<TKey>, OpenIddictNHibernateToken<TKey>, TKey>();
		}

		/// <summary>
		/// Configures OpenIddict to use the specified entities, derived from the default OpenIddict Entity Framework entities.
		/// </summary>
		/// <returns>The <see cref="OpenIddictNHibernateBuilder"/>.</returns>
		public OpenIddictNHibernateBuilder ReplaceDefaultEntities<TApplication, TAuthorization, TScope, TToken, TKey>()
			where TApplication : OpenIddictNHibernateApplication<TKey, TAuthorization, TToken>
			where TAuthorization : OpenIddictNHibernateAuthorization<TKey, TApplication, TToken>
			where TScope : OpenIddictNHibernateScope<TKey>
			where TToken : OpenIddictNHibernateToken<TKey, TApplication, TAuthorization>
			where TKey : notnull, IEquatable<TKey>
		{
#if SUPPORTS_TYPE_DESCRIPTOR_TYPE_REGISTRATION
			// If the specified key type isn't a string (which is special-cased by the stores to avoid having to resolve
			// a TypeDescriptor instance) and the platform supports type registration, register the key type to ensure the
			// TypeDescriptor associated with that type will be preserved by the IL Linker and can be resolved at runtime.
			if (typeof(TKey) != typeof(string))
			{
				TypeDescriptor.RegisterType<TKey>();
			}
#endif

			Services.Replace(ServiceDescriptor.Scoped<IOpenIddictApplicationManager>(provider => provider.GetRequiredService<OpenIddictApplicationManager<TApplication>>()));
			Services.Replace(ServiceDescriptor.Scoped<IOpenIddictAuthorizationManager>(provider => provider.GetRequiredService<OpenIddictAuthorizationManager<TAuthorization>>()));
			Services.Replace(ServiceDescriptor.Scoped<IOpenIddictScopeManager>(provider => provider.GetRequiredService<OpenIddictScopeManager<TScope>>()));
			Services.Replace(ServiceDescriptor.Scoped<IOpenIddictTokenManager>(provider => provider.GetRequiredService<OpenIddictTokenManager<TToken>>()));

		Services.Replace(ServiceDescriptor.Scoped<IOpenIddictApplicationStore<TApplication>, OpenIddictNHibernateApplicationStore<TApplication, TAuthorization, TToken, TKey>>());
		Services.Replace(ServiceDescriptor.Scoped<IOpenIddictAuthorizationStore<TAuthorization>, OpenIddictNHibernateAuthorizationStore<TAuthorization, TApplication, TToken, TKey>>());
		Services.Replace(ServiceDescriptor.Scoped<IOpenIddictScopeStore<TScope>, OpenIddictNHibernateScopeStore<TScope, TKey>>());
		Services.Replace(ServiceDescriptor.Scoped<IOpenIddictTokenStore<TToken>, OpenIddictNHibernateTokenStore<TToken, TApplication, TAuthorization, TKey>>());

		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj) => base.Equals(obj);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode() => base.GetHashCode();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string? ToString() => base.ToString();
}
}