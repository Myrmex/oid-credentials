# OpenIdDict Credentials Flow for WebAPI

AspNet Core 2.1 - OpenIdDict 2.0.0-rc3-0996

## References

- <https://github.com/openiddict>
- <https://github.com/openiddict/openiddict-samples/tree/dev/samples/PasswordFlow>: official sample
- <https://github.com/openiddict/openiddict-core/blob/dev/samples/>: up-to-date samples.
- <https://github.com/openiddict/openiddict-core/issues/593>: latest changes.

## Quick Test

Sample token request:

```
POST http://localhost:53736/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&scope=offline_access profile email roles&resource=http://localhost:4200&username=zeus&password=P4ssw0rd!
```

After getting the token in the response, make requests like:

```
GET http://localhost:53736/api/values
Content-Type: application/json
Authorization: Bearer ...
```

## Instructions

1.create a new WebAPI app without any authentication.

2.add the appropriate MyGet repositories to your NuGet sources. This can be done by adding a new `NuGet.Config` file at the root of your solution:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <add key="aspnet-contrib" value="https://www.myget.org/F/aspnet-contrib/api/v3/index.json" />
  </packageSources>
</configuration>
```

3.ensure that you have these packages in the project (you can list them using a NuGet command like `get-package | Format-Table -AutoSize` in the NuGet console):

```
install-package AspNet.Security.OAuth.Validation -pre
install-package OpenIddict -pre
install-package OpenIddict.EntityFrameworkCore -pre
install-package OpenIddict.Mvc -pre
install-package MailKit
install-package NLog -pre
install-package Swashbuckle.AspNetCore
```

MailKit can be used for mailing, Swashbuckle.AspNetCore for Swagger, NLog for file-based logging.

4.should you want to configure logging or other services, do it in `Program.cs`. Usually, the default configuration already does all what is typically required. See https://joonasw.net/view/aspnet-core-2-configuration-changes .

5.under `Models`, add identity models (`ApplicationUser`, `ApplicationDbContext`).

6.under `Services`, add `DatabaseInitializer`.

7.add your database connection string to `appsettings.json`. You will then override it using an environment variable source (or a production-targeted version of appsettings) for production. E.g.:

```json
  "Data": {
    "DefaultConnection": {
      "ConnectionString": "Server=(local)\\SqlExpress;Database=oid;Trusted_Connection=True;MultipleActiveResultSets=true;"
    }
  }
```

Alternatively, just use an in-memory database.

8. `Startup/ConfigureServices`: see code. Note: if deploying to Azure, ensure to CORS-enable your web app in the portal too.

9. in `Startup/Configure`, add OpenIddict and the OAuth2 token validation middleware in your ASP.NET Core pipeline by calling `app.UseOAuthValidation()` and `app.UseOpenIddict()` after `app.UseIdentity()` and before `app.UseMvc()`: see code. Also note that here we seed the database using the injected service (see nr.6 above).

10.under `Controllers`, add `AuthorizationController.cs`.

**Note**: to secure your API, add an `[Authorize]` or `[Authorize(Roles = "some roles here")]` attribute to your controller or controller's method. Note: *you should define the authentication scheme for this attribute, to avoid redirection to a login page* (and thus a 404 from your client): i.e. use `[Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]`. See <https://github.com/openiddict/openiddict-core/blob/dev/samples/Mvc.Server/Controllers/ResourceController.cs#L9>.

## MySql

To use MySql instead of SqlServer:

1.`install-package MySql.Data.EntityFrameworkCore`
2.in `Startup.cs`, method `ConfigureServices`, replace the SQL Server line with this:

```cs
options.UseMySQL(_configuration["Data:DefaultConnection:ConnectionString"]);
// options.UseSqlServer(_configuration["Data:DefaultConnection:ConnectionString"]);
```

3.in `appsettings.json` change the connection string from SQL Server to MySql: from

```json
  "Data": {
    "DefaultConnection": {
      "ConnectionString": "Server=(local)\\SqlExpress;Database=oid;Trusted_Connection=True;MultipleActiveResultSets=true;"
    }
  }
```

to:

```json
  "Data": {
    "DefaultConnection": {
      "ConnectionString": "Server=localhost;Database=oid;Uid=zeus;Pwd=mysql;SslMode=none"
    }
  }
```

Currently, this throws on startup, when initializing the database:

   System.NotImplementedException
     HResult=0x80004001
     Message=The 'MySQLNumberTypeMapping' does not support value conversions. Support for value conversions typically requires changes in the database provider.
     Source=Microsoft.EntityFrameworkCore.Relational
     StackTrace:
      at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMapping.Clone(ValueConverter converter)
      at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMappingSource.<FindMappingWithConversion>b__7_0(ValueTuple`3 k)
      at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
      at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMappingSource.FindMappingWithConversion(RelationalTypeMappingInfo& mappingInfo, IReadOnlyList`1 principals)
      at Microsoft.EntityFrameworkCore.Storage.Internal.FallbackRelationalTypeMappingSource.FindMappingWithConversion(RelationalTypeMappingInfo& mappingInfo, IReadOnlyList`1 principals)
      at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMappingSource.FindMapping(MemberInfo member)
      at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.PropertyDiscoveryConvention.IsCandidatePrimitiveProperty(PropertyInfo propertyInfo)
      at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.PropertyDiscoveryConvention.Apply(InternalEntityTypeBuilder entityTypeBuilder)
      at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ImmediateConventionScope.OnEntityTypeAdded(InternalEntityTypeBuilder entityTypeBuilder)
      at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.OnEntityTypeAdded(InternalEntityTypeBuilder entityTypeBuilder)
      at Microsoft.EntityFrameworkCore.Metadata.Internal.Model.AddEntityType(EntityType entityType)
      at Microsoft.EntityFrameworkCore.Metadata.Internal.Model.AddEntityType(Type type, ConfigurationSource configurationSource)
      at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalModelBuilder.Entity(TypeIdentity& type, ConfigurationSource configurationSource, Boolean throwOnQuery)
      at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalModelBuilder.Entity(Type type, ConfigurationSource configurationSource, Boolean throwOnQuery)
      at Microsoft.EntityFrameworkCore.ModelBuilder.Entity(Type type)
      at Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizer.FindSets(ModelBuilder modelBuilder, DbContext context)
      at Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelCustomizer.FindSets(ModelBuilder modelBuilder, DbContext context)
      at OpenIddict.EntityFrameworkCore.OpenIddictEntityFrameworkCoreCustomizer`5.Customize(ModelBuilder builder, DbContext context)
      at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
      at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.<>c__DisplayClass5_0.<GetModel>b__1()
      at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
      at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)
      at System.Lazy`1.CreateValue()
      at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
      at Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel()
      at Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
      at Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder.<>c.<TryAddCoreServices>b__7_1(IServiceProvider p)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitFactory(FactoryCallSite factoryCallSite, ServiceProviderEngineScope scope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(IServiceCallSite callSite, TArgument argument)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(IServiceCallSite callSite, TArgument argument)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(IServiceCallSite callSite, TArgument argument)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(IServiceCallSite callSite, TArgument argument)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.DynamicServiceProviderEngine.<>c__DisplayClass1_0.<RealizeService>b__0(ServiceProviderEngineScope scope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngine.GetService(Type serviceType, ServiceProviderEngineScope serviceProviderEngineScope)
      at Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngineScope.GetService(Type serviceType)
      at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
      at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
      at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
      at Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
      at Microsoft.EntityFrameworkCore.DbContext.Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<System.IServiceProvider>.get_Instance()
      at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<System.IServiceProvider>.get_Instance()
      at Microsoft.EntityFrameworkCore.Internal.InternalAccessorExtensions.GetService[TService](IInfrastructure`1 accessor)
      at Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService[TService](IInfrastructure`1 accessor)
      at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.get_DatabaseCreator()
      at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureCreatedAsync(CancellationToken cancellationToken)
      at OidCredentials.Services.DatabaseInitializer.<Seed>d__4.MoveNext() in C:\Projects\Core20\Test\OidCredentials\OidCredentials\Services\DatabaseInitializer.cs:line 25
      at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
      at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
      at OidCredentials.Startup.Configure(IApplicationBuilder app, IHostingEnvironment env, IDatabaseInitializer databaseInitializer) in C:\Projects\Core20\Test\OidCredentials\OidCredentials\Startup.cs:line 125

Apart from the fact that downgrading to .NET Core 2.0 is not an option in my real-world project, If I try I get this other exception:

    MySql.Data.MySqlClient.MySqlException
      HResult=0x80004005
      Message=Specified key was too long; max key length is 3072 bytes
      Source=MySql.Data

I found these relevant links:

- <https://github.com/aspnet/EntityFrameworkCore/issues/11078> (using a variation of the MySql driver, Pomelo, which is not my case);
- <https://bugs.mysql.com/bug.php?id=89855&thanks=sub>;
- <https://github.com/aspnet/EntityFrameworkCore/issues/11078> (explains that the bug for both Oracle and Pomelo providers come from using internal code).
