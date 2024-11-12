{
  buildDotnetModule,
  dotnetCorePackages,
}:
buildDotnetModule {
  pname = "amaurot";
  version = "0.0.1";

  src = ../.;

  projectFile = "Amaurot.sln";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_8_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

  executables = [ ];
}
