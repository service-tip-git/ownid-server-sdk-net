using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class IsFido2UserExistsCommand
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ICacheItemService _cacheItemService;
        private readonly ILocalizationService _localizationService;

        public IsFido2UserExistsCommand(IUserHandlerAdapter userHandlerAdapter, ICacheItemService cacheItemService,
            ILocalizationService localizationService)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
            _localizationService = localizationService;
        }

        public async Task<bool> ExecuteAsync(CommandInput<string> input)
        {
            if (!await _userHandlerAdapter.IsFido2UserExists(input.Data))
                return false;

            var localizedError = _localizationService.GetLocalizedString("Error_PhoneAlreadyConnected");
            await _cacheItemService.FinishFlowWithErrorAsync(input.Context, localizedError);

            return true;
        }
    }
}