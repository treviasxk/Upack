// Software desenvolvido por Trevias Xk
// Redes sociais:       treviasxk
// Github:              https://github.com/treviasxk

using System.Net;
using System.Security.Cryptography;
using System.Text;

public enum StatusFile {Verifying, Downloading, Updated, Failed, Deleting}
public class DownloadInfo{
    public string filename {get;set;}
    public StatusFile Status {get;set;}
    public string speedDownload {get;set;}
    public string totalBytes {get;set;}
    public string bytesReceived {get;set;}
    public int progress {get;set;}
}
public class Upack{
    public static Action<DownloadInfo> OnUpackStatus;
    public static Action OnUpdateCompleted;
    public static Action OnErrorUpdate;
    public static Action OnCleanCompleted;
    static Dictionary<string, string> dwfile = new Dictionary<string, string>();
    static Dictionary<string, string> dwlast = new Dictionary<string, string>();
    static HttpClient webClient = new HttpClient();
    static int progress, time, speed;
    
    static string pathFiles;

    public static void CreateManifest(string PathFiles, string UrlPath, string LocationSave = ""){
        string fullpath =  Path.GetFullPath(PathFiles) + "\\";
        PathFiles = PathFiles.Replace(Path.GetDirectoryName(fullpath) + "\\","");

        string data = "UPACK" + "\n";
        data += "UrlPath=" + UrlPath + "\n";

        var allFiles = Directory.GetFiles(fullpath, "*.*", SearchOption.AllDirectories);
        List<string> filesNoHiden = new List<string>();
        foreach (string file in allFiles) {
            if(!new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden))
                filesNoHiden.Add(file);
        }
        string[] files = filesNoHiden.ToArray();
        data += "TotalFiles=" + files.Count() + "\n";
        data += "\n";
        foreach (string file in files) {
            if(!new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden))
                data += Path.GetFullPath(file).Replace(fullpath,"").Replace("\\","/") + "\n";
        }
        data += "\n";

        foreach (string file in files) {
            if(!new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden))
                data += CalculateMD5(file) + "\n";
        }
        File.WriteAllText(LocationSave != "" ? LocationSave : "Manifest.upack", Convert.ToBase64String(Encoding.ASCII.GetBytes(data)), Encoding.ASCII);
    }

    public static void ClearFiles(string PATH){
        string fullpath =  Path.GetFullPath(PATH) + "\\";
        GC.Collect(); 
        GC.WaitForPendingFinalizers();
        if(Directory.Exists(PATH)){
            string[] files = Directory.GetFiles(fullpath, "*.*", SearchOption.AllDirectories);
            for(int i = 0; i < files.Length; i++) {
                progress = (int)((100 / (float)files.Length) * (i + 1));
                string file = files[i].Replace(fullpath,"");
                OnUpackStatus?.Invoke(new DownloadInfo {filename = file, Status = StatusFile.Deleting, progress = progress});
                try{
                    File.Delete(files[i]);
                }catch{
                    OnUpackStatus?.Invoke(new DownloadInfo {filename = file, Status = StatusFile.Failed, progress = progress});
                }
            }
            string[] paths = Directory.GetDirectories(fullpath, "*.*", SearchOption.AllDirectories);
            for(int i = 0; i < paths.Length; i++) {
                progress = (int)((100 / (float)paths.Length) * (i + 1));
                string file = paths[i].Replace(Path.GetFullPath(PATH),"");
                OnUpackStatus?.Invoke(new DownloadInfo {filename = file, Status = StatusFile.Deleting, progress = progress});
                try{
                    Directory.Delete(paths[i]);
                }catch{
                    OnUpackStatus?.Invoke(new DownloadInfo {filename = file, Status = StatusFile.Failed, progress = progress});
                }
            }
        }
        OnCleanCompleted?.Invoke();
    }

    static string MyUrlPath = "";
    public static async Task UpdateFilesAsync(string PATH, string URL){
        pathFiles = Path.GetFullPath(PATH).Replace("\\","/") + "/";
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        dwfile.Clear();
        dwlast.Clear();
        progress = 0;
        try{
            string data = await webClient.GetStringAsync(URL);
            string[] result = Encoding.ASCII.GetString(Convert.FromBase64String(data)).Split(new string[] {"\n"}, StringSplitOptions.None);
            if(result[0] == "UPACK"){
                MyUrlPath = result[1].Replace("UrlPath=","");

                if(!MyUrlPath.EndsWith("/"))
                    MyUrlPath += "/";
                
                int TotalFiles = Convert.ToInt32(result[2].Replace("TotalFiles=",""));
                string[] files = new string[TotalFiles];
                string[] md5 = new string[TotalFiles];

                files = result.Skip(4).ToArray().Take(TotalFiles).ToArray();
                md5 =  result.Skip(5 + TotalFiles).ToArray();
                await CheckFilesAsync(files,md5);
            }else{
                OnErrorUpdate?.Invoke();
            }
        }catch{
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
                }else{
                    OnUpackStatus?.Invoke(new DownloadInfo {filename = files[i], Status = StatusFile.Updated, progress = progress});
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
            string _url = MyUrlPath + dwfile.ElementAt(i).Key;
            string _location = pathFiles + dwfile.ElementAt(i).Key;
            new FileInfo(_location).Directory.Create();
            try{
                webClient = new HttpClient();
                var response = await webClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);
                var size = response.Content.Headers.ContentLength;
                response.EnsureSuccessStatusCode();
                var contentStream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[(int)size];
                int totalBytes = 0;

                GC.Collect(); 
                GC.WaitForPendingFinalizers();
                if(File.Exists(_location))
                    if(new FileInfo(_location).Length - 1 >= size)
                        File.Delete(_location);

                using(var fileStream = new FileStream(_location, FileMode.Create, FileAccess.ReadWrite, FileShare.None, buffer.Length, true)){
                    do{
                        float x = (100 / (float)dwfile.Count);
                        progress = (int)((x / (float)size) * totalBytes) + (int)((i + 1) * x);
                        if(time != DateTime.Now.Second){
                            time = DateTime.Now.Second;
                            speed = totalBytes - speed;
                        }
                        OnUpackStatus?.Invoke(new DownloadInfo {speedDownload = GetSizeShow(speed), filename = dwfile.ElementAt(i).Key, progress = progress, Status = StatusFile.Downloading, bytesReceived = GetSizeShow(totalBytes), totalBytes =  GetSizeShow((int)size)});
                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if(bytesRead == 0)
                            continue;
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytes += bytesRead;
                    }
                    while (totalBytes < size);
                }
                if(CalculateMD5(_location) == dwfile.ElementAt(i).Value){
                    OnUpackStatus?.Invoke(new DownloadInfo {filename = dwfile.ElementAt(i).Key, progress = progress, Status = StatusFile.Updated, bytesReceived = GetSizeShow(totalBytes), totalBytes =  GetSizeShow((int)size)});
                }else{
                    pass = false;
                    OnUpackStatus?.Invoke(new DownloadInfo {filename = dwfile.ElementAt(i).Key, Status = StatusFile.Failed});
                }
            }catch{
                pass = false;
                OnUpackStatus?.Invoke(new DownloadInfo {filename = dwfile.ElementAt(i).Key, Status = StatusFile.Failed});
            }
        }

        if(!pass)
            OnErrorUpdate?.Invoke();
        else
            OnUpdateCompleted?.Invoke();
    }

    static MD5 md5 = MD5.Create();
    static string CalculateMD5(string filename){
        var stream = File.OpenRead(filename);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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