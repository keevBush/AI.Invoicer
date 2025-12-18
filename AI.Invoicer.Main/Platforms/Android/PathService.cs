using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Main
{
    public partial class PathService
    {
        public partial string GetContentOfFolder()
        {
            RequestStorageAccess();
            string downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            if (!Directory.Exists(downloadsPath))
                throw new ArgumentException($"Path : {downloadsPath} not exists");
            
            return Path.Combine(downloadsPath, "phi3");
        }

        private void RequestStorageAccess()
        {
            // Vérifie si on est sur Android 11 (API 30) ou supérieur (donc inclut Android 16)
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
            {
                // Si on n'a pas déjà la permission "Gérer tous les fichiers"
                if (!Android.OS.Environment.IsExternalStorageManager)
                {
                    try
                    {
                        // On ouvre l'écran des paramètres spécifique à VOTRE application
                        var intent = new Android.Content.Intent(
                            Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);

                        // On lie l'intent au package de l'application
                        var uri = Android.Net.Uri.FromParts("package", Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.PackageName, null);
                        intent.SetData(uri);

                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StartActivity(intent);
                    }
                    catch
                    {
                        // Fallback : si l'intent spécifique échoue, ouvrir la liste générale
                        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);
                        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.StartActivity(intent);
                    }
                }
            }
        }
    }
}
