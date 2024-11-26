{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
  compressor ? "none",
}:
let
  lib = pkgs.lib;

  runner = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "amaurot-processor";

  inherit compressor;

  config = {
    Entrypoint = [
      (lib.getExe runner.dotnet-runtime)
      "${runner}/lib/amaurot-processor/Amaurot.Processor.dll"
    ];

    env = [
      "PATH=${pkgs.git}/bin:${pkgs.openssh}/bin"
      "TOFU_PATH=${lib.getExe pkgs.opentofu}"
    ];
  };

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
