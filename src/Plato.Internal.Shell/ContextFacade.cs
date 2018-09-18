﻿namespace Plato.Internal.Shell
{
    //public class ContextFacade : IContextFacade
    //{

    //    public const string DefaultCulture = "en-US";

    //    private readonly IHttpContextAccessor _httpContextAccessor;
    //    private readonly IActionContextAccessor _actionContextAccessor;
    //    private readonly IShellDescriptorManager _shellDescriptorManager;
    //    private readonly IPlatoUserStore<User> _platoUserStore;
    //    private readonly ISiteSettingsStore _siteSettingsStore;
    //    private readonly IUrlHelperFactory _urlHelperFactory;
    //    private readonly IFeatureFacade _featureFacade; 

    //    private IUrlHelper _urlHelper;

    //    public ContextFacade(
    //        IHttpContextAccessor httpContextAccessor,
    //        IPlatoUserStore<User> platoUserStore,
    //        IShellDescriptorManager shellDescriptorManager,
    //        IActionContextAccessor actionContextAccessor,
    //        ISiteSettingsStore siteSettingsStore,
    //        IUrlHelperFactory urlHelperFactory, IFeatureFacade featureFacade)
    //    {
    //        _httpContextAccessor = httpContextAccessor;
    //        _platoUserStore = platoUserStore;
    //        _shellDescriptorManager = shellDescriptorManager;
    //        _actionContextAccessor = actionContextAccessor;
    //        _siteSettingsStore = siteSettingsStore;
    //        _urlHelperFactory = urlHelperFactory;
    //        _featureFacade = featureFacade;
    //    }

    //    public async Task<User> GetAuthenticatedUserAsync()
    //    {
    //        var user = _httpContextAccessor.HttpContext.User;
    //        var identity = user?.Identity;
    //        if ((identity != null) && (identity.IsAuthenticated))
    //        {
    //            return await _platoUserStore.GetByUserNameAsync(identity.Name);
    //        }

    //        return null;
    //    }


    //    public async Task<IShellModule> GetFeatureByModuleIdAsync(string moduleId)
    //    {
    //        return await _featureFacade.GetModuleByIdAsync(moduleId) ??
    //               throw new Exception($"No feature has been enabled with the ModuleId of'{moduleId}'.");
    //    }
        
    //    public async Task<ISiteSettings> GetSiteSettingsAsync()
    //    {
    //        return await _siteSettingsStore.GetAsync();
    //    }

    //    public async Task<string> GetBaseUrlAsync()
    //    {

    //        var settings = await GetSiteSettingsAsync();
    //        if (!String.IsNullOrWhiteSpace(settings.BaseUrl))
    //        {
    //            // trim tailing forward slash
    //            var lastSlash = settings.BaseUrl.LastIndexOf('/');
    //            return (lastSlash > -1)
    //                ? settings.BaseUrl.Substring(0, lastSlash)
    //                : settings.BaseUrl;
    //        }

    //        var request = _httpContextAccessor.HttpContext.Request;
    //        return $"{request.Scheme}://{request.Host}{request.PathBase}";

    //    }

    //    public string GetRouteUrl(RouteValueDictionary routeValues)
    //    {
    //        if (_urlHelper == null)
    //        {
    //            _urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
    //        }

    //        return _urlHelper.RouteUrl(new UrlRouteContext {Values = routeValues});
    //    }

    //    public async Task<string> GetCurrentCultureAsync()
    //    {

    //        // Get users culture
    //        var user = await GetAuthenticatedUserAsync();
    //        if (user != null)
    //        {
    //            if (!String.IsNullOrEmpty(user.Culture))
    //            {
    //                return user.Culture;
    //            }
                
    //        }

    //        // Get application culture
    //        var settings = await GetSiteSettingsAsync();
    //        if (settings != null)
    //        {
    //            if (!String.IsNullOrEmpty(settings.Culture))
    //            {
    //                return settings.Culture;
    //            }
    //        }
       
    //        // Return en-US default culture
    //        return DefaultCulture;

    //    }

    //    public string GetCurrentCulture()
    //    {
    //        return GetCurrentCultureAsync()
    //            .GetAwaiter()
    //            .GetResult();
    //    }

    //}

}
