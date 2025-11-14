using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.NHibernate.Models;
using Xunit;

namespace OpenIddict.NHibernate.Tests
{
	public class OpenIddictNHibernateExtensionsTests
	{
		[Fact]
		public void UseNHibernate_ThrowsAnExceptionForNullBuilder()
		{
			// Arrange
			var builder = (OpenIddictCoreBuilder) null;

			// Act and assert
			var exception = Assert.Throws<ArgumentNullException>(() => builder.UseNHibernate());

			Assert.Equal("builder", exception.ParamName);
		}

		[Fact]
		public void UseNHibernate_ThrowsAnExceptionForNullConfiguration()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = new OpenIddictCoreBuilder(services);

			// Act and assert
			var exception = Assert.Throws<ArgumentNullException>(() => builder.UseNHibernate(configuration: null));

			Assert.Equal("configuration", exception.ParamName);
		}

		// Note: DefaultApplicationType, DefaultAuthorizationType, DefaultScopeType, DefaultTokenType
		// properties no longer exist in OpenIddict 7.x as the registration model has changed.
		// In OpenIddict 7.x, stores are registered differently and are no longer available
		// as open generic types in the DI container. The stores are registered through the
		// ReplaceApplicationStore/ReplaceAuthorizationStore/etc. extension methods.
	}
}