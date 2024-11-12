{
  pkgs ?
    let
      npins = import ../npins;
    in
    import npins.nixpkgs { },
  compressor ? "none",
}:
let
  anyder = pkgs.callPackage ./. { };
in
pkgs.dockerTools.buildLayeredImage {
  name = "anyder";

  inherit compressor;

  config.command = [
    (pkgs.lib.getExe anyder.dotnet-runtime)
    "${anyder}/lib/anyder/Anyder.dll"
  ];

  tag = "latest";
}
