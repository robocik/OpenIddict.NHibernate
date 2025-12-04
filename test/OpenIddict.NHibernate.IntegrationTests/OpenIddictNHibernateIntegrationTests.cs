using System;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.NHibernate;
using OpenIddict.NHibernate.Models;
using Xunit;

namespace OpenIddict.NHibernate.IntegrationTests
{
	public class OpenIddictNHibernateIntegrationTests : IDisposable
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ISessionFactory _sessionFactory;
		private readonly SQLiteConnection _connection;

		public OpenIddictNHibernateIntegrationTests()
		{
			// Create in-memory SQLite database
			_connection = new SQLiteConnection("Data Source=:memory:");
			_connection.Open();

			// Configure NHibernate
			var configuration = new Configuration();
			configuration.DataBaseIntegration(db =>
			{
				db.Dialect<SQLiteDialect>();
				db.Driver<SQLite20Driver>();
				db.ConnectionString = _connection.ConnectionString;
				db.LogSqlInConsole = true;
			});

			// Add mappings
			var mapper = new ModelMapper();
			mapper.AddMapping<OpenIddictNHibernateApplicationMapping<OpenIddictNHibernateApplication, OpenIddictNHibernateAuthorization, OpenIddictNHibernateToken, string>>();
			mapper.AddMapping<OpenIddictNHibernateAuthorizationMapping<OpenIddictNHibernateAuthorization, OpenIddictNHibernateApplication, OpenIddictNHibernateToken, string>>();
			mapper.AddMapping<OpenIddictNHibernateScopeMapping<OpenIddictNHibernateScope, string>>();
			mapper.AddMapping<OpenIddictNHibernateTokenMapping<OpenIddictNHibernateToken, OpenIddictNHibernateApplication, OpenIddictNHibernateAuthorization, string>>();

			var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
			configuration.AddMapping(mapping);

			_sessionFactory = configuration.BuildSessionFactory();

			// Create schema
			new SchemaExport(configuration).Execute(true, true, false, _connection, null);

			// Setup DI container
			var services = new ServiceCollection();
			
			services.AddMemoryCache();
			
			services.AddOpenIddict()
				.AddCore(options =>
				{
					options.UseNHibernate()
						.UseSessionFactory(_sessionFactory);
				});

			_serviceProvider = services.BuildServiceProvider();
		}

		[Fact]
		public async Task Can_Create_And_Retrieve_Application()
		{
			// Arrange
			var manager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "test-client",
				DisplayName = "Test Application",
				Permissions =
				{
					OpenIddictConstants.Permissions.Endpoints.Token,
					OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
				}
			};

			// Act
			var application = await manager.CreateAsync(descriptor);
			var clientId = await manager.GetClientIdAsync(application);
			var retrieved = await manager.FindByClientIdAsync("test-client");

			// Assert
			Assert.NotNull(application);
			Assert.Equal("test-client", clientId);
			Assert.NotNull(retrieved);
			Assert.Equal("Test Application", await manager.GetDisplayNameAsync(retrieved));
		}

		[Fact]
		public async Task Can_Update_Application()
		{
			// Arrange
			var manager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "update-test",
				DisplayName = "Original Name"
			};

			var application = await manager.CreateAsync(descriptor);

			// Act
			await manager.UpdateAsync(application, new OpenIddictApplicationDescriptor
			{
				ClientId = "update-test",
				DisplayName = "Updated Name"
			});

			var retrieved = await manager.FindByClientIdAsync("update-test");

			// Assert
			Assert.NotNull(retrieved);
			Assert.Equal("Updated Name", await manager.GetDisplayNameAsync(retrieved));
		}

		[Fact]
		public async Task Can_Delete_Application()
		{
			// Arrange
			var manager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "delete-test",
				DisplayName = "To Be Deleted"
			};

			var application = await manager.CreateAsync(descriptor);

			// Act
			await manager.DeleteAsync(application);
			var retrieved = await manager.FindByClientIdAsync("delete-test");

			// Assert
			Assert.Null(retrieved);
		}

		[Fact]
		public async Task Can_Create_And_Retrieve_Authorization()
		{
			// Arrange
			var appManager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var authManager = _serviceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();

			var appDescriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "auth-test-client",
				DisplayName = "Auth Test App"
			};

			var application = await appManager.CreateAsync(appDescriptor);
			var appId = await appManager.GetIdAsync(application);

			var authDescriptor = new OpenIddictAuthorizationDescriptor
			{
				ApplicationId = appId,
				Subject = "test-user",
				Type = OpenIddictConstants.AuthorizationTypes.Permanent,
				Status = OpenIddictConstants.Statuses.Valid
			};

			// Act
			var authorization = await authManager.CreateAsync(authDescriptor);
			var subject = await authManager.GetSubjectAsync(authorization);
			var authorizations = await authManager.FindBySubjectAsync("test-user").ToListAsync();

			// Assert
			Assert.NotNull(authorization);
			Assert.Equal("test-user", subject);
			Assert.Single(authorizations);
		}

		[Fact]
		public async Task Can_Create_And_Retrieve_Scope()
		{
			// Arrange
			var manager = _serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
			var descriptor = new OpenIddictScopeDescriptor
			{
				Name = "test-scope",
				DisplayName = "Test Scope",
				Resources = { "test-resource" }
			};

			// Act
			var scope = await manager.CreateAsync(descriptor);
			var name = await manager.GetNameAsync(scope);
			var retrieved = await manager.FindByNameAsync("test-scope");

			// Assert
			Assert.NotNull(scope);
			Assert.Equal("test-scope", name);
			Assert.NotNull(retrieved);
			Assert.Equal("Test Scope", await manager.GetDisplayNameAsync(retrieved));
		}

		[Fact]
		public async Task Can_Create_And_Retrieve_Token()
		{
			// Arrange
			var appManager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var authManager = _serviceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
			var tokenManager = _serviceProvider.GetRequiredService<IOpenIddictTokenManager>();

			var appDescriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "token-test-client",
				DisplayName = "Token Test App"
			};

			var application = await appManager.CreateAsync(appDescriptor);
			var appId = await appManager.GetIdAsync(application);

			var authDescriptor = new OpenIddictAuthorizationDescriptor
			{
				ApplicationId = appId,
				Subject = "token-test-user",
				Type = OpenIddictConstants.AuthorizationTypes.Permanent,
				Status = OpenIddictConstants.Statuses.Valid
			};

			var authorization = await authManager.CreateAsync(authDescriptor);
			var authId = await authManager.GetIdAsync(authorization);

			var tokenDescriptor = new OpenIddictTokenDescriptor
			{
				ApplicationId = appId,
				AuthorizationId = authId,
				Subject = "token-test-user",
				Type = OpenIddictConstants.TokenTypeHints.AccessToken,
				Status = OpenIddictConstants.Statuses.Valid,
				Payload = "test-payload"
			};

			// Act
			var token = await tokenManager.CreateAsync(tokenDescriptor);
			var subject = await tokenManager.GetSubjectAsync(token);
			var tokens = await tokenManager.FindBySubjectAsync("token-test-user").ToListAsync();

			// Assert
			Assert.NotNull(token);
			Assert.Equal("token-test-user", subject);
			Assert.Single(tokens);
		}

		[Fact]
		public async Task Can_Revoke_Tokens_By_Application()
		{
			// Arrange
			var appManager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var tokenManager = _serviceProvider.GetRequiredService<IOpenIddictTokenManager>();

			var appDescriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "revoke-test-client",
				DisplayName = "Revoke Test App"
			};

			var application = await appManager.CreateAsync(appDescriptor);
			var appId = await appManager.GetIdAsync(application);

			var tokenDescriptor = new OpenIddictTokenDescriptor
			{
				ApplicationId = appId,
				Subject = "revoke-test-user",
				Type = OpenIddictConstants.TokenTypeHints.AccessToken,
				Status = OpenIddictConstants.Statuses.Valid
			};

			var token = await tokenManager.CreateAsync(tokenDescriptor);

			// Act
			await tokenManager.RevokeByApplicationIdAsync(appId);
			var retrievedToken = await tokenManager.FindByIdAsync(await tokenManager.GetIdAsync(token));

			// Assert
			Assert.NotNull(retrievedToken);
			Assert.Equal(OpenIddictConstants.Statuses.Revoked, await tokenManager.GetStatusAsync(retrievedToken));
		}

		[Fact]
		public async Task Can_Prune_Tokens()
		{
			// Arrange
			var tokenManager = _serviceProvider.GetRequiredService<IOpenIddictTokenManager>();

			var tokenDescriptor = new OpenIddictTokenDescriptor
			{
				Subject = "prune-test-user",
				Type = OpenIddictConstants.TokenTypeHints.AccessToken,
				Status = OpenIddictConstants.Statuses.Revoked,
				CreationDate = DateTimeOffset.UtcNow.AddDays(-30)
			};

			await tokenManager.CreateAsync(tokenDescriptor);

			// Act
			var count = await tokenManager.PruneAsync(DateTimeOffset.UtcNow.AddDays(-1));

			// Assert
			Assert.True(count > 0);
		}

		[Fact]
		public async Task NHibernate_Stores_Are_Registered()
		{
			// Arrange & Act
			var applicationStore = _serviceProvider.GetRequiredService<IOpenIddictApplicationStore<OpenIddictNHibernateApplication>>();
			var authorizationStore = _serviceProvider.GetRequiredService<IOpenIddictAuthorizationStore<OpenIddictNHibernateAuthorization>>();
			var scopeStore = _serviceProvider.GetRequiredService<IOpenIddictScopeStore<OpenIddictNHibernateScope>>();
			var tokenStore = _serviceProvider.GetRequiredService<IOpenIddictTokenStore<OpenIddictNHibernateToken>>();

			// Assert
			Assert.NotNull(applicationStore);
			Assert.NotNull(authorizationStore);
			Assert.NotNull(scopeStore);
			Assert.NotNull(tokenStore);
		}

		[Fact]
		public async Task Can_Handle_Custom_Key_Type()
		{
			// This test verifies that the ConvertIdentifierFromString/ToString methods work correctly
			// which is important for OpenIddict 7.0.0 compatibility
			
			var manager = _serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = "key-test-client",
				DisplayName = "Key Test"
			};

			var application = await manager.CreateAsync(descriptor);
			var id = await manager.GetIdAsync(application);

			// Verify we can retrieve by string ID
			var retrieved = await manager.FindByIdAsync(id!);

			Assert.NotNull(retrieved);
			Assert.Equal("key-test-client", await manager.GetClientIdAsync(retrieved));
		}

		public void Dispose()
		{
			_sessionFactory?.Dispose();
			_connection?.Close();
			_connection?.Dispose();
			(_serviceProvider as IDisposable)?.Dispose();
		}
	}

	// Extension method to convert IAsyncEnumerable to List
	public static class AsyncEnumerableExtensions
	{
		public static async Task<System.Collections.Generic.List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
		{
			var list = new System.Collections.Generic.List<T>();
			await foreach (var item in source)
			{
				list.Add(item);
			}
			return list;
		}
	}
}
