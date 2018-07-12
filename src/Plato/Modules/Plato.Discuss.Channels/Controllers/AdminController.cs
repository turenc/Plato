﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Plato.Categories.Models;
using Plato.Categories.Stores;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Stores.Abstractions.Settings;

namespace Plato.Discuss.Channels.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContextFacade _contextFacade;
        private readonly ISiteSettingsStore _settingsStore;
        private readonly ICategoryStore<Category> _categoryStore;

        public AdminController(
            ISiteSettingsStore settingsStore,
            IContextFacade contextFacade,
            ICategoryStore<Category> categoryStore)
        {
            _settingsStore = settingsStore;
            _contextFacade = contextFacade;
            _categoryStore = categoryStore;
        }
        
        public async Task<IActionResult> Index()
        {

            string path = Request.Path;
            ViewData["path"] = path;

            var feature = await _contextFacade.GetCurrentFeatureAsync();

            ViewBag.Feature = feature;

            var category = new Category()
            {
                FeatureId = feature.Id,
                Name = "test"
            };

            await _categoryStore.CreateAsync(category);
          

            return View();
        }

        

    }

}