﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the X509CertificateAuthenticationProvider
    /// </summary>
    [TestClass]
    public class X509CertificateAuthenticationProviderTests
    {
        private static Uri graphResource = new Uri("https://graph.microsoft.com");
        private static string graphMeRequest = "https://graph.microsoft.com/v1.0/me";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestX509CertificateWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateConstructorNoDI()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
        public async Task TestX509CertificateConstructorNoDI_NullClientId_NullTenantId()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreLocation");
            var thumbprint = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:Thumbprint");

            var provider = new X509CertificateAuthenticationProvider(
                null,
                null,
                storeName,
                storeLocation,
                thumbprint);

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestX509CertificateConstructorNoDI_NullThumbprint()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreLocation");

            var provider = new X509CertificateAuthenticationProvider(
                AuthGlobals.DefaultClientId,
                AuthGlobals.OrganizationsTenantId,
                storeName,
                storeLocation,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.AuthenticateRequestAsync(graphResource, null);
        }

        [TestMethod]
        public async Task TestX509CertificateAuthenticateRequestAsyncCorrect()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, graphMeRequest);
            await provider.AuthenticateRequestAsync(graphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncFullNullScopes()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(graphResource, null);
        }

        [TestMethod]
        public async Task TestX509CertificateGetAccessTokenAsyncCorrect()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(graphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static X509CertificateAuthenticationProvider PrepareX509CertificateAuthenticationProvider()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:ClientId");
            var tenantId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:TenantId");
            var storeName = configuration.GetValue<StoreName>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:StoreLocation");
            var thumbprint = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteX509Certificate:X509Certificate:Thumbprint");

            var provider = new X509CertificateAuthenticationProvider(
                clientId,
                tenantId,
                storeName,
                storeLocation,
                thumbprint);
            return provider;
        }
    }
}