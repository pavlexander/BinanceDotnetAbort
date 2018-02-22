# BinanceDotnetAbort
The repository is to be used for a demonstration of dotnet failure during the runtime.

## Compiling
1. Navigate to TestCrash directory
2. Run command "dotnet publish -c release -r debian-x64"

## Testing
1. Upload contents of "publish" folder to linux environemt. In my case it's debian.
2. On linux navigate to folder contents and run "while sleep 0.3 ; do dotnet TestCrash.dll ; done"
3. You will soon see the "Aborted" message

## Links
https://github.com/dotnet/coreclr/issues/16462

## Notes
The Binance libabry clone was done from tag "v0.2.0-alpha33"
I hold no righs or ownership of Binance libabry code. For latest and up-to-date version please follow  https://github.com/sonvister/Binance



