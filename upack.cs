// Software desenvolvido por Trevias Xk
// Redes sociais:       treviasxk
// Github:              https://github.com/treviasxk

using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Upack{
    public enum StatusFile {Verifying, Downloading, Updated, Failed}
    public class DownloadInfo{
        public string filename {get;set;}
        public StatusFile Status {get;set;}
        public string speedDownload {get;set;}
        public string totalBytes {get;set;}
        public string bytesReceived {get;set;}
        public int progress {get;set;}
    }
    public class upack{
        public static Action<DownloadInfo> OnUpackStatus;
        public static Action<string> OnUpdateCompleted;
        public static Action OnErrorUpdate;
        static Dictionary<string, string> dwfile = new Dictionary<string, string>();
        static Dictionary<string, string> dwlast = new Dictionary<string, string>();
        static HttpClient webClient = new HttpClient();
        static int progress;
        
        static string version, urlAsset, pathFiles;
        public static void CreateManifest(string PathFiles, string URLFolder, string Version = ""){
            var fullpath =  Path.GetFullPath(PathFiles);
            PathFiles = Path.GetFullPath(PathFiles);
            PathFiles = PathFiles.Replace(Path.GetDirectoryName(PathFiles) + "\\","");

            string data = "UPACK" + "\n";
            data += "version=" + Version + "\n";
            data += "urlpath=" + URLFolder + "\n";

            var files = Directory.GetFiles(fullpath, "*.*", SearchOption.AllDirectories);
            data += "total=" + files.Count() + "\n";
            data += "\n";
            foreach (string file in files) {
                var xx = Path.GetFullPath(file);
                data += PathFiles + xx.Replace(fullpath,"").Replace("\\","/") + "\n";
            }
            data += "\n";

            foreach (string file in files) {
                data += CalculateMD5(file) + "\n";
            }

            File.WriteAllText("Manifest.txt", Convert.ToBase64String(Encoding.ASCII.GetBytes(data)), Encoding.ASCII);
        }

        public static async Task UpdateFilesAsync(string PATH, string URL){
            if(!PATH.EndsWith("/"))
                PATH += "/";
            pathFiles = PATH.Replace("\\","/");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            dwfile.Clear();
            dwlast.Clear();
            progress = 0;

            string data = await webClient.GetStringAsync(URL);
            string[] result = Encoding.ASCII.GetString(Convert.FromBase64String(data)).Split(new string[] {"\n"}, StringSplitOptions.None);

            if(result[0] == "UPACK"){
                version = result[1].Replace("version=","");
                urlAsset = result[2].Replace("urlpath=","");
                if(!urlAsset.EndsWith("/"))
                    urlAsset += "/";
                int totalfiles = Convert.ToInt32(result[3].Replace("total=",""));
                string[] files = new string[totalfiles];
                string[] md5 = new string[totalfiles];

                files =  result.Skip(5).ToArray().Take(totalfiles).ToArray();
                md5 =  result.Skip(6 + totalfiles).ToArray();
                await CheckFilesAsync(files,md5);
            }else{
                OnErrorUpdate?.Invoke();
            }
        }

        static async Task CheckFilesAsync(string[] files, string[] md5){
            for(int i = 0; i < files.Length; i++){
                progress = (int)((100 / (float)files.Length) * (i + 1));
                OnUpackStatus?.Invoke(new DownloadInfo {filename = files[i], Status = StatusFile.Verifying, progress = progress});
                if(File.Exists(pathFiles + files[i])){
                    if(CalculateMD5(pathFiles + files[i]) != md5[i]){
                        dwfile.Add(files[i], md5[i]);
                    }
                }else{
                    dwfile.Add(files[i], md5[i]);
                }
            }
            await DownloadFileAsync();
        }

        static async Task DownloadFileAsync(){
            bool pass = true;
            for(int i = 0; i < dwfile.Count; i++){
                string _url = urlAsset + dwfile.ElementAt(i).Key;
                string _location = pathFiles + dwfile.ElementAt(i).Key;
                new FileInfo(_location).Directory.Create();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();

                var response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);
                int size = (int)response.Content.Headers.ContentLength;
                Console.WriteLine(size);
                response.EnsureSuccessStatusCode();

                var contentStream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[8192];
                int totalBytes = 0;
                using (var fileStream = new FileStream(_location, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true)){
                    do{
                        float x = (100 / (float)dwfile.Count);
                        progress = (int)((x / (float)size) * totalBytes) + (int)(i * x);
                        OnUpackStatus?.Invoke(new DownloadInfo {filename = dwfile.ElementAt(i).Key, progress = progress, Status = StatusFile.Downloading, bytesReceived = GetSizeShow(totalBytes), totalBytes =  GetSizeShow(size)});
                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if(bytesRead == 0)
                            continue;
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytes += bytesRead;
                        Console.WriteLine(totalBytes);
                    }
                    while (totalBytes < size);
                }
                OnUpackStatus?.Invoke(new DownloadInfo {filename = dwfile.ElementAt(i).Key, progress = 100, Status = StatusFile.Updated, bytesReceived = GetSizeShow(totalBytes), totalBytes =  GetSizeShow(size)});
            }
            if(!pass)
                OnErrorUpdate?.Invoke();
            else
                OnUpdateCompleted?.Invoke(version);
        }

        static string CalculateMD5(string filename){
            using (var md5 = MD5.Create()){
                using (var stream = File.OpenRead(filename)){
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        static string GetSizeShow(int PacketsReceived){
            if(PacketsReceived > 1024000000)
            return (PacketsReceived / 1024000000).ToString("0") + "GB";
            if(PacketsReceived > 1024000)
            return (PacketsReceived / 1024000).ToString("0") + "MB";
            if(PacketsReceived > 1024)
            return (PacketsReceived / 1024).ToString("0") + "KB";
            if(PacketsReceived < 1024)
            return (PacketsReceived).ToString("0") + "Bytes";
            return "";
        }
    }
}