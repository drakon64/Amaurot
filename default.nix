{
  pkgs ?
    let
      npins = import ./npins;
    in
    import npins.nixpkgs { },
}:
{
  nixpkgs = pkgs; # Expose this so that `shell.nix` can use it

  # Run `nix-build -A {{ key }}` to build. For example, `nix-build -A processor`
  processor = pkgs.callPackage ./Amaurot.Processor { };
  receiver = pkgs.callPackage ./Amaurot.Receiver { };

  # Docker images, doesn't work on Darwin (macOS)
  processor-image = pkgs.callPackage ./Amaurot.Processor/docker.nix { };
  receiver-image = pkgs.callPackage ./Amaurot.Receiver/docker.nix { };
}
