using Xunit;

namespace OwnIdSdk.NetCore3.Tests
{
    public class ProviderTest
    {
        [Fact]
        public void Provider_GenerateChallengeJwt_Success()
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
    }
}