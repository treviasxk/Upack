// Software desenvolvido por Trevias Xk
// Redes sociais:       treviasxk
// Github:              https://github.com/treviasxk

Upack.OnUpackStatus = OnUpackStatus;
Upack.OnUpdateCompleted = OnCompleted;
Upack.OnErrorUpdate = OnError;
await Upack.UpdateFilesAsync(Environment.CurrentDirectory, "http://192.168.0.101/data/builds/Manifest.txt");
//upack.CreateManifest("AssetBundles", "http://192.168.0.101/data/builds/android", "0.0.1.0 (alpha)");
Console.ReadKey();

void OnError(){
    Console.WriteLine("Error!");
}

void OnCompleted(string _version){
    Console.WriteLine("Completed!");
}

void OnUpackStatus(DownloadInfo _info){
    Console.WriteLine(_info.Status + " " + _info.filename + " " + _info.progress + "%" + " " + (_info.bytesReceived != null ? _info.bytesReceived + "/" + _info.totalBytes : ""));
}