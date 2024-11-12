{
  pkgs ?
    let
      npins = import ./npins;
    in
    (import npins.nixpkgs { }).pkgsMusl,
  compressor ? "none",
}:
let
  amaurot = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "ktisis";

  inherit compressor;

  config = {
    command = [
      (pkgs.lib.getExe amaurot.dotnet-runtime)
    ];

    env = [ "TOFU_PATH=${pkgs.lib.getExe pkgs.opentofu}" ];
  };

  tag = "latest";
}
