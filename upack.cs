namespace Upack{
    public enum StatusFile {Verifying, Downloading, Updated, Failed}
    public class upack{
        public static Action<string, StatusFile, int> OnUpackStatus;

        public static Action OnErrorUpdate;
        public static Action<string> OnUpdateCompleted;
        static HttpClient webClient = new HttpClient();
        static string version, urlAsset;
        static bool pass = true;
        static int progress;

        public static void CreateManifest(string PathFiles, string URLFolder, string Version = ""){
            if(!PathFiles.EndsWith("/"))
                PathFiles += "/";
            PathFiles.Replace("\\","/");

            string data = "UPACK" + Environment.NewLine;
            data += Version + Environment.NewLine;
            data += URLFolder + Environment.NewLine;

            if(!Directory.Exists(PathFiles))
                Directory.CreateDirectory(PathFiles);

            var files = Directory.GetFiles(PathFiles, "*.*", SearchOption.AllDirectories);
            data += files.Count() + Environment.NewLine;
            data += Environment.NewLine;
            foreach (string file in files) {
                data += file.Replace("\\","/") + Environment.NewLine;
            }
            data += Environment.NewLine;

            foreach (string file in files) {
                data += (new FileInfo(file).Length) + Environment.NewLine;
            }

            File.WriteAllText("Manifest.txt", data, System.Text.Encoding.UTF8);
        }

        public static async Task UpdateFilesAsync(string URL){
            try{
                var data = webClient.GetStringAsync(URL);
                string[] result = data.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if(result[0] == "UPACK"){
                    version = result[1];
                    urlAsset = result[2];
                    if(!urlAsset.EndsWith("/"))
                        urlAsset += "/";
                    int totalfiles = Convert.ToInt32(result[3]);
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

        static async Task CheckFilesAsync(string[] files, string[] sizes){
            pass = true;
            for(int i = 0; i < files.Length; i++){
                progress = (100 / files.Length) * (i + 1);
                OnUpackStatus?.Invoke(files[i], StatusFile.Verifying, progress);
                if(File.Exists(files[i])){
                    if(new FileInfo(files[i]).Length != Convert.ToInt32(sizes[i])){
                        await DownloadFileAsync(files[i]);
                    }
                }else{
                    await DownloadFileAsync(files[i]);
                }
            }
            progress = 100;
            if(!pass)
                OnErrorUpdate?.Invoke();
            else
                OnUpdateCompleted?.Invoke(version);
        }

        static async Task DownloadFileAsync(string filename){
            OnUpackStatus?.Invoke(filename, StatusFile.Downloading, progress);
            HttpResponseMessage response = await webClient.GetAsync(urlAsset + filename);
            Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
            if (response.IsSuccessStatusCode){
                try{
                    byte[] fileBytes = await webClient.GetByteArrayAsync(urlAsset + filename);
                    new FileInfo(filename).Directory.Create();
                    File.WriteAllBytes(filename, fileBytes);
                    OnUpackStatus?.Invoke(filename, StatusFile.Updated, progress);
                }catch{
                    pass = false;
                }
            }else{
                pass = false;
                OnUpackStatus?.Invoke(filename, StatusFile.Failed, progress);
            } 
        }
    }
}