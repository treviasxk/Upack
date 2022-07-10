// Software desenvolvido por Trevias Xk
// Redes sociais:       treviasxk
// Github:              https://github.com/treviasxk

using Upack;

upack.OnUpackStatus = OnUpackStatus;
upack.OnUpdateCompleted = OnCompleted;
upack.OnErrorUpdate = OnError;
//upack.CreateManifest("AssetBundles", "http://127.0.0.1/data/builds/android", "1.0.0.0");
await upack.UpdateFilesAsync(Environment.CurrentDirectory, "http://127.0.0.1/data/builds/Manifest.txt");
Console.ReadKey();

void OnError(){
    Console.WriteLine("Error!");
}

void OnCompleted(string _version){
    Console.WriteLine("Completed!");
}

void OnUpackStatus(DownloadInfo _info){
    Console.WriteLine(_info.Status + " " + _info.filename + " " + _info.progress + "% " + _info.bytesReceived + " " + _info.totalBytes);
}