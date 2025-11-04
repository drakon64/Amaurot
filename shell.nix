{
  pkgs ? import (import ./lon.nix).nixpkgs { },
}:
pkgs.mkShellNoCC {
  packages = with pkgs; [
    dotnetCorePackages.sdk_9_0
    graphviz
    lon
    nixfmt-rfc-style
    opentofu
  ];

  passthru = {
    lon = pkgs.mkShellNoCC {
      packages = [ pkgs.lon ];
    };

    opentofu = pkgs.mkShellNoCC {
      packages = [ pkgs.opentofu ];
    };
  };
}
