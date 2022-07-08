using Upack;

//upack.CreateManifest(Environment.CurrentDirectory + "/data", "http://127.0.0.1/data/builds/assets", "1.0.0.0");
upack.OnUpackStatus = OnUpackStatus;
await upack.UpdateFilesAsync(Environment.CurrentDirectory, "http://127.0.0.1/data/builds/Manifest.txt");
Console.ReadKey();


void OnUpackStatus(string file, StatusFile status, int _min, int _max, string progress){
    Console.WriteLine(status + " " + file + " " + _min + "/" + _max + " " + progress);
}