using System.Diagnostics;
using Microsoft.ML.OnnxRuntimeGenAI;

string GetDownloadedHubModel(string modelname) {
    var parts = modelname.Split(':');
    modelname = parts[0];
    var branchname = parts.Length > 1? parts[1] : "main";

    var modeldirname = $"models--{modelname.Replace("/", "--")}";
    var home = System.Environment.GetEnvironmentVariable("HOME");
    if (String.IsNullOrEmpty(home)) {
        throw new Exception("$HOME not set (or empty)");
    }
    var parentdir = Path.Join(home, ".cache/huggingface/hub");
    var modeldir = Path.Join(parentdir, modeldirname);
    if (!Path.Exists(modeldir)) {
        foreach (var maybedir in Directory.GetDirectories(parentdir)) {
            var name = Path.GetFileName(maybedir);
            if (string.Equals(modeldirname, name, StringComparison.OrdinalIgnoreCase)) {
                modeldir = maybedir;
                break;
            }
        }
    }
    if (!Path.Exists(modeldir)) {
        throw new ArgumentException($"No such model. Try `huggingface-cli download {modelname}`.");
    }

    var refpath = Path.Join(modeldir, $"refs/{branchname}");
    if (!Path.Exists(refpath)) {
        throw new ArgumentException($"Model {modelname} has no branch {branchname}");
    }
    var snapshot = File.ReadAllText(refpath);
    var snapdir = Path.Join(modeldir, $"snapshots/{snapshot}");
    if (!Path.Exists(snapdir)) {
        throw new Exception("Snapshot does not exist.");
    }
    return snapdir;
}

var modeltop = GetDownloadedHubModel("microsoft/Phi-3-vision-128k-instruct-onnx-cuda");
var modelsub = Path.Join(modeltop, "cuda-fp16"); // or cuda-int4-rtn-block-32

// var modeltop = GetDownloadedHubModel("microsoft/Phi-3-vision-128k-instruct-onnx-cpu");
// var modelsub = Path.Join(modeltop, "cpu-int4-rtn-block-32-acc-level-4");








var model = new Model(modelsub);
var processor = new MultiModalProcessor(model);
var tok = processor.CreateStream();

void RunOne(string sysPrompt, string userPrompt, string imgFilename) {
    
    var img = Images.Load(imgFilename);
    var fullPrompt = $"<|system|>{sysPrompt}<|end|><|user|><|image_1|>{userPrompt}<|end|><|assistant|>";
    var inputData = processor.ProcessImages(fullPrompt, img);

    var genParams = new GeneratorParams(model);
    genParams.SetSearchOption("max_length", 4096);
    genParams.SetInputs(inputData);

    Console.WriteLine("...running...");
    var sw = new Stopwatch();
    sw.Start();
    var gen = new Generator(model, genParams);
    while (!gen.IsDone()) {
        gen.ComputeLogits();
        gen.GenerateNextToken();
        var got = gen.GetSequence(0)[^1];
        var decoded = tok.Decode(got);
        Console.Write(decoded);
    }
    sw.Stop();
    Console.WriteLine($"\nDone generating. Took {sw.Elapsed}.");
}

RunOne("", "What is in the image?", "cat.jpg");
RunOne("", "What is in the image?", "cat.jpg");
foreach (var img in Directory.GetFiles(".", "bad-tv-*")) {
    Console.WriteLine($"Inspecting image <{img}>...");
    try {
        RunOne("", "What kind of TV is shown? Is it broken?",img);
    } catch (Exception e) {
        Console.WriteLine($"Could not process image <{img}>:" + e.Message);
    }
}