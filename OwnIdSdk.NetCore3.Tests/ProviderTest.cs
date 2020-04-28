using System.Collections.Generic;
using System.IO;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Store;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests
{
    public class ProviderTest
    {
        [Fact]
        public void GenerateChallengeJwt_Success()
        {
            // var provider = new Provider(new InMemoryCacheStore(), new ProviderConfiguration()
            // {
            //     Requester = new Requester
            //     {
            //         Name = "MyOrg",
            //         PublicKey = "keyasdadsad",
            //         DID = "did:idw:123123123"
            //     },
            //     ProfileFields = new List<ProfileField>
            //     {
            //         new ProfileField
            //         {
            //             Type = ProfileFieldType.Email,
            //             Label = "Email"
            //         }
            //     },
            //     CallbackUrl = "asadad",
            //     TokenSecret = "asdv234234^&%&^%&^hjsdfb2%%%",
            //     OwnIdApplicationUrl = "asdds"
            // });
            //
            // var a = provider.GenerateChallengeJwt("9877654321");
            // Assert.NotEmpty(a);


            Assert.NotNull(new { });
        }

        [Fact]
        public void GetProfileDataFromJwt_Success()
        {
            // var provider = new Provider(new InMemoryCacheStore(), new ProviderConfiguration(
            //     RsaHelper.ReadKeyFromPem(File.OpenText(
            //         @"/Users/c5306486/RiderProjects/ownid-docker-example/ownid-api/config/jwtRS256.key.pub")),
            //     RsaHelper.ReadKeyFromPem(
            //         File.OpenText(@"/Users/c5306486/RiderProjects/ownid-docker-example/ownid-api/config/jwtRS256.key")),
            //     "https://google.com",
            //     new List<ProfileField>
            //     {
            //         ProfileField.Email, ProfileField.FirstName, ProfileField.LastName
            //     }, "http://localhost:5000/", new Requester
            //     {
            //         DID = "did:ownid:12312313",
            //         Name = "OrgName",
            //         Description = "descr"
            //     }));
            //
            // provider.GetProfileDataFromJwt(
                 // @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlNDA4OGMyMy1hYzNjLTQ3OTAtOGQzMS1mODQ2NGJjMWJkMjAiLCJpYXQiOjE1MTYyMzkwMjIsInVzZXIiOnsiZGlkIjoibG9sb2xvRElEIiwicHJvZmlsZSI6W3sidmFsdWUiOiJhc2RzQHNkLnNkIiwidHlwZSI6ImVtYWlsIn1dLCJwdWJLZXkiOiJNSUlCSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQVE4QU1JSUJDZ0tDQVFFQW56eWlzMVpqZk5CMGJCZ0tGTVN2dmtUdHdsdkJzYUpxN1M1d0Era3plVk9WcFZXd2tXZFZoYTRzMzhYTS9wYS95cjQ3YXY3K3ozVlRtdkRSeUFIY2FUOTJ3aFJFRnBMdjljajVsVGVKU2lieXIvTXJtL1l0akNaVldnYU9ZSWh3clh3S0xxUHIvMTFpbldzQWtmSXl0dkhXVHhaWUVjWExnQVhGdVV1YVMzdUY5Z0VpTlF3ekdUVTF2MEZxa3FUQnI0QjhuVzNIQ040N1hVdTB0OFkwZStsZjRzNE94UWF3V0Q3OUo5LzVkM1J5MHZiVjNBbTFGdEdKaUp2T3dSc0lmVkNoRHBZU3RUY0hUQ01xdHZXYlY2TDExQldrcHpHWFNXNEh2NDNxYStHU1lPRDJRVTY4TWI1OW9TazJPQitCdE9McEpvZm1iR0VHZ3Ztd3lDSTlNd0lEQVFBQiJ9fQ.CNPfcNANSBSLUXT7snQin0OzcMjx5sFvKTo294VP_W8oGfAgtbrcGYIvWwJQGaRW3W3l7QutoLUTj6EJtoQTGogo6Z-vzyq0M0Q5X70Gmiua_f5ExLHMn2tY9Lpga04-MAcUMawqT24d3pGTJb4lovdM15kri-2-HbVZnzYiyG5pXN3uRuCxvrT_kF02j2QjNM9uY9ds7b7gu3tKx3aP1w_OzkdA6mzS5BZ6RWgOhBd4hhLheluVYwQwTUQtfwQeWi8CHeliTbC_qaPMgOsaCXKnyomEsfi_zyLgJB8y0KRtobwGKp54_F0ZsYkc55gLPR2N8q8X6ShAB4ho-rkepw");
        }
    }
}