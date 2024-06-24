# Model

Retrieved with `huggingface-cli`.

It's under `${HOME}/.cache/huggingface/hub` and looks like a Git repo.

```
~/.cache/huggingface/hub/models--microsoft--Phi-3-vision-128k-instruct-onnx-cuda/refs
~/.cache/huggingface/hub/models--microsoft--Phi-3-vision-128k-instruct-onnx-cuda/snapshots/77717364052f6b70ae645b65ca6efd0da486d371/cuda-fp16/
```

# Packages

```
dotnet add package Microsoft.ML.OnnxRuntime
dotnet add package Microsoft.ML.OnnxRuntimeGenAI
dotnet add package Microsoft.ML.OnnxRuntimeGenAI.Cuda


#dotnet add package Microsoft.ML.OnnxRuntime.Gpu # Maybe this will make it work?
```

## onnxruntime-genai

https://github.com/microsoft/onnxruntime-genai/releases/tag/v0.3.0

Download and extract the Linux Cuda tarball from the release. 

# Running

Needs `libcublaslt11 libcublas11 libcufft10 libcudart11.0` packages from apt.
And `nvidia-cudnn`, which either is or depends on a "download and install external package" package with an Nvidia license agreement.

(Idenfitifed by `apt-file search $FILENAME` on the library in the error message, over multiple rounds to get everything.)

Needs libs copied to output (in the `.csproj` file). (I downloaded the release tarball for linuux-cuda; that's what the `.csproj` additions are copying from.)

Needs `LD_PRELOAD` in order to find one of the libs even despite it being copied: 
```
LD_PRELOAD=bin/Debug/net8.0/libonnxruntime.so.1.18.0 dotnet run
```
This can be identified by running under `strace -o trc.out -ff` and grepping for the library name that the error message says it can't find. Apparently it tried to (re-)load it after forking, and the subprocesses don't include the path that I'm copying it into? (Maybe I'm working from bad info about *where* to put it?)
