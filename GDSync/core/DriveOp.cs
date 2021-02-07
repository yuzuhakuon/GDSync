using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDFile = Google.Apis.Drive.v3.Data.File;

namespace GDSync.core
{
    public class DriveOp
    {
        public static DriveService GetServiceAccountCredential(string path)
        {
            ServiceAccountCredential sa;
            using (var stream =
                new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                // sa = ServiceAccountCredential.FromServiceAccountData(stream);
                sa = GoogleCredential.FromStream(stream)
                                     .CreateScoped(Scopes)
                                     .UnderlyingCredential as ServiceAccountCredential;
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = sa,
                ApplicationName = ApplicationName,
            });

            return service;
        }


        public static DriveService GetUserAccountCredential(string path)
        {
            UserCredential credential;
            using (var stream =
                new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = $"token/{Common.GetMD5HashFromFile(path)}";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                //Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });


            return service;
        }


        public static GDFile GetDriveFileInfo(DriveService service, string file_id)
        {
            FilesResource.GetRequest fileRequest = service.Files.Get(file_id);
            fileRequest.SupportsAllDrives = true;
            fileRequest.Fields = Var.FIELDS;

            GDFile file = fileRequest.Execute();
            return file;
        }


        public static GDFile Copy(DriveService service, string src_file_id, string dst_folder_id)
        {
            var body = new GDFile
            {
                Parents = new List<string>() { dst_folder_id }
            };

            FilesResource.CopyRequest copyRequest = service.Files.Copy(body, src_file_id);
            copyRequest.SupportsAllDrives = true;
            copyRequest.Fields = "parents, id, name, size";
            var result = copyRequest.Execute();

            return result;
        }


        public static string CreateFolder(DriveService service, DriveFileInfo src_folder, string dst_folder_id)
        {
            var body = new GDFile();
            body.Name = src_folder.Name;
            body.Kind = "drive#folder";
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<string>() { dst_folder_id };

            FilesResource.CreateRequest createRequest = service.Files.Create(body);
            createRequest.SupportsAllDrives = true;
            var result = createRequest.Execute();

            return result.Id;
        }


        public static GDFile CreateFolder(DriveService service, string name, string dst_folder_id)
        {
            var body = new GDFile();
            body.Name = name;
            body.Kind = "drive#folder";
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<string>() { dst_folder_id };

            FilesResource.CreateRequest createRequest = service.Files.Create(body);
            createRequest.SupportsAllDrives = true;
            var result = createRequest.Execute();

            return result;
        }


        public static List<GDFile> ListCurrentFiles(DriveService service, string src_folder_id)
        {
            var files = new List<GDFile>();

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Corpora = "allDrives";
            listRequest.IncludeItemsFromAllDrives = true;
            listRequest.Q = $"'{src_folder_id}' in parents  and trashed=false";
            listRequest.SupportsAllDrives = true;
            listRequest.Fields = $"nextPageToken, files({Var.FIELDS})";
            var results = listRequest.Execute();
            files.AddRange(results.Files);

            while (results.NextPageToken != null)
            {
                listRequest = service.Files.List();
                listRequest.Corpora = "allDrives";
                listRequest.IncludeItemsFromAllDrives = true;
                listRequest.Q = $"'{src_folder_id}' in parents  and trashed=false";
                listRequest.SupportsAllDrives = true;
                listRequest.Fields = $"nextPageToken, files({Var.FIELDS})";
                listRequest.PageToken = results.NextPageToken;
                results = listRequest.Execute();
                files.AddRange(results.Files);
            }

            Global.AddFolderList(src_folder_id, files);
            return files;
        }


        public static GDFile Rename(DriveService service, string src_folder_id, string name)
        {
            var body = new GDFile
            {
                Name = name
            };


            FilesResource.UpdateRequest updateRequest = service.Files.Update(body, src_folder_id);
            updateRequest.SupportsAllDrives = true;
            var result = updateRequest.Execute();
            return result;
        }


        public static GDFile Move(DriveService service, string src_file_id, string dst_folder_id, string parent)
        {
            var body = new GDFile { };


            FilesResource.UpdateRequest updateRequest = service.Files.Update(body, src_file_id);
            updateRequest.AddParents = dst_folder_id;
            updateRequest.RemoveParents = parent;
            updateRequest.SupportsAllDrives = true;
            var result = updateRequest.Execute();
            return result;
        }


        public static string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly, DriveService.Scope.DriveMetadata, DriveService.Scope.DriveMetadataReadonly, DriveService.Scope.DriveAppdata, DriveService.Scope.DrivePhotosReadonly };
        public static string ApplicationName = "Drive API .NET Quickstart";
    }
}
