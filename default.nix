{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
pkgs.callPackage ./src/package.nix { }
