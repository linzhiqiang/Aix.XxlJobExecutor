set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder


dotnet build ./src/DotXxlJobExecutor/DotXxlJobExecutor.csproj -c Release

dotnet pack ./src/DotXxlJobExecutor/DotXxlJobExecutor.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/DotXxlJobExecutor.*.nupkg -k $PRIVATE_NUGET_KEY -s https://www.nuget.org
