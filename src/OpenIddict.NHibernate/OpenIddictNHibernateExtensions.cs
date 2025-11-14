using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.NHibernate.Models;

namespace OpenIddict.NHibernate
{
	/// <summary>
	/// Exposes extensions allowing to register the OpenIddict NHibernate services.
	/// </summary>
	public static class OpenIddictNHibernateExtensions
	{
		/// <summary>
		/// Registers the NHibernate stores services in the DI container and
		/// configures OpenIddict to use the NHibernate entities by default.
		/// </summary>
		/// <param name="builder">The services builder used by OpenIddict to register new services.</param>
		/// <remarks>This extension can be safely called multiple times.</remarks>
		/// <returns>The <see cref="OpenIddictNHibernateBuilder"/>.</returns>
	public static OpenIddictNHibernateBuilder UseNHibernate(this OpenIddictCoreBuilder? builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		// Since NHibernate may be used with databases performing case-insensitive or
		// culture-sensitive comparisons, ensure the additional filtering logic is enforced
		// in case case-sensitive stores were registered before this extension was called.
		builder.Configure(options => options.DisableAdditionalFiltering = false);

		builder
			.SetDefaultApplicationEntity<OpenIddictNHibernateApplication>()
			.SetDefaultAuthorizationEntity<OpenIddictNHibernateAuthorization>()
			.SetDefaultScopeEntity<OpenIddictNHibernateScope>()
			.SetDefaultTokenEntity<OpenIddictNHibernateToken>();

		builder
			.ReplaceApplicationStore<OpenIddictNHibernateApplicationStore>()
			.ReplaceAuthorizationStore<OpenIddictNHibernateAuthorizationStore>()
			.ReplaceScopeStore<OpenIddictNHibernateScopeStore>()
			.ReplaceTokenStore<OpenIddictNHibernateTokenStore>();

		//// Note: a default session factory is always registered to make debugging easier when
		//// no session type was configured by the user: the default implementation
		//// registered here is automatically replaced by the UseSessionFactory() API.
		//builder.Services.TryAddScoped<IOpenIddictNHibernateContext>(provider =>
		//{
		//	throw new InvalidOperationException("No NHibernate session was configured to be used with OpenIddict.\\n" +
		//				 "To configure the OpenIddict NHibernate stores to use a specific 'ISession', use 'options.UseNHibernate().UseSessionFactory()'.");
		//});

		builder.Services.TryAddScoped<IOpenIddictNHibernateContext, OpenIddictNHibernateContext>();

		return new OpenIddictNHibernateBuilder(builder.Services);
	}		/// <summary>
		/// Registers the NHibernate stores services in the DI container and
		/// configures OpenIddict to use the NHibernate entities by default.
		/// </summary>
		/// <param name="builder">The services builder used by OpenIddict to register new services.</param>
		/// <param name="configuration">The configuration delegate used to configure the NHibernate services.</param>
		/// <remarks>This extension can be safely called multiple times.</remarks>
		/// <returns>The <see cref="OpenIddictCoreBuilder"/>.</returns>
		public static OpenIddictCoreBuilder UseNHibernate(this OpenIddictCoreBuilder? builder
			, Action<OpenIddictNHibernateBuilder>? configuration
		)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			configuration(builder.UseNHibernate());

			return builder;
		}
	}
}
