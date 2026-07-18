{
  pkgs ? import (import ./pomni).nixpkgs { },
  pomni ? pkgs.callPackage (import (import ./pomni).pomni) { },
}:

pkgs.mkShellNoCC {
  packages = with pkgs; [
    dotnetCorePackages.sdk_10_0
    opentofu

    nixfmt
    pomni
  ];

  passthru.pomni = pkgs.mkShellNoCC {
    packages = [ pomni ];
  };
}
