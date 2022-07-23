// Software desenvolvido por Trevias Xk
// Redes sociais:       treviasxk
// Github:              https://github.com/treviasxk

Upack.OnUpackStatus = OnUpackStatus;
Upack.OnUpdateCompleted = OnCompleted;
Upack.OnErrorUpdate = OnError;
//Upack.ClearFiles(Environment.CurrentDirectory + "/AssetBundles");
await Upack.UpdateFilesAsync(Environment.CurrentDirectory + "/AssetBundles", "http://192.168.0.101/data/builds/WindowsEditor.upack");
Console.ReadKey();

/*
UpackManifest manifest = new UpackManifest();
manifest.Name = "Alpha Xk";
manifest.Version = "0.0.1.0 (alpha)";
manifest.BuildVersion = "0.0.1.0 (alpha)";
manifest.Status = UpackStatus.Online;
//manifest.Message = "We are experiencing some instability, please try again later.";
manifest.WebSite = "http://192.168.0.101";
manifest.UrlPath = "http://192.168.0.101/data/builds/android";
Upack.CreateManifest("AssetBundles", manifest);
Console.ReadKey();*/
void OnError(){
    Console.WriteLine("Error!");
}

void OnCompleted(UpackManifest _manifest){
    Console.WriteLine("Completed!");
}

void OnUpackStatus(DownloadInfo _info){
    Console.WriteLine(_info.Status + " " + _info.filename + " "+ _info.speedDownload+" " + _info.progress + "%" + " " + (_info.bytesReceived != null ? _info.bytesReceived + "/" + _info.totalBytes : ""));
}