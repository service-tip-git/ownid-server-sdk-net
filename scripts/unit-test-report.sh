#/Users/c5306486/.dotnet/dotnet 
rm -rf ./TestResults/*
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults" /p:CollectCoverage=true /p:Threshold=18 /p:ThresholdStat=total
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:TestResults/report"