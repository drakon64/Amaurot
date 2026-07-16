{
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  stdenv,
  darwin,
}:

buildDotnetModule (finalAttrs: {
  pname = "Amaurot";
  version = "1.0.0";

  src = ./src;

  strictDeps = true;
  __structuredAttrs = true;

  projectFile = "Amaurot.csproj";
  nugetDeps = ./deps.json;

  # Required for Native AOT
  nativeBuildInputs = [ stdenv.cc ];
  buildInputs = lib.optional stdenv.hostPlatform.isDarwin darwin.ICU;
  selfContainedBuild = true;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = null; # No runtime required for Native AOT

  executables = [ "Amaurot" ];

  meta = {
    description = "OpenTofu pull request automation for GitHub";
    homepage = "https://github.com/drakon64/Amaurot";
    license = lib.licenses.eupl12;
    mainProgram = "Amaurot";
    maintainers = with lib.maintainers; [ drakon64 ];
  };
})
