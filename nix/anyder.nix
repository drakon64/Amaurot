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
  name = "anyder";

  inherit compressor;

  config.command = [
    (pkgs.lib.getExe amaurot.dotnet-runtime)
  ];

  tag = "latest";
}
