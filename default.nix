{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
{
  processor = pkgs.callPackage ./src/Processor/package.nix { };
  receiver = pkgs.callPackage ./src/Receiver/package.nix { };
}
