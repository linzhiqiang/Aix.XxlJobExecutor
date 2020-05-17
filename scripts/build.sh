set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./DotXxlJobExecutor.sln
dotnet build ./DotXxlJobExecutor.sln -c Release
