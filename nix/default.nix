{
  lib,
  buildDotnetModule,
  dotnetCorePackages,
}:
let
  fs = lib.fileset;
  sourceFiles = fs.difference (fs.gitTracked ../.) (
    fs.unions [
      (fs.maybeMissing ./result)
      ../default.nix
      (fs.fileFilter (file: file.hasExt "nix") ./.)
      ./npins
    ]
  );
in
buildDotnetModule {
  pname = "amaurot";
  version = "0.0.1";

  src = fs.toSource {
    fileset = sourceFiles;

    root = ../.;
  };

  projectFile = "Amaurot.sln";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_8_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

  executables = [ ];

  enableParallelBuilding = false;
}
