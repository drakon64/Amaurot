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
  name = "amaurot-runner";

  inherit compressor;

  config = {
    Entrypoint = [
      (lib.getExe runner.dotnet-runtime)
      "${runner}/lib/amaurot-runner/Amaurot.Runner.dll"
    ];

    env = [ "TOFU_PATH=${lib.getExe pkgs.opentofu}" ];
  };

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
