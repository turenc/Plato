﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plato.Internal.Abstractions;
using Plato.Internal.Theming.Abstractions;
using Plato.Internal.Theming.Abstractions.Models;

namespace Plato.Internal.Theming
{
    public class DummySiteThemeManager : ISiteThemeManager
    {

        private readonly IThemeManager _themeManager;

        public DummySiteThemeManager(IThemeManager themeManager)
        {
            _themeManager = themeManager;
        }

        public string RootPath => _themeManager.RootPath;

        public IEnumerable<IThemeDescriptor> AvailableThemes => _themeManager.AvailableThemes;

    }

    public class DummySiteThemeFileManager : ISiteThemeFileManager
    {

        private readonly IThemeFileManager _themeFileManager;

        public DummySiteThemeFileManager(ISiteThemeManager siteThemeManager,
            IThemeFileManager themeFileManager)
        {
            _themeFileManager = themeFileManager;
            SetThemeManager(siteThemeManager);
        }

        public void SetThemeManager(IThemeManager manager)
        {
            _themeFileManager.SetThemeManager(manager);
        }

        public IEnumerable<IThemeFile> GetFiles(string themeId)
        {
            return _themeFileManager.GetFiles(themeId);
        }

        public IEnumerable<IThemeFile> GetFiles(string themeId, string relativePath)
        {
            return _themeFileManager.GetFiles(themeId, relativePath);
        }

        public IThemeFile GetFile(string themeId, string relativePath)
        {
            return _themeFileManager.GetFile(themeId, relativePath);
        }

        public IEnumerable<IThemeFile> GetParents(string themeId, string relativePath)
        {
            return _themeFileManager.GetParents(themeId, relativePath);
        }

        public async Task<string> ReadFileAsync(IThemeFile themeFile)
        {
            return await _themeFileManager.ReadFileAsync(themeFile);
        }

        public async Task SaveFileAsync(IThemeFile themeFile, string contents)
        {
            await _themeFileManager.SaveFileAsync(themeFile, contents);
        }

    }
}
