using Upack;

//upack.CreateManifest("data", "http://127.0.0.1/data/builds/assets", "1.0.0.0");
upack.OnUpackStatus = OnUpackStatus;
await upack.UpdateFilesAsync("http://127.0.0.1/data/builds/assets/Manifest.txt");
Console.ReadKey();



void OnUpackStatus(string file, StatusFile status, int progress){
    Console.WriteLine(status + " " + file + " " + progress + "%");
}