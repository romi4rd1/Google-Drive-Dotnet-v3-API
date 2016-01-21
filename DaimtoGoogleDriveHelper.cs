/*
This is my conversion to Google Apis Drive v3 from https://github.com/LindaLawton/Google-Dotnet-Samples/blob/master/Google-Drive/Google-Drive-Api-dotnet/Google-Drive-Api-dotnet/Program.cs
Oryginal author: Linda Lawton https://github.com/LindaLawton
This version Author: Marcel Garbarczyk https://github.com/m4rcelpl  
*/

using System;
using System.Collections.Generic;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace gDrive_Backup
{
    public class DaimtoGDrive
    {


        /// <summary>
        /// Download a file
        /// Documentation: https://developers.google.com/drive/v3/web/manage-downloads
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_fileResource">File resource of the file to download</param>
        /// <param name="_saveTo">location of where to save the file including the file name to save it as.</param>
        /// <returns></returns>

        public static Boolean downloadFile(DriveService _service, File _fileResource, string _saveTo)
        {
            var request = _service.Files.Get(_fileResource.Id);
            var stream = new System.IO.MemoryStream();

            request.Download(stream);
            System.IO.FileStream file = new System.IO.FileStream(_saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            try
            {
                stream.WriteTo(file);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                file.Close();
                stream.Close();
            }
            return true;
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        /// <summary>
        /// Uploads a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_uploadFile">path to the file to upload</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <returns>If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null</returns>
        public static File uploadFile(DriveService _service, string _uploadFile, string _parent, string Description)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = Description;
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<string> { _parent };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    var request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    //request.Convert = true;   // uncomment this line if you want files to be converted to Drive format
                    request.Upload();

                    return request.ResponseBody;

                }
                catch (Exception)
                {
                    return null;
                    throw;
                }
            }
            else {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }

        /// <summary>
        /// Updates a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/update
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_uploadFile">path to the file to upload</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <param name="_fileId">the resource id for the file we would like to update</param>                      
        /// <returns>If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null</returns>
        public static File updateFile(DriveService _service, string _uploadFile, string _parent, string _fileId, string Description)
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = Description;
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<string> { _parent };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = _service.Files.Update(body, _fileId, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception)
                {
                    return null;
                    throw;

                }
            }
            else
            {
                return null;
            }

        }


        /// <summary>
        /// Create a new Directory.
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_name">The title of the file. Used to identify file or folder name.</param>
        /// <param name="_description">A short description of the file.</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <returns></returns>
        public static File createDirectory(DriveService _service, string _name, string _description, string _parent)
        {
            File NewDirectory = null;

            // Create metaData for a new Directory
            File body = new File();
            body.Name = _name;
            body.Description = _description;
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<string> { _parent };
            try
            {
                var request = _service.Files.Create(body);
                NewDirectory = request.Execute();
            }
            catch (Exception)
            {
                throw;
            }

            return NewDirectory;
        }


        /// <summary>
        /// List all of the files and directories for the current user.  
        /// 
        /// Documentation: https://developers.google.com/drive/v3/reference/files/list
        /// Documentation Search: https://developers.google.com/drive/v3/web/search-parameters
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>        
        /// <param name="pageToken">if pageToken is null will return all files</param>
        /// <param name="Fields">by default you can get File ID and Name. If you want more you must specify it. List of it you can find here:
        /// https://developers.google.com/drive/v3/reference/files
        /// <param name="Spaces"> The list of spaces which contain the file. The currently supported values are 'drive', 'appDataFolder' and 'photos'. 
        /// <returns></returns>
        /// 

        public static IList<File> GetFiles(DriveService service, string filds, string search)
        {
            string pageToken = null;
            var Files = new List<File>();

            do
            {
                try
                {
                    var request = service.Files.List();
                    request.Q = search;
                    request.Spaces = "drive";
                    request.Fields = "nextPageToken, files(id, name, " + filds + ")";
                    request.PageToken = pageToken;
                    var result = request.Execute();

                    foreach (var file in result.Files)
                    {
                        Files.Add(file);
                    }
                    pageToken = result.NextPageToken;
                }
                catch (Exception)
                {
                    throw;
                }

            } while (pageToken != null);

            return Files;
        }

    }
}
