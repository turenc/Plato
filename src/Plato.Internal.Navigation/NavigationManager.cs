﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Plato.Internal.Models.Shell;

namespace Plato.Internal.Navigation
{
    public class NavigationManager : INavigationManager
    {

        #region "Private Variables"

        private static readonly string[] Schemes = { "http", "https", "tel", "mailto" };

        #endregion

        #region "Constructor"

        private readonly IEnumerable<INavigationProvider> _navigationProviders;
        private readonly ILogger _logger;
        private readonly IShellSettings _shellSettings;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IAuthorizationService _authorizationService;

        private IUrlHelper _urlHelper;

        public NavigationManager(
            IEnumerable<INavigationProvider> navigationProviders,
            ILogger<NavigationManager> logger,
            IShellSettings shellSettings,
            IUrlHelperFactory urlHelperFactory,
            IAuthorizationService authorizationService)
        {
            _navigationProviders = navigationProviders;
            _logger = logger;
            _shellSettings = shellSettings;
            _urlHelperFactory = urlHelperFactory;
            _authorizationService = authorizationService;
        }

        #endregion

        #region "Implementation"

        public IEnumerable<MenuItem> BuildMenu(string name, ActionContext actionContext)
        {
            var builder = new NavigationBuilder();

            // Processes all navigation builders to create a flat list of menu items.
            // If a navigation builder fails, it is ignored.
            
            foreach (var navigationProvider in _navigationProviders)
            {
                try
                {
                    navigationProvider.BuildNavigation(name, builder);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An exception occurred while building the menu: {name}");
                }
            }

            var menuItems = builder.Build();

            // Merge all menu hierarchies into a single one
            Merge(menuItems);

            // Remove unauthorized menu items
            //menuItems = Authorize(menuItems, actionContext.HttpContext.User);

            // Compute Url and RouteValues properties to Href
            menuItems = ComputeHref(menuItems, actionContext);

            // Keep only menu items with an Href, or that have child items with an Href
            menuItems = Reduce(menuItems);

            // Recursive sort
            menuItems = RecursiveSort(menuItems);

            return menuItems;
        }

        #endregion

        #region "Private Methods"

        static void Merge(List<MenuItem> items)
        {
            
            // Use two cursors to find all similar captions. If the same caption is represented
            // by multiple menu item, try to merge it recursively.
            for (var i = 0; i < items.Count; i++)
            {
                var source = items[i];
                var merged = false;
                for (var x = items.Count - 1; x > i; x--)
                {
                    var cursor = items[x];

                    // A match is found, add all its items to the source
                    if (String.Equals(cursor.Text.Name, source.Text.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        merged = true;
                        foreach (var child in cursor.Items)
                        {
                            source.Items.Add(child);
                        }

                        items.RemoveAt(x);

                        // If the item to merge is more authoritative then use its values
                        if (cursor.Position != null && source.Position == null)
                        {
                            source.Culture = cursor.Culture;
                            source.Href = cursor.Href;
                            source.Id = cursor.Id;
                            source.LinkToFirstChild = cursor.LinkToFirstChild;
                            source.LocalNav = cursor.LocalNav;
                            source.Position = cursor.Position;
                            source.Order = cursor.Order;
                            source.Resource = cursor.Resource;
                            source.RouteValues = cursor.RouteValues;
                            source.Text = cursor.Text;
                            source.Url = cursor.Url;

                            //source.Permissions.Clear();
                            //source.Permissions.AddRange(cursor.Permissions);

                            source.Classes.Clear();
                            source.Classes.AddRange(cursor.Classes);
                        }
                    }
                }

                // If some items have been merged, apply recursively
                if (merged)
                {
                    Merge(source.Items);
                }
            }
        }
        
        List<MenuItem> ComputeHref(List<MenuItem> menuItems, ActionContext actionContext)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.Href = GetUrl(menuItem.Url, menuItem.RouteValues, actionContext);
                menuItem.Items = ComputeHref(menuItem.Items, actionContext);
            }

            return menuItems;
        }
        
        string GetUrl(string menuItemUrl, RouteValueDictionary routeValueDictionary, ActionContext actionContext)
        {
            string url;
            if (routeValueDictionary == null || routeValueDictionary.Count == 0)
            {
                if (String.IsNullOrEmpty(menuItemUrl))
                {
                    return "#";
                }
                else
                {
                    url = menuItemUrl;
                }
            }
            else
            {
                if (_urlHelper == null)
                {
                    _urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                }

                url = _urlHelper.RouteUrl(new UrlRouteContext { Values = routeValueDictionary });
            }

            if (!string.IsNullOrEmpty(url) &&
                actionContext?.HttpContext != null &&
                !(url.StartsWith("/") ||
                Schemes.Any(scheme => url.StartsWith(scheme + ":"))))
            {
                if (url.StartsWith("~/"))
                {
                    if (!String.IsNullOrEmpty(_shellSettings.RequestedUrlPrefix))
                    {
                        url = _shellSettings.RequestedUrlPrefix + "/" + url.Substring(2);
                    }
                    else
                    {
                        url = url.Substring(2);
                    }
                }

                if (!url.StartsWith("#"))
                {
                    var appPath = actionContext.HttpContext.Request.PathBase.ToString();
                    if (appPath == "/")
                        appPath = "";
                    url = appPath + "/" + url;
                }
            }

            return url;
        }
        
        List<MenuItem> Authorize(IEnumerable<MenuItem> items, ClaimsPrincipal user)
        {
            var filtered = new List<MenuItem>();

            foreach (var item in items)
            {
                // TODO: Attach actual user and remove this clause
                if (user == null)
                {
                    filtered.Add(item);
                }
                //else if (!item.Permissions.Any())
                //{
                //    filtered.Add(item);
                //}
                //else
                //{
                //    foreach (var permission in item.Permissions)
                //    {
                //        if (_authorizationService.AuthorizeAsync(user, permission, item.Resource).Result)
                //        {
                //            filtered.Add(item);
                //        }
                //    }
                //}

                // Process child items
                var oldItems = item.Items;

                item.Items = Authorize(item.Items, user).ToList();
            }

            return filtered;
        }

        List<MenuItem> Reduce(IEnumerable<MenuItem> items)
        {
            var filtered = items.ToList();

            foreach (var item in items)
            {
                if (!HasHrefOrChildHref(item))
                {
                    filtered.Remove(item);
                }

                item.Items = Reduce(item.Items);
            }

            return filtered;
        }

        List<MenuItem> RecursiveSort(List<MenuItem> items)
        {
            if (items != null)
            {
                items = items.OrderBy(o => o.Order).ToList();
                foreach (var item in items)
                {
                    if (item.Items.Count > 0)
                    {
                        item.Items = item.Items.OrderBy(o => o.Order).ToList();
                        RecursiveSort(item.Items);
                    }
                }
            }
            
            return items?.OrderBy(o => o.Order).ToList();

        }

        private static bool HasHrefOrChildHref(MenuItem item)
        {
            if (item.Href != "#")
            {
                return true;
            }

            return item.Items.Any(HasHrefOrChildHref);
        }

        #endregion

    }
}