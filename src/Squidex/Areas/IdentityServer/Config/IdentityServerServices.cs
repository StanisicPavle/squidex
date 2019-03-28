﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Users;
using Squidex.Shared.Identity;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerServices
    {
        public static void AddMyIdentityServer(this IServiceCollection services)
        {
            X509Certificate2 certificate;

            var assembly = typeof(IdentityServerServices).Assembly;

            using (var certStream = assembly.GetManifestResourceStream("Squidex.Areas.IdentityServer.Config.Cert.IdentityCert.pfx"))
            {
                var certData = new byte[certStream.Length];

                certStream.Read(certData, 0, certData.Length);
                certificate = new X509Certificate2(certData, "password",
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
            }

            services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(s =>
            {
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = s.GetRequiredService<IXmlRepository>();
                });
            });

            services.AddDataProtection().SetApplicationName("Squidex");
            services.AddSingleton(GetApiResources());
            services.AddSingleton(GetIdentityResources());

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders();
            services.AddSingleton<IPasswordValidator<IdentityUser>,
                PwnedPasswordValidator>();
            services.AddSingleton<IUserClaimsPrincipalFactory<IdentityUser>,
                UserClaimsPrincipalFactoryWithEmail>();
            services.AddSingleton<IClientStore,
                LazyClientStore>();
            services.AddSingleton<IResourceStore,
                InMemoryResourcesStore>();

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = "/error/";
                })
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryApiResources(GetApiResources())
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddSigningCredential(certificate);
        }

        private static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource(Constants.ApiScope)
            {
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Role,
                    SquidexClaimTypes.Permissions
                }
            };
        }

        private static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
            yield return new IdentityResources.Email();
            yield return new IdentityResource(Constants.RoleScope,
                new[]
                {
                    JwtClaimTypes.Role
                });
            yield return new IdentityResource(Constants.PermissionsScope,
                new[]
                {
                    SquidexClaimTypes.Permissions
                });
            yield return new IdentityResource(Constants.ProfileScope,
                new[]
                {
                    SquidexClaimTypes.DisplayName,
                    SquidexClaimTypes.PictureUrl
                });
        }
    }
}
