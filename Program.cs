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
Upack.CreateManifest("AssetBundles", "http://192.168.0.101/data/builds/android");
Console.ReadKey();*/
void OnError(){
    Console.WriteLine("Error!");
}

void OnCompleted(){
    Console.WriteLine("Completed!");
}

void OnUpackStatus(DownloadInfo _info){
    Console.WriteLine(_info.Status + " " + _info.filename + " "+ _info.speedDownload+" " + _info.progress + "%" + " " + (_info.bytesReceived != null ? _info.bytesReceived + "/" + _info.totalBytes : ""));
}