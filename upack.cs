using System.Net;

namespace Upack{
    public enum StatusFile {Verifying, Downloading, Updated, Failed}
    public class upack{
        public static Action<string, StatusFile, int, int, string> OnUpackStatus;
        public static Action<string> OnUpdateCompleted;
        public static Action OnErrorUpdate;
        static HttpClient webClient = new HttpClient();
        
        static string version, urlAsset, pathFiles;
        public static void CreateManifest(string PathFiles, string URLFolder, string Version = ""){
            var fullpath =  Path.GetFullPath(PathFiles);
            PathFiles = Path.GetFullPath(PathFiles);
            PathFiles = PathFiles.Replace(Path.GetDirectoryName(PathFiles) + "\\","");

            string data = "UPACK" + Environment.NewLine;
            data += "version=" + Version + Environment.NewLine;
            data += "urlpath=" + URLFolder + Environment.NewLine;

            var files = Directory.GetFiles(fullpath, "*.*", SearchOption.AllDirectories);
            data += "total=" + files.Count() + Environment.NewLine;
            data += Environment.NewLine;
            foreach (string file in files) {
                var xx = Path.GetFullPath(file);
                data += PathFiles + xx.Replace(fullpath,"").Replace("\\","/") + Environment.NewLine;
            }
            data += Environment.NewLine;

            foreach (string file in files) {
                data += (new FileInfo(file).Length) + Environment.NewLine;
            }

            File.WriteAllText("Manifest.txt", data, System.Text.Encoding.UTF8);
        }

        public static async Task UpdateFilesAsync(string PATH, string URL){
            try{
                if(!PATH.EndsWith("/"))
                    PATH += "/";
                pathFiles = PATH.Replace("\\","/");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                dwfiles.Clear();
                dwsizes.Clear();

                var data = webClient.GetStringAsync(URL);
                string[] result = data.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if(result[0] == "UPACK"){
                    version = result[1].Replace("version=","");
                    urlAsset = result[2].Replace("urlpath=","");
                    if(!urlAsset.EndsWith("/"))
                        urlAsset += "/";
                    int totalfiles = Convert.ToInt32(result[3].Replace("total=",""));
                    string[] files = new string[totalfiles];
                    string[] sizes = new string[totalfiles];

                    files =  result.Skip(5).ToArray().Take(totalfiles).ToArray();
                    sizes =  result.Skip(6 + totalfiles).ToArray();
                    await CheckFilesAsync(files,sizes);
                }else{
                    OnErrorUpdate?.Invoke();
                }
            }catch{
                OnErrorUpdate?.Invoke();
            }
        }

        static List<string> dwfiles = new List<string>();
        static List<string> dwsizes = new List<string>();
        static async Task CheckFilesAsync(string[] files, string[] sizes){
            for(int i = 0; i < files.Length; i++){
                OnUpackStatus?.Invoke(files[i], StatusFile.Verifying, i, files.Length - 1, GetSizeShow(sizes[i]));
                if(File.Exists(pathFiles + files[i])){
                    if(new FileInfo(files[i]).Length != Convert.ToInt32(sizes[i])){
                        dwfiles.Add(files[i]);
                        dwsizes.Add(sizes[i]);
                    }
                }else{
                    dwfiles.Add(files[i]);
                    dwsizes.Add(sizes[i]);
                }
            }
            await DownloadFileAsync();
        }

        static async Task DownloadFileAsync(){
            bool pass = true;
            for(int i = 0; i < dwfiles.Count; i++){
                OnUpackStatus?.Invoke(dwfiles[i], StatusFile.Downloading, i, dwfiles.Count - 1, GetSizeShow(dwsizes[i]));
                HttpResponseMessage response = await webClient.GetAsync(urlAsset + dwfiles[i]);
                if (response.IsSuccessStatusCode){
                    try{
                        var _bytes = await webClient.GetByteArrayAsync(urlAsset + dwfiles[i]);
                        new FileInfo(pathFiles + dwfiles[i]).Directory.Create();
                        File.WriteAllBytes(pathFiles + dwfiles[i], _bytes);
                        OnUpackStatus?.Invoke(dwfiles[i], StatusFile.Updated, i, dwfiles.Count - 1, GetSizeShow(dwsizes[i]));
                    }catch{
                        pass = false;
                        OnUpackStatus?.Invoke(dwfiles[i], StatusFile.Failed, i, dwfiles.Count - 1, GetSizeShow(dwsizes[i]));
                    }
                }else{
                    pass = false;
                    OnUpackStatus?.Invoke(dwfiles[i], StatusFile.Failed, i, dwfiles.Count - 1, GetSizeShow(dwsizes[i]));
                }
            }

            if(!pass)
                OnErrorUpdate?.Invoke();
            else
                OnUpdateCompleted?.Invoke(version);
        }

        static string GetSizeShow(string _size){
            int PacketsReceived = Convert.ToInt32(_size);
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