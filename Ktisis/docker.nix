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

  ktisis = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "ktisis";

  inherit compressor;

  config = {
    Entrypoint = [
      (lib.getExe ktisis.dotnet-runtime)
      "${ktisis}/lib/ktisis/Ktisis.dll"
    ];

    env = [ "TOFU_PATH=${lib.getExe pkgs.opentofu}" ];
  };

  contents = with pkgs; [ cacert ];

  tag = "latest";
}
