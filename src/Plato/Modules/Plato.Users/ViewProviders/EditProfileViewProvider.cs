﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.FileSystem.Abstractions;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Shell;
using Plato.Internal.Models.Users;
using Plato.Internal.Stores.Abstractions.Files;
using Plato.Internal.Stores.Abstractions.Users;
using Plato.Users.Models;
using Plato.Users.Services;
using Plato.Users.ViewModels;

namespace Plato.Users.ViewProviders
{

    public class EditProfileViewProvider : BaseViewProvider<EditProfileViewModel>
    {

        private static string _pathToAvatarFolder;
        private static string _urlToAvatarFolder;
        
        private readonly IPlatoUserStore<User> _platoUserStore;
        private readonly IUserPhotoStore<UserPhoto> _userPhotoStore;
        private readonly ISitesFolder _sitesFolder;
        private readonly IPlatoUserManager<User> _platoUserManager;

        public EditProfileViewProvider(
            IShellSettings shellSettings,
            IPlatoUserStore<User> platoUserStore,
            IHostingEnvironment hostEnvironment,
            IFileStore fileStore,
            IUserPhotoStore<UserPhoto> userPhotoStore,
            ISitesFolder sitesFolder,
            IPlatoUserManager<User> platoUserManager)
        {
            _platoUserStore = platoUserStore;
            _userPhotoStore = userPhotoStore;
            _sitesFolder = sitesFolder;
            _platoUserManager = platoUserManager;

            // paths
            _pathToAvatarFolder = fileStore.Combine(hostEnvironment.ContentRootPath, shellSettings.Location, "images");
            _urlToAvatarFolder = $"/sites/{shellSettings.Location}/images/";
            
        }

        #region "Implementation"

        public override Task<IViewProviderResult> BuildIndexAsync(EditProfileViewModel viewModel, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildDisplayAsync(EditProfileViewModel viewModel, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override async Task<IViewProviderResult> BuildEditAsync(EditProfileViewModel viewModel, IViewProviderContext context)
        {

            var user = await _platoUserStore.GetByIdAsync(viewModel.Id);
            if (user == null)
            {
                return await BuildIndexAsync(viewModel, context);
            }
            
            return Views(
                View<User>("Home.Edit.Header", model => user).Zone("header"),
                View<User>("Home.Edit.Sidebar", model => user).Zone("sidebar"),
                View<User>("Home.Edit.Tools", model => user).Zone("tools"),
                View<EditProfileViewModel>("Home.EditProfile.Content", model => viewModel).Zone("content"),
                View<User>("Home.Edit.Footer", model => user).Zone("footer")
            );

        }

        public override async Task<bool> ValidateModelAsync(EditProfileViewModel viewModel, IUpdateModel updater)
        {
            return await updater.TryUpdateModelAsync(new EditProfileViewModel()
            {
                DisplayName = viewModel.DisplayName,
                Location = viewModel.Location,
                Biography = viewModel.Biography,
                Url = viewModel.Url
            });
        }

        public override async Task<IViewProviderResult> BuildUpdateAsync(EditProfileViewModel userProfile, IViewProviderContext context)
        {
            var user = await _platoUserStore.GetByIdAsync(userProfile.Id);
            if (user == null)
            {
                return await BuildIndexAsync(userProfile, context);
            }

            var model = new EditProfileViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model))
            {
                return await BuildEditAsync(userProfile, context);
            }
            
            if (context.Updater.ModelState.IsValid)
            {

                // Update user 
                user.DisplayName = model.DisplayName.Trim();
                user.Biography = model.Biography;
                user.Location = model.Location;
                user.Url = model.Url;

                // Example of how to store on custom user data object
                //var data = user.GetOrCreate<UserDetail>();
                //data.Profile.Location = model.Location;
                //data.Profile.Bio = model.Bio;
                //data.Profile.Url = model.Url;
                //user.AddOrUpdate<UserDetail>(data);

                // Update user avatar

                if (model.AvatarFile != null)
                {
                   user.PhotoUrl = await UpdateUserPhoto(user, model.AvatarFile);
                }

                // Update user
                var result = await _platoUserManager.UpdateAsync(user);
                foreach (var error in result.Errors)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            return await BuildEditAsync(userProfile, context);

        }

        #endregion

        #region "Private Methods"

        async Task<string> UpdateUserPhoto(User user, IFormFile file)
        {

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var stream = file.OpenReadStream();
            byte[] bytes = null;
            if (stream != null)
            {
                bytes = stream.StreamToByteArray();
            }

            // Ensure we have a valid byte array
            if (bytes == null)
            {
                return string.Empty;
            }
            
            // Get any existing photo
            var existingPhoto = await _userPhotoStore.GetByUserIdAsync(user.Id);

            // Upload the new file
            var fileName = await _sitesFolder.SaveUniqueFileAsync(stream, file.FileName, _pathToAvatarFolder);
           
            // Ensure the new file was created
            if (!string.IsNullOrEmpty(fileName))
            {
                // Delete any existing file
                if (existingPhoto != null)
                {
                    _sitesFolder.DeleteFile(existingPhoto.Name, _pathToAvatarFolder);
                }
            }

            // Insert or update photo entry
            var id = existingPhoto?.Id ?? 0;
            var userPhoto = new UserPhoto
            {
                Id = id,
                UserId = user.Id,
                Name = fileName,
                ContentType = file.ContentType,
                ContentLength = file.Length,
                ContentBlob = bytes,
                CreatedUserId = user.Id,
                CreatedDate = DateTime.UtcNow
            };
            
            var newOrUpdatedPhoto = id > 0
                ? await _userPhotoStore.UpdateAsync(userPhoto)
                : await _userPhotoStore.CreateAsync(userPhoto);
            if (newOrUpdatedPhoto != null)
            {
                return _urlToAvatarFolder + newOrUpdatedPhoto.Name;
            }
            
            return string.Empty;

        }

        #endregion

    }

}
