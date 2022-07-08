using Upack;

upack.OnUpackStatus = OnUpackStatus;
upack.OnUpdateCompleted = OnCompleted;
upack.OnErrorUpdate = OnError;
upack.CreateManifest("C:/Users/trevi/OneDrive/Documentos/Projetos/Alpha Xk/Assets/AssetBundle", "http://127.0.0.1/data/builds/assets", "1.0.0.0");
//await upack.UpdateFilesAsync(Environment.CurrentDirectory, "http://127.0.0.1/data/builds/Manifest.txt");
Console.ReadKey();

void OnError(){
    Console.WriteLine("Error!");
}

void OnCompleted(string _version){
    Console.WriteLine("Completed!");
}

void OnUpackStatus(string file, StatusFile status, int _min, int _max, string progress){
    Console.WriteLine(status + " " + file + " " + _min + "/" + _max + " " + progress);
}