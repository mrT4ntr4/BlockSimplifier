# BlockSimplifier
Basic usage of de4dot.blocks that simplifies method blocks

## Usage
```
λ .\BlockSimplifier.exe --help
BlockSimplifier 1.0.0.0
Copyright ©  2023

  -f, --filename    Required. File to deobfuscate

  -t, --tokens      Metadata Tokens of methods to simplify (hex) | eg. -t "0x06000003" "0x06000004"

  -a, --all         Simplify all methods

  --help            Display this help screen.

  --version         Display version information.
```


## Example
```
λ .\BlockSimplifier.exe -f test-guarded_Devirt.exe -t "0x06000003" "0x06000004"
[+] Found Method from Metadata Token (06000003) : System.Void VirtualGuard.Tests.Authenticator::InputPassword(System.String)
[+] Found Method from Metadata Token (06000004) : System.Boolean VirtualGuard.Tests.Authenticator::Validate()
[~] Simplifying System.Void VirtualGuard.Tests.Authenticator::InputPassword(System.String)
[~] Simplifying System.Boolean VirtualGuard.Tests.Authenticator::Validate()
===>> Simplified File saved at : test-guarded_Devirt_Simplified.exe
```

---

## References
https://github.com/de4dot/de4dot