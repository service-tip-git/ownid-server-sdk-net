using System;
using System.Threading.Tasks;
using System.Web;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.MagicLink;
using OwnID.Extensibility.Providers;
using OwnID.Extensibility.Services;
using OwnID.Extensions;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands.MagicLink
{
    public class SendMagicLinkCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IEmailService _emailService;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IMagicLinkConfiguration _magicLinkConfiguration;
        private readonly TimeSpan _tokenExpiration;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public SendMagicLinkCommand(ICacheItemRepository cacheItemRepository, IUserHandlerAdapter userHandlerAdapter,
            IIdentitiesProvider identitiesProvider, IEmailService emailService,
            IMagicLinkConfiguration magicLinkConfiguration)
        {
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
            _identitiesProvider = identitiesProvider;
            _emailService = emailService;
            _magicLinkConfiguration = magicLinkConfiguration;
            _tokenExpiration = TimeSpan.FromMilliseconds(magicLinkConfiguration.TokenLifetime);
        }

        public async Task<MagicLinkResponse> ExecuteAsync(string email)
        {
            var did = await _userHandlerAdapter.GetUserIdByEmail(email);

            if (string.IsNullOrEmpty(did))
                throw new CommandValidationException($"No user was found with email '{email}'");

            var result = new MagicLinkResponse();
            var context = _identitiesProvider.GenerateContext();
            var token = _identitiesProvider.GenerateMagicLinkToken();

            await _cacheItemRepository.CreateAsync(new CacheItem
            {
                ChallengeType = ChallengeType.Login,
                Context = context,
                Payload = token,
                DID = did,
                Status = CacheItemStatus.Finished
            }, _tokenExpiration);

            if (_magicLinkConfiguration.SameBrowserUsageOnly)
            {
                result.CheckTokenKey = $"ownid-mlc-{context}";
                result.CheckTokenValue = GetCheckToken(context, token, did);
                result.CheckTokenLifetime = _magicLinkConfiguration.TokenLifetime;
            }

            var link = new UriBuilder(_magicLinkConfiguration.RedirectUrl);
            var query = HttpUtility.ParseQueryString(link.Query);
            query["ownid-mtkn"] = token;
            query["ownid-ctxt"] = context;
            link.Query = query.ToString() ?? string.Empty;

            await _emailService.SendAsync(email, "OwnID - Magic Link",
                $"Here is your <a href=\"{link.Uri}\">magic link</a> to login. <br>Cmon click me!", true);

            return result;
        }

        public static string GetCheckToken(string context, string token, string did)
        {
            return $"{context}::{token}::{did}".GetSha256();
        }
    }
}