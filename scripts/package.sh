set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./Aix.XxlJobExecutor.sln
dotnet build ./Aix.XxlJobExecutor.sln -c Release


dotnet pack ./src/Aix.XxlJobExecutor/Aix.XxlJobExecutor.csproj -c Release -o $artifactsFolder
